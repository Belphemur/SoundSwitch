import {
  DOWNLOAD_TOTAL_TTL_SECONDS,
  FALLBACK_DOWNLOAD_TOTAL,
  FALLBACK_ERROR_TTL_SECONDS,
  GITHUB_API_BASE,
  REPO_NAME,
  REPO_OWNER,
  USER_AGENT,
  DOWNLOADS_TRACKER_OBJECT_NAME,
} from "./config";
import type { DownloadsPayload, Env, GitHubRelease } from "./types";
import { decorateResponse, formatNumber } from "./http";
import { recordDownloadSnapshot } from "./downloads-tracker";

export async function handleDownloadsRequest(
  env: Env,
  ctx: ExecutionContext,
): Promise<Response> {
  const cache = caches.default;
  const cacheKey = new Request(
    "https://cache.internal/soundswitch/downloads-total/v1",
    { method: "GET" },
  );

  const cached = await cache.match(cacheKey);
  if (cached) {
    return decorateResponse(cached, { cached: true });
  }

  try {
    const total = await fetchTotalDownloads(env);
    const payload: DownloadsPayload = {
      total,
      formatted: formatNumber(total),
      asOf: new Date().toISOString(),
      fallback: false,
    };
    const response = new Response(JSON.stringify(payload), {
      headers: {
        "content-type": "application/json; charset=utf-8",
        "cache-control": `public, max-age=${DOWNLOAD_TOTAL_TTL_SECONDS}, s-maxage=${DOWNLOAD_TOTAL_TTL_SECONDS}`,
      },
    });
    ctx.waitUntil(cache.put(cacheKey, response.clone()));
    ctx.waitUntil(recordDownloadSnapshot(env, total));
    return decorateResponse(response, { cached: false, fallback: false });
  } catch {
    const payload: DownloadsPayload = {
      total: FALLBACK_DOWNLOAD_TOTAL,
      formatted: formatNumber(FALLBACK_DOWNLOAD_TOTAL),
      asOf: new Date().toISOString(),
      fallback: true,
    };
    return new Response(JSON.stringify(payload), {
      status: 200,
      headers: {
        "content-type": "application/json; charset=utf-8",
        "cache-control": `public, max-age=${FALLBACK_ERROR_TTL_SECONDS}, s-maxage=${FALLBACK_ERROR_TTL_SECONDS}`,
        "access-control-allow-origin": "*",
        "x-cache": "MISS",
        "x-fallback": "1",
      },
    });
  }
}

export async function handleDownloadsHistoryRequest(
  env: Env,
): Promise<Response> {
  try {
    const objectId = env.DOWNLOADS_TRACKER.idFromName(
      DOWNLOADS_TRACKER_OBJECT_NAME,
    );
    const objectStub = env.DOWNLOADS_TRACKER.get(objectId);
    const response = await objectStub.fetch("https://do.internal/history");
    if (!response.ok) {
      return new Response(
        JSON.stringify({ error: "Unable to retrieve download history" }),
        {
          status: 502,
          headers: {
            "content-type": "application/json; charset=utf-8",
            "cache-control": "no-store",
            "access-control-allow-origin": "*",
          },
        },
      );
    }
    return decorateResponse(response, { cached: false, fallback: false });
  } catch {
    return new Response(
      JSON.stringify({ error: "Download history unavailable" }),
      {
        status: 502,
        headers: {
          "content-type": "application/json; charset=utf-8",
          "cache-control": "no-store",
          "access-control-allow-origin": "*",
        },
      },
    );
  }
}

export async function recordSnapshotFromResponse(
  response: Response,
  env: Env,
): Promise<void> {
  try {
    const payload = (await response.json()) as {
      total?: unknown;
      fallback?: unknown;
    };
    const total = payload.total;
    const isFallback = Boolean(payload.fallback);
    if (typeof total === "number" && total > 0 && !isFallback) {
      await recordDownloadSnapshot(env, total);
    }
  } catch {
    // Ignore cache payload parsing failures; history updates are best-effort.
  }
}

export async function fetchTotalDownloads(env: Env): Promise<number> {
  let page = 1;
  let total = 0;

  const MAX_PAGES = 20;
  while (page <= MAX_PAGES) {
    const releases = await fetchReleasesPage(env, page);
    if (releases.length === 0) break;

    for (const release of releases) {
      for (const asset of release.assets) {
        total += asset.download_count;
      }
    }

    if (releases.length < 100) break;
    page += 1;
  }

  return total;
}

async function fetchReleasesPage(
  env: Env,
  page: number,
): Promise<GitHubRelease[]> {
  const url = `${GITHUB_API_BASE}/repos/${REPO_OWNER}/${REPO_NAME}/releases?per_page=100&page=${page}`;
  const headers: Record<string, string> = {
    accept: "application/vnd.github+json",
    "user-agent": USER_AGENT,
    "x-github-api-version": "2022-11-28",
  };
  if (env.GITHUB_TOKEN) {
    headers.authorization = `Bearer ${env.GITHUB_TOKEN}`;
  }

  const response = await fetch(url, { headers });
  if (!response.ok) {
    throw new Error(`GitHub API ${response.status} ${response.statusText}`);
  }
  return (await response.json()) as GitHubRelease[];
}

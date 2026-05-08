/**
 * Cloudflare Worker entry for the SoundSwitch website.
 *
 * Responsibilities:
 *  - Serve the dynamic `/api/downloads.json` endpoint that aggregates the
 *    total number of installer downloads across every GitHub release of the
 *    SoundSwitch repository.
 *  - Delegate every other request to the static assets binding so the
 *    VuePress site keeps behaving as before.
 *
 * Caching strategy:
 *  - The aggregated total is stored in the per-edge Cache API for
 *    DOWNLOAD_TOTAL_TTL_SECONDS (6 hours by default). This keeps the
 *    GitHub call rate well below the per-IP / per-token limits regardless
 *    of how many visitors hit the page.
 *  - The optional GITHUB_TOKEN secret raises the GitHub REST limit from
 *    60 req/hour to 5000 req/hour so cold cache misses are safe even
 *    during a traffic spike.
 */

interface Env {
  ASSETS: Fetcher
  GITHUB_TOKEN?: string
}

interface GitHubAsset {
  name: string
  download_count: number
}

interface GitHubRelease {
  tag_name: string
  assets: GitHubAsset[]
}

const DOWNLOAD_TOTAL_TTL_SECONDS = 6 * 60 * 60 // 6 hours
const FALLBACK_ERROR_TTL_SECONDS = 5 * 60 // 5 minutes; cache failures briefly so we don't hammer GitHub during an outage
const GITHUB_API_BASE = 'https://api.github.com'
const REPO_OWNER = 'Belphemur'
const REPO_NAME = 'SoundSwitch'
const USER_AGENT = 'SoundSwitch-Website-Worker'
// Floor used when the GitHub API is unreachable. SoundSwitch had already
// crossed this number well before the dynamic counter shipped, so it stays
// truthful while keeping the home page from flashing a missing value.
const FALLBACK_DOWNLOAD_TOTAL = 3_000_000

export default {
  async fetch(request: Request, env: Env, ctx: ExecutionContext): Promise<Response> {
    const url = new URL(request.url)

    if (url.pathname === '/api/downloads.json') {
      return handleDownloadsRequest(request, env, ctx)
    }

    return env.ASSETS.fetch(request)
  },
} satisfies ExportedHandler<Env>

async function handleDownloadsRequest(
  request: Request,
  env: Env,
  ctx: ExecutionContext,
): Promise<Response> {
  // Reuse the Cache API as a per-edge KV. The key intentionally omits the
  // request URL search params so any visitor benefits from the same warm
  // entry.
  const cache = caches.default
  const cacheKey = new Request(
    `https://cache.internal/soundswitch/downloads-total/v1`,
    { method: 'GET' },
  )

  const cached = await cache.match(cacheKey)
  if (cached) {
    return decorateResponse(cached, { cached: true })
  }

  try {
    const total = await fetchTotalDownloads(env);
    const payload = {
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
    return decorateResponse(response, { cached: false, fallback: false });
  } catch {
    // GitHub unreachable / rate-limited / parsing failure: serve a known
    // floor so the UI never appears broken.
    const payload = {
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

async function fetchTotalDownloads(env: Env): Promise<number> {
  let page = 1
  let total = 0

  // GitHub allows up to 100 results per page. The repository has well under
  // 10 pages of releases, so we cap the loop defensively to avoid runaway
  // requests if the API behaves unexpectedly.
  const MAX_PAGES = 20
  while (page <= MAX_PAGES) {
    const releases = await fetchReleasesPage(env, page)
    if (releases.length === 0) break

    for (const release of releases) {
      for (const asset of release.assets) {
        total += asset.download_count
      }
    }

    if (releases.length < 100) break
    page += 1
  }

  return total
}

async function fetchReleasesPage(env: Env, page: number): Promise<GitHubRelease[]> {
  const url = `${GITHUB_API_BASE}/repos/${REPO_OWNER}/${REPO_NAME}/releases?per_page=100&page=${page}`
  const headers: Record<string, string> = {
    accept: 'application/vnd.github+json',
    'user-agent': USER_AGENT,
    'x-github-api-version': '2022-11-28',
  }
  if (env.GITHUB_TOKEN) {
    headers.authorization = `Bearer ${env.GITHUB_TOKEN}`
  }

  const response = await fetch(url, { headers })
  if (!response.ok) {
    throw new Error(`GitHub API ${response.status} ${response.statusText}`)
  }
  return (await response.json()) as GitHubRelease[]
}

function decorateResponse(
  response: Response,
  opts: { cached: boolean; fallback?: boolean },
): Response {
  const headers = new Headers(response.headers);
  headers.set("access-control-allow-origin", "*");
  headers.set("x-cache", opts.cached ? "HIT" : "MISS");
  if (opts.fallback) headers.set("x-fallback", "1");
  return new Response(response.body, {
    status: response.status,
    statusText: response.statusText,
    headers,
  });
}

function formatNumber(value: number): string {
  return value.toLocaleString('en-US')
}

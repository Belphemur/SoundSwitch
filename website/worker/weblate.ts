import {
  FALLBACK_ERROR_TTL_SECONDS,
  WEBLATE_API_URL,
  WEBLATE_LANGUAGES_TTL_SECONDS,
  USER_AGENT,
} from "./config";
import type { Env } from "./types";
import { decorateResponse } from "./http";

export async function handleWeblateLanguagesRequest(
  env: Env,
  ctx: ExecutionContext,
): Promise<Response> {
  const cache = caches.default;
  const cacheKey = new Request(
    "https://cache.internal/soundswitch/weblate-languages/v1",
    { method: "GET" },
  );

  const cached = await cache.match(cacheKey);
  if (cached) {
    return decorateResponse(cached, { cached: true });
  }

  if (!env.WEBLATE_TOKEN) {
    return new Response(
      JSON.stringify({ error: "WEBLATE_TOKEN secret is not configured" }),
      {
        status: 503,
        headers: {
          "content-type": "application/json; charset=utf-8",
          "cache-control": `public, max-age=${FALLBACK_ERROR_TTL_SECONDS}, s-maxage=${FALLBACK_ERROR_TTL_SECONDS}`,
          "access-control-allow-origin": "*",
          "x-cache": "MISS",
          "x-fallback": "1",
        },
      },
    );
  }

  try {
    const upstream = await fetch(WEBLATE_API_URL, {
      headers: {
        accept: "application/json",
        "user-agent": USER_AGENT,
        authorization: `Token ${env.WEBLATE_TOKEN}`,
      },
    });
    if (!upstream.ok) {
      throw new Error(`Weblate API ${upstream.status} ${upstream.statusText}`);
    }
    const body = await upstream.text();
    const response = new Response(body, {
      headers: {
        "content-type": "application/json; charset=utf-8",
        "cache-control": `public, max-age=${WEBLATE_LANGUAGES_TTL_SECONDS}, s-maxage=${WEBLATE_LANGUAGES_TTL_SECONDS}`,
      },
    });
    ctx.waitUntil(cache.put(cacheKey, response.clone()));
    return decorateResponse(response, { cached: false, fallback: false });
  } catch {
    return new Response(JSON.stringify({ error: "Weblate API unavailable" }), {
      status: 502,
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

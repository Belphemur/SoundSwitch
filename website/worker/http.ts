export function decorateResponse(
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

export function formatNumber(value: number): string {
  return value.toLocaleString("en-US");
}

export interface Env {
  ASSETS: Fetcher;
  DOWNLOADS_TRACKER: DurableObjectNamespace;
  GITHUB_TOKEN?: string;
  WEBLATE_TOKEN?: string;
  // Donation thank-you flow (PayPal IPN -> Postmark email) — all secrets:
  POSTMARK_SERVER_TOKEN?: string; // required
  POSTMARK_FROM?: string; // required — must be set via `wrangler secret put`
  PAYPAL_MERCHANT_EMAIL?: string; // optional anti-fraud guard (receiver_email)
  // Required KV for idempotent, retry-safe email delivery (keyed by PayPal txn_id)
  IPN_DEDUPE?: KVNamespace;
  // Non-secret donation config (set via wrangler.jsonc `vars`):
  PAYPAL_IPN_MODE?: string; // "live" | "sandbox" (defaults to live in code)
  SITE_HOSTNAME?: string;
}

export interface GitHubAsset {
  name: string;
  download_count: number;
}

export interface GitHubRelease {
  tag_name: string;
  assets: GitHubAsset[];
}

export interface DownloadsPayload {
  total: number;
  formatted?: string;
  asOf?: string;
  fallback?: boolean;
}

export interface DownloadsHistoryPoint {
  date: string;
  total: number;
  formatted?: string;
}

export interface DownloadsHistoryAxisTick {
  value: number;
  formatted?: string;
}

export interface DownloadsHistoryPayload {
  asOf?: string;
  retentionDays?: number;
  history: DownloadsHistoryPoint[];
  yAxisTicks?: DownloadsHistoryAxisTick[];
}

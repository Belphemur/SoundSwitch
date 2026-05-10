export interface Env {
  ASSETS: Fetcher;
  DOWNLOADS_TRACKER: DurableObjectNamespace;
  GITHUB_TOKEN?: string;
  WEBLATE_TOKEN?: string;
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

export interface DownloadsHistoryPayload {
  asOf?: string;
  retentionDays?: number;
  history: DownloadsHistoryPoint[];
}

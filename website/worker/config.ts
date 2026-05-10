export const DOWNLOAD_TOTAL_TTL_SECONDS = 6 * 60 * 60; // 6 hours
export const FALLBACK_ERROR_TTL_SECONDS = 5 * 60; // 5 minutes; cache failures briefly so we don't hammer GitHub during an outage
export const GITHUB_API_BASE = "https://api.github.com";
export const REPO_OWNER = "Belphemur";
export const REPO_NAME = "SoundSwitch";
export const USER_AGENT = "SoundSwitch-Website-Worker";
// Floor used when the GitHub API is unreachable. SoundSwitch had already
// crossed this number well before the dynamic counter shipped, so it stays
// truthful while keeping the home page from flashing a missing value.
export const FALLBACK_DOWNLOAD_TOTAL = 3_000_000;
export const HISTORY_RETENTION_DAYS = 90;
export const DOWNLOADS_TRACKER_OBJECT_NAME = "global-downloads-tracker";

export const WEBLATE_LANGUAGES_TTL_SECONDS = 24 * 60 * 60; // 24 hours
export const WEBLATE_API_URL =
  "https://hosted.weblate.org/api/projects/soundswitch/languages/";

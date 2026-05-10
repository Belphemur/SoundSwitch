import {
  fetchTotalDownloads,
  handleDownloadsHistoryRequest,
  handleDownloadsRequest,
} from "./downloads";
import { DownloadsTracker, recordDownloadSnapshot } from "./downloads-tracker";
import { handleWeblateLanguagesRequest } from "./weblate";
import type { Env } from "./types";

export default {
  async fetch(
    request: Request,
    env: Env,
    ctx: ExecutionContext,
  ): Promise<Response> {
    const url = new URL(request.url);

    if (url.pathname === "/api/downloads.json") {
      return handleDownloadsRequest(env, ctx);
    }

    if (url.pathname === "/api/weblate-languages.json") {
      return handleWeblateLanguagesRequest(env, ctx);
    }

    if (url.pathname === "/api/downloads-history.json") {
      return handleDownloadsHistoryRequest(env);
    }

    return env.ASSETS.fetch(request);
  },

  async scheduled(
    _event: ScheduledEvent,
    env: Env,
    _ctx: ExecutionContext,
  ): Promise<void> {
    try {
      const total = await fetchTotalDownloads(env);
      await recordDownloadSnapshot(env, total);
    } catch {
      // Best-effort job: keep serving user traffic even if GitHub is unavailable.
    }
  },
} satisfies ExportedHandler<Env>;

export { DownloadsTracker };

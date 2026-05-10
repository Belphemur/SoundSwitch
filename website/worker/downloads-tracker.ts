import {
  DOWNLOADS_TRACKER_OBJECT_NAME,
  HISTORY_RETENTION_DAYS,
} from "./config";
import { formatNumber } from "./http";
import type { Env } from "./types";

export class DownloadsTracker {
  constructor(private readonly state: DurableObjectState) {}

  async fetch(request: Request): Promise<Response> {
    const url = new URL(request.url);
    this.ensureSchema();

    if (request.method === "POST" && url.pathname === "/snapshot") {
      return this.handleSnapshot(request);
    }

    if (request.method === "GET" && url.pathname === "/history") {
      return this.handleHistory();
    }

    return new Response("Not found", { status: 404 });
  }

  private ensureSchema(): void {
    this.state.storage.sql.exec(`
      CREATE TABLE IF NOT EXISTS download_history (
        snapshot_date TEXT PRIMARY KEY,
        total INTEGER NOT NULL,
        created_at TEXT NOT NULL,
        updated_at TEXT NOT NULL
      ) STRICT
    `);
  }

  private async handleSnapshot(request: Request): Promise<Response> {
    let payload: { total?: unknown };
    try {
      payload = (await request.json()) as { total?: unknown };
    } catch {
      return new Response(JSON.stringify({ error: "Invalid JSON payload" }), {
        status: 400,
        headers: {
          "content-type": "application/json; charset=utf-8",
        },
      });
    }

    const total = payload.total;
    if (typeof total !== "number" || !Number.isFinite(total) || total <= 0) {
      return new Response(
        JSON.stringify({ error: "Field 'total' must be > 0" }),
        {
          status: 400,
          headers: {
            "content-type": "application/json; charset=utf-8",
          },
        },
      );
    }

    const now = new Date();
    const day = now.toISOString().slice(0, 10);
    const nowIso = now.toISOString();

    this.state.storage.sql.exec(
      `
        INSERT INTO download_history (snapshot_date, total, created_at, updated_at)
        VALUES (?, ?, ?, ?)
        ON CONFLICT(snapshot_date) DO UPDATE SET
          total = excluded.total,
          updated_at = excluded.updated_at
      `,
      day,
      Math.floor(total),
      nowIso,
      nowIso,
    );

    const cutoff = new Date(now);
    cutoff.setUTCDate(cutoff.getUTCDate() - HISTORY_RETENTION_DAYS);
    const cutoffDay = cutoff.toISOString().slice(0, 10);

    this.state.storage.sql.exec(
      `DELETE FROM download_history WHERE snapshot_date < ?`,
      cutoffDay,
    );

    return new Response(JSON.stringify({ ok: true }), {
      headers: {
        "content-type": "application/json; charset=utf-8",
      },
    });
  }

  private async handleHistory(): Promise<Response> {
    const rows = this.state.storage.sql.exec(
      `
        SELECT snapshot_date, total
        FROM download_history
        ORDER BY snapshot_date ASC
      `,
    );

    const history = Array.from(
      rows as Iterable<{ snapshot_date: string; total: number }>,
      (row) => ({
        date: row.snapshot_date,
        total: row.total,
        formatted: formatNumber(row.total),
      }),
    );

    return new Response(
      JSON.stringify({
        asOf: new Date().toISOString(),
        retentionDays: HISTORY_RETENTION_DAYS,
        history,
      }),
      {
        headers: {
          "content-type": "application/json; charset=utf-8",
          "cache-control": "public, max-age=300, s-maxage=300",
        },
      },
    );
  }
}

export async function recordDownloadSnapshot(
  env: Env,
  total: number,
): Promise<void> {
  if (!Number.isFinite(total) || total <= 0) {
    return;
  }

  const objectId = env.DOWNLOADS_TRACKER.idFromName(
    DOWNLOADS_TRACKER_OBJECT_NAME,
  );
  const objectStub = env.DOWNLOADS_TRACKER.get(objectId);
  const request = new Request("https://do.internal/snapshot", {
    method: "POST",
    headers: {
      "content-type": "application/json",
    },
    body: JSON.stringify({ total: Math.floor(total) }),
  });

  const response = await objectStub.fetch(request);
  if (!response.ok) {
    throw new Error("Failed to store download snapshot");
  }
}

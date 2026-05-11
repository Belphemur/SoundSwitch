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
    const yAxisTicks = buildYAxisTicks(history.map((point) => point.total));

    return new Response(
      JSON.stringify({
        asOf: new Date().toISOString(),
        retentionDays: HISTORY_RETENTION_DAYS,
        history,
        yAxisTicks,
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

function buildYAxisTicks(totals: number[]): Array<{ value: number; formatted: string }> {
  const SINGLE_VALUE_PADDING_PERCENT = 0.02;
  const MIN_SINGLE_VALUE_PADDING = 10_000;

  if (totals.length === 0) {
    return [];
  }

  let min = Math.min(...totals);
  let max = Math.max(...totals);

  if (min === max) {
    const padded = Math.max(
      MIN_SINGLE_VALUE_PADDING,
      Math.round(min * SINGLE_VALUE_PADDING_PERCENT),
    );
    min = Math.max(0, min - padded);
    max = max + padded;
  }

  const desiredTickCount = 4;
  const rawStep = Math.max(1, (max - min) / Math.max(1, desiredTickCount - 1));
  const step = chooseNiceStep(rawStep);
  const start = Math.floor(min / step) * step;
  const end = Math.ceil(max / step) * step;
  const maxInclusiveRoundingTolerance = step / 2;

  const ticks: Array<{ value: number; formatted: string }> = [];
  // Include the upper boundary tick even when floating-point rounding drifts.
  for (let value = start; value <= end + maxInclusiveRoundingTolerance; value += step) {
    ticks.push({
      value,
      formatted: formatNumber(value),
    });
  }

  return ticks;
}

function chooseNiceStep(rawStep: number): number {
  const power = Math.pow(10, Math.floor(Math.log10(rawStep)));
  const normalized = rawStep / power;

  if (normalized <= 1) {
    return power;
  }

  if (normalized <= 2) {
    return 2 * power;
  }

  if (normalized <= 5) {
    return 5 * power;
  }

  return 10 * power;
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

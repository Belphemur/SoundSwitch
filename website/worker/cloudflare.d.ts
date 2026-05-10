declare interface CacheStorage {
  default: Cache;
}

declare interface Fetcher {
  fetch(input: RequestInfo | URL, init?: RequestInit): Promise<Response>;
}

declare interface ExportedHandler<Env = unknown> {
  fetch?: (
    request: Request,
    env: Env,
    ctx: ExecutionContext,
  ) => Promise<Response>;
  scheduled?: (
    event: ScheduledEvent,
    env: Env,
    ctx: ExecutionContext,
  ) => Promise<void>;
}

declare interface ExecutionContext {
  waitUntil(promise: Promise<unknown>): void;
}

declare interface ScheduledEvent {
  cron: string;
  scheduledTime: number;
}

declare interface DurableObjectNamespace {
  idFromName(name: string): DurableObjectId;
  get(id: DurableObjectId): DurableObjectStub;
}

declare interface DurableObjectId {}

declare interface DurableObjectStub {
  fetch(input: RequestInfo | URL, init?: RequestInit): Promise<Response>;
}

declare interface DurableObjectState {
  storage: {
    sql: {
      exec<T = unknown>(query: string, ...bindings: unknown[]): Iterable<T>;
    };
  };
}

export {};

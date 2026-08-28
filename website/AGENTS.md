# Website Agents

## Scope

This file defines guidance for the `website/` directory. It overrides or
extends the root `AGENTS.md` for all work done inside this folder.

## Technology

- **Framework**: [VuePress 2](https://v2.vuepress.vuejs.org/) with
  [vuepress-theme-hope](https://theme-hope.vuejs.press/)
- **Bundler**: Vite (`@vuepress/bundler-vite`)
- **Language**: TypeScript + Vue 3 Composition API (`<script setup lang="ts">`)
- **Styling**: Scoped `<style scoped>` blocks; global styles via
  `src/.vuepress/styles/`
- **Deployment**: Cloudflare Workers via Wrangler (`wrangler.jsonc`)

## Project Structure

```
website/
├── src/
│   ├── .vuepress/
│   │   ├── components/   # Global Vue components registered in client.ts
│   │   ├── config.ts     # VuePress site & theme configuration
│   │   ├── client.ts     # Client-side app enhancement (component registration)
│   │   └── styles/       # Global SCSS overrides
│   └── **/*.md           # Documentation pages
├── scripts/
│   └── sync-changelog.mjs  # Copies root CHANGELOG.md → src/changelog.md
├── worker/               # Cloudflare Worker entry point
├── package.json
└── wrangler.jsonc
```

## Development Rules

- All Vue components live in `src/.vuepress/components/` and must be registered
  globally in `src/.vuepress/client.ts`.
- Use Vue 3 Composition API with `<script setup lang="ts">` for all components.
- Use `withDefaults(defineProps<...>(), {...})` to expose configurable props
  with sensible defaults.
- Use `useRoute()` from `vue-router` (bundled with VuePress) when components
  need to react to client-side navigation.
- Wrap DOM-dependent initialization in `nextTick()` to ensure the element is
  rendered before accessing it.
- Avoid hardcoding values that could reasonably vary per usage — extract them
  as props with defaults.
- Keep user-visible strings in the Markdown pages; avoid embedding prose in
  Vue component templates.

## Validation

```bash
cd website
npm ci                  # install dependencies
npm run docs:build      # changelog sync + VuePress build — must pass cleanly
```

Run `npm run docs:build` after every change to the website. A successful build
is the minimum bar before pushing.

## Commits

Follow the same conventional-commit conventions as the root `AGENTS.md`.
Use `website` as the scope when changes are limited to this directory,
for example `fix(website): ...` or `feat(website): ...`.

## Donation thank-you flow

The donate module (the `HomeHero.vue` modal + `paypal.com/donate` links) is kept.
Donations use **PayPal IPN** (Instant Payment Notification), which works on a
**Personal** PayPal account (no Business account / REST app needed). The donate
links carry `notify_url=https://soundswitch.aaflalo.me/api/paypal-ipn`; after a
payment completes, PayPal POSTs the IPN to our Worker, which **verifies it** by
echoing it back to PayPal (`cmd=_notify-validate`) and then sends a personalized
HTML thank-you email through **Postmark** (`worker/donations.ts`). See the
`soundswitch-web` skill for the full setup.

- Worker route: `POST /api/paypal-ipn` (live: `https://soundswitch.aaflalo.me/api/paypal-ipn`).
- The IPN **endpoint URL is set on each PayPal donate link** via `notify_url`
  (in `HomeHero.vue`), not in the PayPal Dashboard. No PayPal Developer app is
  required.
- Secrets (set via `wrangler secret put`, never in `wrangler.jsonc`):
  - `POSTMARK_SERVER_TOKEN` — **required** (Postmark Server API key).
  - `POSTMARK_FROM` — **required** (verified sender, e.g. `ss-donation-no-reply@aaflalo.me`).
    It is NOT a `vars` value; if it's missing the Worker returns `503` and sends no email.
  - `PAYPAL_MERCHANT_EMAIL` — optional anti-fraud guard (rejects IPNs to other receivers).
- Non-secret `vars` in `wrangler.jsonc`: `SITE_HOSTNAME`, `PAYPAL_IPN_MODE` (defaults to `live`).
- `IPN_DEDUPE` (KV) is **required** for idempotent, retry-safe delivery (dedupes by `txn_id`,
  7-day TTL); the Worker refuses to process without it.
- The VuePress site build (`docs:build`) does NOT type-check the Worker; always run
  `wrangler deploy --dry-run` to validate Worker changes.

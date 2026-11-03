# Website: mobile horizontal scroll + restore search

## Problem

Two regressions on the SoundSwitch docs site (https://soundswitch.aaflalo.me):

1. **Search is gone** — the slimsearch box is not rendered in the navbar.
2. **Mobile horizontal scroll** — on a 390px-wide mobile viewport the page pans
   72px horizontally; the fixed back-to-top button appears misoverlayed while
   the page slides under it.

## Root causes (verified by source + Playwright probes)

### Search: commit `eac4bc3b` dropped the slimsearch plugin

Renovate PR #2483 bumped `vuepress-theme-hope` to rc.110 and removed
`@vuepress/plugin-slimsearch` from `website/package.json` as "redundant".
It is not redundant: theme-hope rc.110 declares the plugin only as an
**optional peer dependency** (`peerDependenciesMeta.optional: true`), so npm
never installs it, the lockfile has no slimsearch entry, and
`config.ts`'s `theme: ['search', 'slimsearch', …]` option is silently ignored.
The deployed bundle (`app-_RNpoRCS.js`) contains no `SearchBox`/`SearchModal`
client code — confirmed against the live site and a local build.

### Mobile scroll: content-box overflow in two components

Playwright probe (iPhone UA, 390×844, isMobile, WebKit) against the live site:

```
vw=390  documentElement.scrollWidth=462  scrollXMax=72px   (WebKit only;
Chromium emulation hides it because its vw follows the doc width)

.home-wrapper    left=-8  right=398  w=406   (8px past each edge)
.home-hero       left=-8  right=398  w=406
.features-section left=-8 right=398  w=406
.google-ad       left=-8  right=462  w=470   <-- the 72px overflow
```

- `components/GoogleAd.vue` styles `.google-ad` with `width: 100%` (406px) +
  `padding: 0 2rem` (64px) under **content-box** sizing → 470px, 72px past the
  viewport.
- `components/HomeHero.vue` styles `.home-wrapper` with `margin: -2rem -2rem 0`;
  the -32px horizontal margin over-pulls the parent's 24px mobile padding,
  leaving 8px bleed past each viewport edge.

The back-to-top button is **not** a cause: it is `position: fixed`,
`z-index: 100`, right edge at 374 < 390 — fully inside the viewport.

## Changes

- `website/package.json`: re-add `@vuepress/plugin-slimsearch@2.0.0-rc.134`
  (rc.137 breaks the build on Node 22 via `@vuepress/search-helper` CSS ESM
  loading; rc.134 matches the other pinned vuepress plugins).
- `components/GoogleAd.vue`: `box-sizing: border-box` on `.google-ad` so the
  padding is included in the width; drop the redundant `min-width: 100%`.
- `components/HomeHero.vue`: remove the horizontal `-2rem` negative margin from
  `.home-wrapper` (vertical bleed kept).

## Verification

- `npm run docs:build` green (4.9s); `SearchModal`/slimsearch present in
  `dist/assets/app-*.js`.
- Playwright 390×844 (isMobile, touch): after the fix `scrollWidth=390`,
  `scrollXMax=0`, zero overflowing elements, back-to-top button inside the
  viewport, `BUTTON.slimsearch-button aria="Search"` present in the navbar.
- PR preview deployed via `wrangler versions upload` (production untouched).

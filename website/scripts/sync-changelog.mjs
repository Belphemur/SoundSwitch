#!/usr/bin/env node
// Copies the repo-root CHANGELOG.md into website/src/changelog.md so VuePress
// can include it as a normal page. We avoid a symlink because Vite/Rolldown
// resolves symlinks to their real path, which then sits outside the website
// node_modules tree and breaks "vue" import resolution during the build.

import { copyFileSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const source = resolve(here, "../../CHANGELOG.md");
const target = resolve(here, "../src/changelog.md");

const header = `---
title: Changelog
icon: clock-rotate-left
---

<!-- This file is generated from CHANGELOG.md at the repository root. -->
<!-- Do not edit by hand; run \`npm run sync:changelog\` (or any docs script) instead. -->

`;

const body = readFileSync(source, "utf8");
mkdirSync(dirname(target), { recursive: true });
writeFileSync(target, header + body, "utf8");

console.log(`[sync-changelog] wrote ${target}`);

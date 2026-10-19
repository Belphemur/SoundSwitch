#!/usr/bin/env node
// Generates website/src/contributors.md from the repo-root .all-contributorsrc, the
// same way sync-changelog.mjs generates src/changelog.md from CHANGELOG.md.
//
// The root file stays the single source of truth: it is what the all-contributors
// CLI writes to and what the README table is generated from. Copying rather than
// symlinking mirrors the changelog script, because Vite/Rolldown resolves
// symlinks to their real path, which then sits outside the website node_modules
// tree and breaks "vue" import resolution during the build.

import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const source = resolve(here, "../../.all-contributorsrc");
const target = resolve(here, "../src/contributors.md");

const ICONS = {
  code: "💻",
  doc: "📖",
  design: "🎨",
  translation: "🌍",
  test: "⚠️",
  security: "🛡️",
  maintenance: "🚧",
  question: "🐛",
  userTesting: "✅",
  blog: "📝",
  research: "🔬",
};

const LABELS = {
  code: "Code",
  doc: "Documentation",
  design: "Design",
  translation: "Translation",
  test: "Tests",
  security: "Security",
  maintenance: "Maintenance",
  question: "Answered questions",
  userTesting: "User testing",
  blog: "Blogposts",
  research: "Research",
};

/** GitHub avatar URLs already carry a `?v=4`; add the size parameter to it. */
const sizedAvatar = (url) => (url.includes("?") ? `${url}&s=160` : `${url}?s=160`);

const escapeHtml = (value) =>
  String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;");

const renderContributions = (contributions) =>
  (contributions ?? [])
    .map((type) => {
      const icon = ICONS[type] ?? "•";
      const label = LABELS[type] ?? type;
      return `<span class="badge" title="${escapeHtml(label)}">${icon}</span>`;
    })
    .join("\n            ");

const renderCard = (contributor) => `      <a class="card" href="${escapeHtml(contributor.profile)}" target="_blank" rel="noopener noreferrer">
        <img src="${escapeHtml(sizedAvatar(contributor.avatar_url))}" width="80" height="80" alt="${escapeHtml(contributor.name)}" loading="lazy" />
        <span class="name">${escapeHtml(contributor.name)}</span>
        <span class="badges">
            ${renderContributions(contributor.contributions)}
        </span>
      </a>`;

const { contributors } = JSON.parse(readFileSync(source, "utf8"));
const cards = contributors.map(renderCard).join("\n");

const header = `---
title: Contributors
icon: users
---

<!-- This file is generated from .all-contributorsrc at the repository root. -->
<!-- Do not edit by hand; run \`npm run sync:contributors\` (or any docs script) instead. -->

# Contributors

SoundSwitch is built and maintained by volunteers in their spare time. These are
the people who have contributed code, documentation, translations, designs,
security research and bug reports to the project.

Thanks to all of them, and to everyone who has filed an issue, reported a bug or
translated a string.
`;

const body = `
## People

<div class="contributors">
${cards}
</div>
`;

const styles = `
<style>
.contributors {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(9rem, 1fr));
  gap: 1.25rem;
  margin: 1.5rem 0;
}
.contributors .card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.4rem;
  padding: 1rem 0.5rem;
  border: 1px solid var(--vp-c-divider);
  border-radius: 8px;
  text-align: center;
  color: inherit;
  text-decoration: none;
  transition: border-color 0.2s, transform 0.2s;
}
.contributors .card:hover {
  border-color: var(--vp-c-brand-1);
  transform: translateY(-2px);
}
.contributors .card img {
  border-radius: 50%;
}
.contributors .name {
  font-weight: 600;
  font-size: 0.9rem;
  word-break: break-word;
}
.contributors .badges {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 0.15rem;
  font-size: 0.85rem;
}
</style>
`;

mkdirSync(dirname(target), { recursive: true });
writeFileSync(target, header + body + styles, "utf8");

console.log(`[sync-contributors] wrote ${target} (${contributors.length} contributors)`);
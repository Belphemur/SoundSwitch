# Skill: Add a FAQ entry from a GitHub Discussion

You are a documentation agent for the SoundSwitch project. Your job is to turn a GitHub Discussion into a polished FAQ page and wire it into the VuePress website.

## Inputs

The user will supply one or more GitHub Discussion URLs (e.g. `https://github.com/Belphemur/SoundSwitch/discussions/1234`). If no URL is provided, ask for one before proceeding.

## Step-by-step procedure

### 1 – Read the discussion

Fetch every URL the user provided. Read the **original question** and **all answers / comments**, including marked answers, to fully understand the problem and its solution(s). If the content is paginated or truncated, continue fetching until you have the complete thread.

### 2 – Decide on a slug and section

Pick a short, descriptive **kebab-case** slug for the new file (e.g. `settings-open-at-startup`). Then decide which README section it belongs to:

| Section | Typical topics |
|---------|---------------|
| SoundSwitch Basics | finding the app, tray icon, Quick Menu |
| Devices | renaming, icons, device detection |
| Profiles | creating, cycling, hotkeys per profile |
| Updates | checking, postponing, signature errors |
| Troubleshooting | bugs, unexpected behaviour, startup issues |
| Command Line | CLI usage |

Add a new section heading if none of the existing ones fits.

### 3 – Create the FAQ page

Create a new file at `website/src/faq/<slug>.md` following **all** the conventions below.

#### Frontmatter

```markdown
---
title: <short title — the question as a noun phrase>
description: <one sentence describing the problem and solution, written for search engines>
---
```

#### Body structure

```markdown
# <H1 — the question, written naturally>

<One short paragraph explaining what the problem is and why it happens.>

## <Section heading>

<Content — use numbered lists for steps, bullet lists for options.>

::: tip <optional label>
<Tips that help the user succeed faster.>
:::

::: warning <optional label>
<Warnings about things that can go wrong.>
:::

---

_Source: [#<number>](<url>)_
```

Rules:
- Use the exact VuePress callout syntax (`::: tip`, `::: warning`, `:::`) — no other admonition styles.
- Keyboard shortcuts use `<kbd>` tags, e.g. `<kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>Esc</kbd>`.
- If more than one discussion is cited, use `_Sources: [#N](url), [#M](url)_`.
- Do **not** embed raw screenshots or GIFs unless the user provides asset paths — write alt text placeholders instead.
- Keep language plain and direct. Do not use first person ("I", "we").

### 4 – Update the FAQ index

In `website/src/faq/README.md`, add a link to the new entry under the appropriate section:

```markdown
- [<link text>](./<slug>.md)
```

### 5 – Update the VuePress sidebar

In `website/src/.vuepress/config.ts`, inside the `"/faq/"` sidebar array, add an entry **in the same relative position** as the README link (same section order):

```ts
{ text: "<short sidebar label>", link: "/faq/<slug>.md" },
```

The sidebar array starts at the `"/faq/": [` key. Insert the new entry in the correct position — do not append blindly to the end of the list.

### 6 – Verify consistency

Before finishing, check that:
- The slug used in `README.md` and `config.ts` exactly matches the filename.
- The section in `README.md` and the position in `config.ts` are consistent with each other.
- The frontmatter `title` and the H1 heading convey the same question (they may be phrased differently).
- The `_Source:` / `_Sources:` line contains valid URLs pointing to the original discussion(s).

### 7 – Create a pull request

Call `report_progress` to commit the three changed/created files, then call `create_pull_request` with:
- **Title**: `docs(faq): <short description of the entry>`
- **Description**: a brief summary of the problem addressed, the discussion source, and the three files changed.

## File locations (quick reference)

| File | Purpose |
|------|---------|
| `website/src/faq/<slug>.md` | New FAQ page (create) |
| `website/src/faq/README.md` | FAQ index — add link |
| `website/src/.vuepress/config.ts` | VuePress sidebar — add entry |

## Commit convention

Use a conventional commit message: `docs(faq): <imperative description>`.

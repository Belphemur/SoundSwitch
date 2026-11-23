<!-- SoundSwitch Pull Request Template
     The PR validator workflow (.github/workflows/pr-validator.yml) checks for
     the exact section headers below. Do not rename or reword them. -->

## 🎯 Summary (Human-Readable) — for the maintainer

<!-- One or two sentences each. No jargon dumps. This is the part Antoine reads. -->

**Why:**

**How:**

---

## 🤖 AI Disclosure

**Was this PR written (fully or partially) by an AI agent?** <!-- Yes / No -->

<!-- If Yes, fill in every field below. If No, delete everything from here to the next section. -->

| Field | Value |
|---|---|
| Agent / Framework | <!-- Hermes, pi (pi-supervisor), OpenCode, Copilot, Cursor, Claude Code, … --> |
| Model | <!-- e.g. glm-5.3-flash, claude-sonnet-4, gpt-5 … --> |
| Provider | <!-- e.g. OpenRouter, Anthropic, Nous … (omit if n/a) --> |
| Scope of AI involvement | <!-- Full implementation / partial (human wrote X) / diagnosis only / review only --> |
| Human review | <!-- Who reviewed the output and how (self, other agent, human pass) --> |

### Full Technical Explanation (AI-Authored)

<!-- Written by the agent: full design rationale, files touched, edge cases, risks. -->

### Verification

- [ ] Build passes (`dotnet build SoundSwitch.sln -c Debug`)
- AI-proposed changes were reviewed by a human before merge.

---

## ✅ Checklist

- [ ] **PR targets the `dev` branch** (the repo's default branch; `main` is release-only)
- [ ] Conventional commit subject (`feat`/`fix`/`perf`/`lang`/`boost`/`tests`)
- [ ] User-facing strings live in localization resources, not hardcoded literals
- [ ] Docs updated in the same change if user-visible (`website/src/**/*.md`, screenshots in `img/Documentation/**`)
- [ ] ADR under `docs/design/` exists if this is a new/reworked feature or cross-project change
- [ ] Squash-merge title is a user-facing changelog line

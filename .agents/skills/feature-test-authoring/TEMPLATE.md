# TEMPLATE.md — starting point for a generated `<feature>-testing` skill

Copy this file to `.agents/skills/<feature>-testing/SKILL.md`, replace every
placeholder, keep the table shape (it lets a reviewer pick a command in
seconds), and delete sections that genuinely do not apply. Keep the terse,
imperative style — the skill is a routing table, not an essay.

Placeholders: `<Feature>`, `<FeaturePath>` (code under `SoundSwitch/`),
`<feature>`, `<AutomationId>`, `<TabName>`.

The copied file **starts at the frontmatter opener** — the `---` line that
opens the YAML block below. Delete this preamble; do not copy it.

---
name: <Feature> testing
description: Test the <feature> feature — unit fixtures, a live-app control script, and an end-to-end test that drives the real Settings UI and CLI. Use this when working on <one-line>, <one-line>, or reviewing such changes.
---

# <Feature> testing

<One paragraph: what the user-visible feature does, in the same words the docs
use, split into numbered sub-behaviours below.>

Testing has three layers. Use the cheapest layer that can catch your change,
following the layer decision in the `feature-test-authoring` skill:
run the end-to-end layer before shipping anything that touches the feature's
runtime behaviour — skip it entirely for a pure-logic feature (parsing,
formatting, math) and delete the Layer 3 section here.

| Layer | Command | Time | Needs a running app | Needs real devices |
| --- | --- | --- | --- | --- |
| Unit tests | `pwsh tools\Test-<Feature>.ps1` | ~10 s | no | no |
| Live control | `pwsh tools\Control-SoundSwitch.ps1 ...` | seconds | yes (script can launch) | optional |
| End-to-end | `pwsh tools\Test-<Feature>EndToEnd.ps1` | ~1 min | yes (script launches) | yes |

## What the feature does

1. **<Sub-behaviour>.** <Where it lives: `SoundSwitch/<FeaturePath>`, what is
   persisted, how it is triggered.>
2. **<Sub-behaviour>.** <...>

## Layer 1 — unit tests

```powershell
pwsh tools\Test-<Feature>.ps1          # feature fixtures (~2 s after build)
pwsh tools\Test-<Feature>.ps1 -List    # list the fixtures
pwsh tools\Test-<Feature>.ps1 -All     # full SoundSwitch.Tests suite
pwsh tools\Test-<Feature>.ps1 -NoBuild # skip the build
```

<Any fixture or test excluded from the runner and why — must name test filters
that apply on every run mode, -All included.>

| Area | Fixture |
| --- | --- |
| <Area> | `<FixtureName>` |

## Layer 2 — control a live app

<What `tools/Control-SoundSwitch.ps1` gives you for this feature: CLI command,
tab, automation id of the control, screenshot. Only list what is real.>

```powershell
pwsh tools\Control-SoundSwitch.ps1 -Tab <TabName> -Screenshot shot
pwsh tools\Control-SoundSwitch.ps1 -Cli '<cli command>'
```

Automation ids (from `Settings.Designer.cs` / programmatic `Name = "..."`):
`<AutomationId>` — <what it is>. Names are localized; automation ids are not.

## Layer 3 — end-to-end test

```powershell
pwsh tools\Test-<Feature>EndToEnd.ps1
pwsh tools\Test-<Feature>EndToEnd.ps1 -NoBuild   # useful flags only
```

It backs up `%APPDATA%\SoundSwitch\SoundSwitchConfiguration.json`, drives the
real UI and CLI, and asserts:

1. <Observable assertion 1>
2. <Observable assertion 2>

<Original configuration is always restored, and the app it launched is stopped,
even when a check fails. State this only if true.>

## Before pushing

- Change under `<FeaturePath>` or <related files>: run
  `pwsh tools\Test-<Feature>.ps1`<and, when the change touches what the feature
  does at runtime, `pwsh tools\Test-<Feature>EndToEnd.ps1`>. For pure-logic
  changes the unit layer alone is the requirement.
- <Change class>: <which layer covers it / what to extend>.
- <User-visible sub-behaviour> changed: also load the `documentation-sync` skill
  — docs and screenshots change in the same PR.

---
name: Documentation sync
description: Keep the user documentation, website pages, and screenshots in sync whenever a user-visible feature, setting, or workflow changes in SoundSwitch. Use this when adding, changing, or removing anything a user sees or does — a new setting, tab, button, default, trigger, or notification behaviour — and after merging such a change that shipped without its docs.
---

# Documentation sync

A change is not finished when it compiles and the tests pass. If it changes what a
user **sees or does**, the matching documentation and screenshots change **in the
same pull request**. Docs drift is an incomplete change, not a follow-up task.

This applies to everything user-visible: a new or renamed setting, a changed
default, a new tab or button, a new trigger type, a new notification style, a
removed option, a moved control, or a new FAQ-worthy behaviour.

## Where the documentation lives

| Surface | Location | Notes |
| --- | --- | --- |
| Website (public docs) | `website/src/**/*.md` | The canonical user-facing documentation. |
| Screenshots | `img/Documentation/**` | Source of truth for every image. |
| Website images | `website/src/.vuepress/public/images` | A **symlink** to `../../../../img/Documentation`, served as `/images/`. Update the PNG once in `img/Documentation/` and both the website and the README pick it up. |
| Root README | `README.md` | Marketing/overview; references `img/preview.gif` and the logo. |
| Developer docs | `docs/**` | Architecture/build/contribution notes — update only when the engineering story changes. |
| FAQ | `website/src/faq/**` | Add entries via the `add-faq-entry` agent flow. |

Do **not** edit files under `website/src/.vuepress/public/images` directly — it is a
symlink; change the real file in `img/Documentation/`.

## Screenshot ↔ page ↔ tab map

| Page | Image(s) | Settings tab |
| --- | --- | --- |
| `website/src/configuration/general.md` | `General.png` | General |
| `website/src/configuration/playback.md` | `Playback.png` | Playback |
| `website/src/configuration/recording.md` | `Recording.png` | Recording |
| `website/src/configuration/notifications.md` | `Notifications.png` | Notifications |
| `website/src/usage/profiles.md` | `Profile.png`, `Profile Add.png` | Profiles (+ profile editor) |
| `website/src/usage/app-rules.md` | `AppRules.png`, `AppRules Add.png` | App Rules (+ rule dialog, `-Click addAppSoundRuleButton`) |
| `website/src/advanced/troubleshooting.md` | `Troubleshoot.png` | Troubleshooting |
| `website/src/faq/**` | `faq/*.png`, `faq/*.gif` | — |

If a screenshot is stale, the text around it almost always is too (and vice versa).
Treat them as one artifact.

## Taking screenshots with automation

Never paste a full-screen grab or a hand-cropped image. Use the automation script,
which builds the app, launches it, opens Settings through the app's IPC/CLI
surface, selects the tab, and captures **only the Settings window bounds**:

```powershell
# From the repository root. Requires PowerShell 7+.
pwsh .\tools\Capture-DocumentationScreenshot.ps1 -Name Notifications -Tab Notifications
pwsh .\tools\Capture-DocumentationScreenshot.ps1 -Name Playback -Tab Playback
pwsh .\tools\Capture-DocumentationScreenshot.ps1 -Name "AppRules Add" -Tab "App Rules" -Click addAppSoundRuleButton
pwsh .\tools\Capture-DocumentationScreenshot.ps1 -Name faq/quick-menu-popup
```

What the script does, and why:

1. Builds `SoundSwitch` and `SoundSwitch.CLI` for the selected configuration.
2. Requires that no other SoundSwitch instance is running (they share one named
   pipe and one config file), then launches the freshly built `SoundSwitch.exe`
   with `--disable-updater`.
3. Opens the Settings window with `SoundSwitch.CLI.exe settings` — the supported
   automation surface — instead of simulating tray clicks.
4. Selects the tab by name with UI Automation (`System.Windows.Automation`) and,
   with `-Click`, invokes a button to open a sub-dialog.
5. Captures the window with `PrintWindow(PW_RENDERFULLCONTENT)` clipped to
   `GetWindowRect`, so the PNG is cropped to the window (title bar included, no
   desktop background) and matches the existing screenshots.
6. Saves to `img/Documentation/<Name>.png` and closes the window/app it opened.

Notes and gotchas:

- The screenshot shows **real devices from the local machine's configuration**.
  Review it before committing; use a representative setup, and never leak a
  personal device name that is not already in the docs.
- The UI follows the Windows light/dark app-mode. Match the theme of the existing
  screenshots for the page you are updating.
- The committed screenshots use the **English** UI. If SoundSwitch is set to another
  language, switch it back (General tab) before capturing, and pass tab names in
  the language currently displayed (`-Tab` matches the localized label).
- Sub-dialogs (add-profile, add-rule, …) are captured with `-Click`: after the tab
  is selected the script clicks the named button and captures the dialog it opens,
  e.g. `-Tab "App Rules" -Click addAppSoundRuleButton`. Pass the control name from
  the designer (`addAppSoundRuleButton`, `addProfileButton`) or its visible label
  (`Add`) when the label is unambiguous on that tab. Dialogs with no launching
  button (the colour picker, the file picker) still need `-AllowExistingInstance`
  and no `-Tab` while the dialog is visible.
- Use `-ListWindows` to see what the script finds, and `-KeepOpen` to inspect the
  result before the window closes.

## Workflow

1. **Detect the impact.** For every user-visible change, list the pages that
   mention the affected control, tab, or behaviour (`grep` the setting name and the
   tab name across `website/src` and `README.md`).
2. **Update the text.** Change the description of the feature and any onboarding
   steps, tables, or option lists. Keep the website's plain, direct tone and the
   existing structure.
3. **Re-capture the screenshots.** Regenerate every PNG/GIF that shows the changed
   area with `tools\Capture-DocumentationScreenshot.ps1`. Do not leave an old image
   next to new text.
4. **Update navigation if needed.** A new page requires a link in the section
   README **and** a sidebar entry in `website/src/.vuepress/config.ts`.
5. **Update the frontmatter `description`** when the page's purpose changed.
6. **Validate.** Run `cd website; npm run docs:build` — it must pass cleanly.
7. **Commit** with a `docs(...)` scope, e.g. `docs(notifications): document banner
   monitor, colour and scale`. When the docs ship with the feature, mention them in
   the feature commit instead.

## Definition of done

- [ ] Every user-visible change is described in the affected page(s).
- [ ] Every affected screenshot was regenerated with the automation script.
- [ ] No stale image sits beside updated text (and no updated text beside a stale image).
- [ ] New pages are linked from the section README and `config.ts`.
- [ ] `npm run docs:build` succeeds.
- [ ] The change ships in the same commit/PR as the feature whenever possible.

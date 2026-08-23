---
title: Telemetry and Privacy
description: What SoundSwitch telemetry collects, why, and how to disable it.
---

# Telemetry and Privacy

SoundSwitch is a free, open-source application. To understand which features are actually used and where to focus development, SoundSwitch can send pseudonymous usage data to the developers.

## What is sent

When telemetry is enabled, SoundSwitch sends the following to [Sentry](https://sentry.io/) (a privacy-focused error and performance monitoring service):

- **Application version and release channel** (Stable, Beta, or Nightly) — so we know which versions are in use.
- **A per-install pseudonymous identifier** — a random GUID generated on first run, not tied to your name, account, or device serial. It is used only to distinguish one installation from another.
- **Feature usage counts** — anonymous counters such as "a playback device switch occurred", "a profile was activated", "a microphone mute toggle happened". No user-chosen text (profile names, device names, file paths) is sent.
- **App Sound Lock (App Rules) usage counts** — anonymous counters for when an App Rule is triggered (the matched process basename is hashed with SHA256, first 8 hex characters, before being sent), created, or deleted. The activation counter also records the `trigger` source (process or foreground window). No process paths, window titles, or App Rule content are sent.
- **Update subsystem usage counts** — anonymous counters for which update mode is configured (`Silent`, `Notify`, or `Never`, emitted when the setting changes and once at startup as a baseline), when a manual "Check for update" is triggered, when a newer release is found and offered, and when an install is attempted or applied (with a categorical result of `success`, `signature_error`, or `failed`). No version numbers, file paths, or other identifiers are sent — only the categorical update mode and outcome.
- **Update-postponed and startup configuration snapshot** — an anonymous counter (`soundswitch.update.postponed`) when an offered update is postponed; and, once at startup, a categorical snapshot of which settings are enabled (for example: include beta versions, language, quick menu, keep volume, switch foreground program, advanced notification mode, auto-add new devices, and tray icon style) plus the counts of configured profiles (`soundswitch.profile.count`) and App Sound Lock rules (`soundswitch.apprule.count`). Only the categorical setting values and counts are sent — no free text, names, or identifiers.
- **Breadcrumbs** — lightweight records of actions like "hotkey pressed" or "settings saved", attached to sessions and used only as context if a crash occurs.
- **Local Windows username** — hashed with SHA256 (first 8 hex characters) and sent as a pseudonymous label on crash reports via Sentry's SDK to help distinguish users during debugging.

**What is NOT sent:**

- Audio device names or device IDs
- Profile names, profile content, or profile rules
- File paths, file names, or media content
- Process paths, window titles, or App Rule content in the telemetry counters — App Rules are represented only by a pseudonymous SHA256 hash of the process basename. Note: a local crash report's breadcrumbs (from application logs) may still include some of these fields, because Sentry.Serilog forwards Debug-level and higher log events; those breadcrumbs are sent only with a crash, not as part of the usage counters.
- Any network identifiers, IP addresses, or location data

### About the Sentry username field

SoundSwitch populates the Sentry username field with the local Windows username. This is only used as a label on crash reports to help distinguish reports from different users on the same machine during debugging.

## Why we collect this

- Knowing which features are used helps prioritize development. If only 2% of users ever activate a profile, that feature may need better discoverability — or may be fine as a niche feature.
- Knowing which notification style people use helps decide what to keep, improve, or remove.
- Crash reports with session context help fix bugs faster.

## Who can see it

- The data is sent to Sentry's servers (see [Sentry's privacy policy](https://sentry.io/privacy/)).
- Only the SoundSwitch developer has access to the project data.
- The data is not shared with third parties for advertising or profiling.

## How to disable telemetry

1. Open SoundSwitch settings (right-click the tray icon → **Settings**).
2. Go to the **General** tab.
3. Uncheck **Telemetry**.

The change takes effect immediately — there is no separate save button. No restart is required. When telemetry is disabled:

- No feature-usage counters are sent.
- Crash reports and their breadcrumbs (which may include limited local context such as process names from application logs) are sent only if the application crashes, independent of this setting. The usage counters described above are not sent.

## Data retention

Sentry retains event data according to its [privacy and data retention policy](https://sentry.io/privacy/). The SoundSwitch project does not extend or modify this retention.

## Contact

If you have questions about telemetry or privacy, open an issue on the [SoundSwitch GitHub repository](https://github.com/Belphemur/SoundSwitch/issues) or contact the developer via [aaflalo.me](https://www.aaflalo.me/contact/).

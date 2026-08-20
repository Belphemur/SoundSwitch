---
title: Telemetry and Privacy
description: What SoundSwitch telemetry collects, why, and how to disable it.
---

# Telemetry and Privacy

SoundSwitch is a free, open-source application. To understand which features are actually used and where to focus development, SoundSwitch can send anonymized usage data to the developers.

## What is sent

When telemetry is enabled, SoundSwitch sends the following to [Sentry](https://sentry.io/) (a privacy-focused error and performance monitoring service):

- **Application version and release channel** (Stable, Beta, or Nightly) — so we know which versions are in use.
- **A per-install anonymous identifier** — a random GUID generated on first run, not tied to your name, account, or device serial. It is used only to distinguish one installation from another.
- **Feature usage counts** — anonymous counters such as "a playback device switch occurred", "a profile was activated", "a microphone mute toggle happened". No user-chosen text (profile names, device names, file paths) is sent.
- **App Sound Lock (App Rules) usage counts** — anonymous counters for when an App Rule is triggered (the matched process basename is hashed with SHA256, first 8 hex characters, before being sent), created, or deleted. No process paths, window titles, or App Rule content are sent.
- **Breadcrumbs** — lightweight records of actions like "hotkey pressed" or "settings saved", attached to sessions and used only as context if a crash occurs.
- **Local Windows username** — sent as a label on crash reports via Sentry's SDK to help distinguish users during debugging.

**What is NOT sent:**

- Audio device names or device IDs
- Profile names, profile content, or profile rules
- File paths, file names, or media content
- Process paths, window titles, or App Rule content (App Rules are counted only via an anonymized SHA256 hash of the process basename)
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
4. Click **Save**.

The change takes effect immediately. No restart is required. When telemetry is disabled:

- No feature-usage counters are sent.
- No breadcrumbs are recorded.
- Session tracking (used for crash reporting context) is also disabled.
- Crash reports are not sent unless the app crashes while telemetry was enabled.

## Data retention

Sentry retains event data according to its [privacy and data retention policy](https://sentry.io/privacy/). The SoundSwitch project does not extend or modify this retention.

## Contact

If you have questions about telemetry or privacy, open an issue on the [SoundSwitch GitHub repository](https://github.com/Belphemur/SoundSwitch/issues) or contact the developer via [aaflalo.me](https://www.aaflalo.me/contact/).

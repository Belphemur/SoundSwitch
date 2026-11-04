---
title: App Rules (App Sound Lock)
description: Route individual applications to specific audio devices in Windows without changing the system default. Choose Automatic mode for hands-free routing or Hotkey mode to cycle device pairs on demand.
---

# App Rules (App Sound Lock)

App Rules — also called **App Sound Lock** — route **individual applications** to specific audio devices without changing the system default. Use it when you want Spotify on your speakers while your game stays on your headset.

Each rule targets an application (and optionally a window title) and owns an ordered list of **device pairs**. A pair contains a playback device, a recording device, or both.

## Two modes

| Mode | What it does | Best for |
|------|--------------|----------|
| **Automatic** | Applies the rule's first device pair whenever the application starts or comes to the foreground. | Hands-free routing that should always be active. |
| **Hotkey** | Does nothing automatically. Pressing the rule's hotkey applies the next device pair to the running application. | Cycling an application between several devices on demand. |

The modes are exclusive. A rule is either Automatic or Hotkey, never both. See [Automatic mode](./automatic.md) and [Hotkey mode](./hotkey.md) for the details. An Automatic rule that targets one concrete application is also registered in Windows' own app audio settings, so the route survives SoundSwitch being closed — see [Windows also remembers it](./automatic.md#windows-also-remembers-it).

## How it works

App Rules use the Windows Audio Session APIs to target specific processes. Only the matched application's audio is redirected; the **system default device remains unchanged**.

Rules are evaluated in two ways:

- **Automatic rules** are evaluated whenever a new process is detected and whenever the foreground window changes.
- **Hotkey rules** are applied only when you press their hotkey. The rule then looks for every running process that matches it.

A rule matches on:

| Field | What it matches | Example |
|-------|-----------------|---------|
| **Process Path** | The process path or name. Matched as a **regular expression**, with a glob fallback (`*chrome*`). | `.*chrome\.exe.*` matches any Chrome process |
| **Window Title** | The title of the application's active window. Also regex with a glob fallback. | `.*YouTube.*` matches any window with "YouTube" in the title |

If a field is left blank, it acts as a wildcard. A rule with only a Process Path matches that process regardless of its window title.

## The App Rules list

The App Rules tab lists every rule with these columns:

| Column | Description |
|--------|-------------|
| **Application** | The process name or path the rule matches. |
| **Window Title** | The window title pattern, or blank for any window. |
| **Playback** | The first pair's playback device. |
| **Recording** | The first pair's recording device. |
| **Mode** | `Automatic` or `Hotkey`. Hotkey rules with several pairs also show the pair count, for example `Hotkey (3)`. |
| **Hotkey** | The rule's global shortcut, for Hotkey rules. |

Hover a row to see every device pair in a tooltip.

## Windows assignments

Above your own rules, the list also shows — dimmed — the applications that Windows itself has routed to a specific device through its own per-app audio settings. SoundSwitch does not control these rows; they come straight from the same per-app store Windows Settings writes to:

- **Read-only.** There is nothing to edit on a Windows assignment row. Change it in Windows' per-app audio settings, not in SoundSwitch.
- **No duplicates.** An application covered by one of your rules shows up only as that rule's row, never again in this section — even when the rule also writes an assignment.
- **Your rules win.** If an application managed by one of your rules is changed from Windows' side, the rule re-applies its pair the next time the application is seen. Deleting the rule is the way to stop managing that application.

## Rule options

| Option | Description |
|--------|-------------|
| **Enabled** | Turns the rule on or off without deleting it. Disabled rules are ignored and their hotkeys are not registered. |
| **Notify when triggered** | Shows a notification when the rule routes an application. Useful while setting a rule up. |
| **Case Sensitive** | Makes the process and window patterns case-sensitive. |

## The process picker

The **Process Path** field has a **...** button. It opens a process picker that lists applications with their process name, window title, current playback and recording devices, and full path. Select an application to pre-fill the pattern, or use the filter box to narrow the list.

Instead of one flat list of every process, the picker groups the entries:

| Group | What it shows |
|-------|---------------|
| **Has own device** | Applications that already have their own audio device assigned — through Windows' per-app device setting or one of your rules. Applications that are not currently running appear here too, dimmed and labelled with a *(not running)* suffix. They are still selectable, so you can create a rule for a closed application. |
| **Using audio now** | Applications that have an active audio session but no device of their own yet. |
| **Show all processes** | The full process list as before. Collapsed by default; click the header row to expand or collapse it. Typing in the filter box expands this group automatically so its matches are visible, and clearing the filter collapses it again. |

The filter box searches all three groups.

![The process picker grouped by device assignment](/images/AppRules%20Process%20Picker.png)

![The process picker while filtering](/images/AppRules%20Process%20Picker%20Search.png)

## Creating a rule

1. Open the **App Rules** tab in SoundSwitch Settings.
2. Click **Add**.
3. Use **...** next to **Process Path** to pick the application, or type a pattern manually.
4. Optionally set a **Window Title** pattern.
5. Choose a **Mode**:
   - **Automatic** — select the playback and/or recording device the application should always use.
   - **Hotkey** — press the shortcut you want, then build the ordered device-pair list. See [Device pairs](./device-pairs.md).
6. Adjust **Enabled**, **Case Sensitive**, and **Notify when triggered** as needed.
7. Click **Save**.

![App Rules list](/images/AppRules.png)

![Add an App Rule in Automatic mode](/images/AppRules%20Add.png)

![Add an App Rule in Hotkey mode](/images/AppRules%20Add%20Hotkey.png)

## Managing rules

- **Edit**: select a rule and click **Edit**, or double-click it.
- **Enable/disable**: use the checkbox in the list. The change is saved immediately and the rule's hotkey is registered or released.
- **Delete**: select a rule and click **Delete**.

## When routing is applied

- **Automatic** rules apply as soon as a matching process starts or becomes the foreground window.
- **Hotkey** rules apply only when you press the shortcut, and only to the matching application's running processes.
- Routing persists while the application is running. If the application restarts, an Automatic rule re-applies; a Hotkey rule waits for the next press.

## Device changes

Rules store a device identity, not just a device name. If a device gets a new id after a driver update, SoundSwitch resolves it by its device-instance path or unique name and updates the stored identity automatically. If a device is unplugged, the affected side of a pair is skipped until the device is available again.

## Resetting per-app routing

If routing gets into a confusing state, open the **Troubleshooting** tab and use **Reset** under **Reset Per App Audio**. This clears every process-level assignment — including the routes Windows remembered for your applications — so everything goes back to the default devices. The reset applies to all applications at once, not just the ones your rules target. Afterwards, an **Automatic** rule applies its device again the next time the application runs (and registers it with Windows again); **Hotkey** rules wait for you to press their shortcut.

## App Rules vs Profiles

> **App Rules** route **individual applications** to specific devices. The system default device does not change.
>
> **Profiles** change the **system default device** when a trigger fires. This affects every application that uses the default device.
>
> See [Profiles vs App Rules](../README.md#profiles-vs-app-rules) for the full comparison.

## Telemetry and privacy

When telemetry is enabled, App Rule activations are counted anonymously. SoundSwitch records only a SHA256 hash of the matched process basename (the first 8 hex characters) and the trigger source. No process path or window title is sent. See [Privacy and Telemetry](/legal/telemetry.md) for details.

## In this section

- [Automatic mode](./automatic.md) — one pair, applied whenever the application matches.
- [Hotkey mode](./hotkey.md) — cycle device pairs with a shortcut.
- [Device pairs](./device-pairs.md) — build and order the playback/recording combinations.
- [Examples](./examples.md) — ready-to-adapt rules for common setups.
- [Troubleshooting](./troubleshooting.md) — when a rule or hotkey does not do what you expect.

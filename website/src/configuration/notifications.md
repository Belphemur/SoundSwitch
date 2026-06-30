---
title: Notification Settings
description: Choose how SoundSwitch alerts you on device changes — banner, Windows toast, sound only or silent — and configure the microphone mute banner.
---

# Notifications

The **Notifications** tab lets you choose how SoundSwitch alerts you when an audio device is switched or when your microphone is muted or unmuted.

![Notifications](/images/Notifications.png)

## Notification Type

The **Notification Type** panel controls which notification method is used and provides an **Advanced...** toggle to reveal additional options.

- **Switch device** — Sets the notification style used when switching audio devices via hotkey. Options include **Banner Notification**, **Windows Toast**, **Sound**, or **None**.
- **Advanced...** (checkbox) — When checked, reveals additional notification controls: **Switch profile** (notification for profile activation) and **Microphone mute** (notification for mute toggle). The panel height expands to accommodate these options.

## Custom Sound File

Click **Select...** to choose an audio file that SoundSwitch plays when a device is switched. Click the delete (×) button to remove the custom sound.

## Banner Options

Banner notifications are designed to **never steal focus** from other applications. They are shown as non-activating windows, so full-screen games and other apps should not minimize or lose focus when a banner appears.

### Exclusive Fullscreen Detection

When a game or application is running in **true exclusive fullscreen** mode (where the game takes exclusive control of the display output and DWM composition is suspended), SoundSwitch automatically switches from banner notifications to **Windows Toast** notifications. In true exclusive fullscreen, no overlay window can appear on screen, so toast is the only way to deliver a notification.

SoundSwitch detects true exclusive fullscreen by checking whether:

- The **display mode has changed** — the current resolution or refresh rate differs from the desktop default, indicating an application has taken exclusive control of the display output.
- The foreground window covers the entire monitor, uses a borderless style, and is set as **topmost** — a secondary signal for older-style FSE games running at native resolution.

**Borderless fullscreen** games (including modern titles like Counter-Strike 2 that use the DXGI flip model) are handled normally with banner notifications. Banners are designed to never steal focus — they use non-activating window styles that do not trigger focus-loss events in the game.

When using banner notifications, you can customize their behavior:

- **Always use primary screen** — Forces the banner to display on your primary monitor instead of the monitor with the currently focused window.
- **Only one banner** — When checked, only one banner is shown at a time. If multiple events occur, they queue rather than stacking.
- **On-screen time** — How long the banner stays visible (in seconds, default is 3).
- **Opacity** — Banner transparency level (0–100%, default is 100).
- **Display Info** — How much detail to show on the banner: **Full Display** (device name and icon), **Name Only**, or **Icon Only**.

## Microphone Mute

Controls the notification behavior when the microphone mute toggle is activated:

- **Microphone On** — Notification style when the microphone is unmuted. Options include **None**, **Fading In**, or **Persistent**.
- **Microphone Off** — Notification style when the microphone is muted. Options include **None**, **Fading In**, or **Persistent**.

## Position

Select where on the screen the banner appears using the 3×3 grid of radio buttons:

| Left        | Center        | Right        |
| ----------- | ------------- | ------------ |
| Top Left    | Top Center    | Top Right    |
| Center Left | Center        | Center Right |
| Bottom Left | Bottom Center | Bottom Right |

A **Custom...** option with a slider lets you drag the banner to any position on your screen for fine-tuned placement.

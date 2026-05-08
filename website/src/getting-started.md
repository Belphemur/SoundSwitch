---
title: Getting Started
description: Install SoundSwitch on Windows 10 or later, set up your first hotkeys, and start cycling between playback and recording devices in under a minute.
---

# Getting Started

## Requirements

- **Operating System**: Windows 10 or newer (x64 or ARM64)
- **.NET**: Requires the .NET runtime (handled automatically by the installer)

## Installation

Grab the installer from the **Download** button at the top of this site and run it. The installer automatically detects and installs the required .NET runtime if it isn't already present on your system.

## First Steps

After installation, SoundSwitch starts in the background and places an icon in the **system tray** (next to the clock). The settings window opens automatically the first time — after that, only the tray icon remains. If you can't see it, check the hidden icons (`^`) on the taskbar and see [Where did SoundSwitch go?](./faq/finding-soundswitch.md).

1. **Right-click** the SoundSwitch tray icon and select **Settings**.
2. On the **Playback** tab, check the devices you want to cycle through (e.g., headphones, speakers).
3. On the **Recording** tab, check the microphones or input devices you want to cycle through.
4. **Close** the settings window — SoundSwitch is ready to use.

## Quick Switch

With SoundSwitch running, use these hotkeys to switch devices instantly:

- **Cycle playback devices**: `Ctrl` + `Alt` + `F11`
- **Cycle recording devices**: `Ctrl` + `Alt` + `F7`
- **Toggle microphone mute**: `Ctrl` + `Alt` + `M`

You can also double-click the tray icon to cycle through playback devices.

## System Tray Actions

The SoundSwitch tray icon supports several actions:

- **Right-click** → Opens the context menu with settings, device selection, and profile options.
- **Double-click** → Cycles to the next audio device (configurable to switch playback, recording, profiles, or open settings).
- **Hover** → Shows the current active device (configurable on the General settings tab).

## Next Steps

Now that SoundSwitch is up and running, dive into the configuration to tailor it to your workflow:

- [General Settings](./configuration/general.md) — Startup, tray behavior, hotkeys, and language.
- [Playback Devices](./configuration/playback.md) — Pick which output devices to cycle through.
- [Recording Devices](./configuration/recording.md) — Pick which input devices to cycle through.
- [Notifications](./configuration/notifications.md) — Customize banners, sounds, and Windows toast notifications.
- [All Configuration Options](./configuration/) — Browse the full configuration reference.

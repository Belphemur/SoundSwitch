---
title: Communication Devices
description: Understand how SoundSwitch switches the Windows Default Communication Device for VoIP and conferencing apps alongside the regular default device.
---

# Communication Devices

SoundSwitch changes the **Default Communication Device** when the "Switch Default Communication Device" option is enabled on the **General** settings tab. It is enabled by default on new installs, so switching devices moves both roles out of the box.

## Why Does This Matter?

Windows differentiates between two types of audio devices:

- **Multimedia device** — The standard default playback/recording device used by most applications.
- **Communication device** — A separate default used by communication apps like Discord, Teams, Skype, and Zoom.

With the option enabled, switching the default playback device also switches the Windows Default Communication Device to the same device, so voice chat apps follow your audio changes.

## How to Disable It

If you prefer communication apps to stay on a fixed device while SoundSwitch switches only the multimedia default:

1. Open SoundSwitch **Settings** (right-click the tray icon → **Settings**).
2. Go to the **General** tab.
3. Uncheck the **Switch Default Communication Device** checkbox.
4. Click **Close**.

Your choice is remembered, including across upgrades.

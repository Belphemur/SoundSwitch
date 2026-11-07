---
title: Behavior and audio settings
description: Startup with Windows, Systray Icon, double-click action, default communication device, foreground app switching, volume keeping, Quick Menu, tooltip on hover and device cycling on the General tab.
---

# Behavior and audio settings

The left column of the **General** tab groups the tray and audio-specific preferences. They cover how SoundSwitch shows itself in the system tray, which extra defaults are switched alongside the main playback device, and how device cycling picks its candidates.

## Basic Settings

### Start automatically with Windows

Registers SoundSwitch to run at login so it is always available to switch devices, enforce profiles, and apply app rules without manual intervention.

### Systray Icon

Determines what icon the SoundSwitch tray icon displays.

| Option | Behavior |
|--------|----------|
| **Default** | Always display the default SoundSwitch icon. |
| **Recording** | Show the icon of the active recording (microphone) device. |
| **Playback** | Show the icon of the active playback (speaker/headphone) device. |
| **Both** | Show the current device icons, alternating between playback and recording. |
| **Theme Based** | Use a light or dark application icon that matches the current Windows taskbar theme. |

### Double-click Action

Sets what happens when you double-click the tray icon.

| Option | Behavior |
|--------|----------|
| **Switch playback device** | Cycles to the next available playback device. |
| **Switch recording device** | Cycles to the next available recording device. |
| **Open settings** | Opens the SoundSwitch settings window. |
| **Switch profile** | Cycles through the configured audio profiles. |
| **Toggle mute microphone** | Mutes or unmutes the configured mute microphone (**Windows default input** by default, or the device chosen in **Mute microphone**). |
| **Disabled** | Disables double-click detection entirely. The device menu opens instantly on a single click with **no delay**. Choose this if you never use the double-click action and want the fastest possible access to the tray menu. |

::: tip Faster tray menu
When a double-click action is configured, SoundSwitch waits up to **250 ms** after each click to decide whether a second click is coming. Setting the action to **Disabled** removes that wait completely, so right-clicking or left-clicking the tray icon opens the menu with zero delay.
:::

## Audio Settings

### Switch Default Communication Device

When enabled, switching the default playback device also switches the Windows **Default Communication Device**. Windows uses the communication device separately from the multimedia device — for example, VoIP apps like Discord or Skype may default to the communication device. Enabling this keeps both in sync when SoundSwitch changes devices.

### Also switch the foreground program

When enabled, changing the default audio device also re-routes the currently focused (foreground) application to the new default device. This ensures the app you are actively using immediately follows the device change.

### Keep volume level across devices

Windows stores volume levels independently per device. When this option is enabled, SoundSwitch preserves the current volume percentage on the source device and applies it to the destination device when switching. This prevents sudden volume jumps when moving between devices with different volume levels.

### Quick Menu on hotkey

Instead of blindly cycling through devices every time you press the hotkey, enabling this option shows a visual selection menu. The hotkey opens the menu; pressing the hotkey again cycles the selection; releasing the hotkey activates the highlighted device.

### Tooltip on Hover

Determines what information appears when you hover over the SoundSwitch tray icon.

| Option | Tooltip shows |
|--------|--------------|
| **Playback Device** | Name of the current default playback device. |
| **Recording Device** | Name of the current default recording device. |
| **Both Devices** | Names of both the current playback and recording devices. |

### Cycle through

Controls which devices are included when cycling through audio devices via hotkey or tray icon double-click.

| Option | Behavior |
|--------|----------|
| **All audio devices** | Cycle through every detected device on the system. |
| **Only selected audio devices** | Cycle through only the devices you have checked on the **Playback** and **Recording** tabs. This is the recommended option for most users. |

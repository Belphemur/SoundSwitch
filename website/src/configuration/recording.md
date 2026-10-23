---
title: Recording Settings
description: Pick which microphones and recording devices SoundSwitch cycles through, choose the microphone muted by the mute hotkey and set the input switch hotkey.
---

# Recording

![Recording tab](/images/Recording.png)

The **Recording** tab is where you choose which audio input devices (microphones, line-in, etc.) SoundSwitch will cycle through.

## Selecting Devices

The main area lists all recording devices detected by Windows, grouped by their connection status:

- **Connected** — Devices that are currently active and available.
- **Disconnected** — Devices that are currently unplugged or disabled.

Check the boxes next to the devices you want to include in the rotation. Devices you have checked appear in the **Selected** group. SoundSwitch will only switch between the checked devices when you use the hotkey.

## Mute Target

The **Mute microphone** dropdown at the bottom of the tab chooses which input the **Toggle mute** hotkey (and the tray/CLI mute action) controls:

- **Windows default input** — Mutes the Windows default communication input (default).
- **A specific device** — Mutes exactly the chosen input, even a virtual input such as NVIDIA Broadcast or a virtual cable, whether or not it is part of the switching rotation.

SoundSwitch remembers your choice and will do its best to find the microphone again if it disappears. While it is unavailable, the mute hotkey uses the Windows default input.

## Mute Banners

![Mute banner option](/images/Notifications.png)

Microphone mute notifications are configured on the **Notifications** tab. The **Show mute banners only for selected input** option under **Banner Options** decides how much you see:

- **Checked (default)** — A banner appears only for the microphone selected in **Mute microphone**. Mute changes on your other microphones stay silent.
- **Unchecked** — A banner appears for every microphone mute change.

This option only limits **banners**. If you use a Windows toast or a sound for microphone mute notifications, every microphone keeps notifying you.

See [Notifications](notifications.md) for the Microphone On/Off notification styles.

## Hotkey Settings

At the bottom of the tab, you have two independent hotkey controls:

- **Switch device** — Sets the hotkey for cycling through your selected recording devices (the default is `Ctrl + Alt + F7`).
- **Toggle mute** — Sets a separate hotkey to instantly mute or unmute the microphone chosen in **Mute microphone** above (the default is `Ctrl + Alt + M`). With the default target, this is the Windows default communication microphone.
- **Hotkey enabled** — Each hotkey has its own checkbox. Check it to activate the shortcut, or uncheck it to disable it.

To set a hotkey, click the corresponding input field and press the desired key combination. SoundSwitch will register the shortcut immediately.

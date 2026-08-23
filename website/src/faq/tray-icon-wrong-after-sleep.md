---
title: Why does the tray icon show the wrong device after waking from sleep?
description: After sleep/resume the tray icon can display a device that isn't the one actually playing audio. SoundSwitch now re-syncs the default device on wake.
---

# Why does the tray icon show the wrong device after waking from sleep?

After the computer wakes from sleep, the SoundSwitch tray icon can briefly show a
device that is not the one actually playing your audio. This happens because the
icon's cached view of the default device went out of sync with Windows during the
resume transition.

SoundSwitch now refreshes the cached default device automatically when the system
wakes up, so the icon corrects itself within a second or two — no restart required.

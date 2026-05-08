---
title: An app stopped switching audio after a SoundSwitch update
description: Fix Windows app-level default device locks that keep one application stuck on the previous audio device after a SoundSwitch update.
---

# My applications don't switch sound after an update

If after updating SoundSwitch one specific application (often a browser) keeps playing on the previous device while everything else switches correctly, it's because Windows has **locked the default device** for that application.

This was originally caused by a regression in **v5.9.4**, which incorrectly disabled the "Also switch foreground device" feature and left certain apps stuck on whichever device they were last using.

## How to fix it

Reset the per-application sound preferences in **Windows Sound Settings**:

1. Open **Settings → System → Sound**.
2. Scroll down and open **App volume and device preferences** (Windows 10) or **Volume mixer → Reset sound devices and volumes for all apps** (Windows 11).
3. Click **Reset**.

![Resetting Windows sound settings](/images/faq/reset-windows-sound.gif)

After the reset, applications fall back to the system default audio device — which SoundSwitch can then switch normally.

---

_Source: [#645](https://github.com/Belphemur/SoundSwitch/discussions/645)_

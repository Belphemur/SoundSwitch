---
title: Microphone on/off stopped working
description: If microphone switching suddenly stops working, make sure Windows still uses your intended mic as the default communication device.
---

# Microphone on/off stopped working

If SoundSwitch no longer toggles your microphone correctly, check your Windows default communication device.

In this case, the issue was caused by a different audio device being set as the **default communication device**. Once the intended microphone was set back as the default, microphone on/off worked again.

## How to fix it

1. In SoundSwitch, open **Settings → General** and enable **Switch Default Communication Device**.
2. Open **Windows Settings → System → Sound** and go to **Input**.
3. Select the microphone you want SoundSwitch to toggle and ensure it is set as the **default communication device**.

---

_Source: [#2183](https://github.com/Belphemur/SoundSwitch/discussions/2183#discussioncomment-17106232)_

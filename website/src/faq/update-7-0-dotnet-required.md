---
title: SoundSwitch 7.0.0 keeps asking for .NET 10 even when installed
description: Fix startup failures after updating to SoundSwitch 7.0.0 when Windows still reports Microsoft.NETCore.App 10.0.0 as missing.
---

# SoundSwitch 7.0.0 keeps asking for .NET 10 even when installed

If SoundSwitch **7.0.0** shows this message at startup:

> You must install or update .NET to run this application. Required: Microsoft.NETCore.App, version '10.0.0' (x64)

![.NET required on SoundSwitch startup](/images/faq/update-7-0-dotnet-runtime.png)

you likely have leftover files from a previous install that prevent SoundSwitch from launching correctly.

## Fix steps

1. Uninstall **SoundSwitch** from Windows Apps settings (or from Programs and Features).
2. Delete any remaining SoundSwitch files in:
   - `C:\Program Files\SoundSwitch\` (or your custom install folder)
   - `%localappdata%\SoundSwitch\`
3. Install SoundSwitch again using the latest installer.

After reinstalling on a clean folder, SoundSwitch should start normally.

---

_Source: [#2138](https://github.com/Belphemur/SoundSwitch/issues/2138)_

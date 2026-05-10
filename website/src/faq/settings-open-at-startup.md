---
title: Settings window opens every time Windows starts
description: If the SoundSwitch settings window appears on every boot, you likely have two copies of SoundSwitch starting at the same time. Here's how to find and remove the duplicate startup entry.
---

# Settings window opens every time Windows starts

If the SoundSwitch settings window pops up on every reboot, it almost always means **two instances of SoundSwitch are launching at startup**. When the second instance detects that one is already running, it signals the first to open the settings window — which is what you see.

This typically happens when SoundSwitch was installed to a different location at some point (e.g. once for the current user in `%LocalAppData%` and once for all users in `C:\Program Files\SoundSwitch`), leaving behind a startup entry that now points to a path that no longer exists or a second live installation.

## Where to look for duplicate startup entries

Check each of the following locations and remove any SoundSwitch entry that shouldn't be there:

### 1. Startup folders

Open these two folders in Explorer and delete any SoundSwitch shortcut you find:

- **User startup** — `shell:startup` (`%AppData%\Microsoft\Windows\Start Menu\Programs\Startup`)
- **All-users startup** — `shell:common startup` (`C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup`)

::: tip
Type `shell:startup` or `shell:common startup` directly into the Explorer address bar to open the folder instantly.
:::

### 2. Task Manager → Startup apps

1. Open **Task Manager** (<kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>Esc</kbd>).
2. Go to the **Startup apps** tab.
3. Right-click the **Name** column header and enable **Command line** to see the full path of each entry.
4. Look for any SoundSwitch entry with an unexpected path or a broken icon. Disable or delete it.

::: warning
A broken entry (shown with a plain white icon) may survive even after uninstalling SoundSwitch. It can still trigger the issue because Windows still tries to launch it.
:::

### 3. Registry run keys

Open **Registry Editor** (`regedit`) and check both locations for a `SoundSwitch` value:

- `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run`

Delete any SoundSwitch entry that points to a path where SoundSwitch is no longer installed.

### 4. Leftover installation folders

SoundSwitch can be installed per-user or for all users, so check both locations:

- `%LocalAppData%\SoundSwitch` (per-user install)
- `C:\Program Files\SoundSwitch` (all-users install)
- `C:\Program Files (x86)\SoundSwitch`

If you find a folder for an installation you no longer want, remove it after making sure the corresponding startup entry is also gone.

## Recommended clean-up procedure

1. Uninstall SoundSwitch from **Settings → Apps**.
2. Check all four locations above and remove any remaining SoundSwitch entries.
3. Restart your computer and confirm no settings window appears.
4. Reinstall SoundSwitch once, choosing the installation scope you want (current user or all users).
5. Restart again to verify the problem is resolved.

::: tip Stubborn startup entries
If Task Manager won't let you delete a broken startup entry, try [Microsoft Autoruns](https://learn.microsoft.com/en-us/sysinternals/downloads/autoruns) (run as administrator). If the shortcut file itself is missing, open the startup folder directly (`shell:startup` or `shell:common startup`), search for "SoundSwitch", and delete the broken shortcut from there.
:::

---

_Source: [#1914](https://github.com/Belphemur/SoundSwitch/discussions/1914)_

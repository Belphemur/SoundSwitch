# Fix the nightly 7.3.3.27 startup crash around `SetCompatibleTextRenderingDefault`

**Status:** Draft v1 for review
**Author:** Hermes
**Branch:** `fix/issue-2435-dark-theme-startup-crash`
**Base:** `origin/dev` (`502ef673`)
**Issues:** [Belphemur/SoundSwitch#2448](https://github.com/Belphemur/SoundSwitch/issues/2448), referenced from [#2435](https://github.com/Belphemur/SoundSwitch/issues/2435)
**Scope:** startup ordering only. The dark-theme feature from PR #2420 is preserved.

---

## 1. Problem

Nightly 7.3.3.27 (first build containing PR #2420) terminates at boot with:

```
System.InvalidOperationException: SetCompatibleTextRenderingDefault must be called
before the first IWin32Window object is created in the application.
   at System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(Boolean defaultValue)
   at SoundSwitch.Program.OldMain(String[] args) in SoundSwitch/Program.cs:line 131
   at SoundSwitch.Program.Main(String[] args) in SoundSwitch/Program.cs:line 54
```

The only machine known to be affected runs Windows 11 with the Windows **app-mode set to dark**. Light-mode machines do not crash. No UI or tray icon appears; the process dies before any form is shown.

## 2. Root cause

This is **not** caused by `SetColorMode(System)` creating a WinForms control/window on the main thread. The dark theme API itself is benign. The real sequence is a **startup-ordering race** that `SetColorMode(System)` makes deterministic on dark-mode machines.

### 2.1 The guard is process-wide

`Application.SetCompatibleTextRenderingDefault` checks `NativeWindow.AnyHandleCreated`, which is process-wide — not thread-local. In the .NET 10 WinForms source:

```csharp
public static void SetCompatibleTextRenderingDefault(bool defaultValue)
{
    if (NativeWindow.AnyHandleCreated)
    {
        throw new InvalidOperationException(...);
    }

    Control.UseCompatibleTextRenderingDefault = defaultValue;
}
```

So once **any** `NativeWindow`/`Control` handle has been created anywhere in the process — including on a background thread — the call throws.

### 2.2 SoundSwitch creates a hidden WinForms form on a background thread

`WindowsAPIAdapter` is a `Form` subclass used as a hidden message-pump window for hotkeys, device events, restart-manager events, session events, and theme-change events. `WindowsAPIAdapter.Start()` is called **before** the current startup block:

- `SoundSwitch/Program.cs:91` (release path) or `:93` (debug path) calls `WindowsAPIAdapter.Start(...)`.
- `WindowsAPIAdapter.Start()` (`SoundSwitch/Framework/WinApi/WindowsAPIAdapter.cs:98-109`) launches a dedicated STA background thread.
- That thread's `RunForm()` (`SoundSwitch/Framework/WinApi/WindowsAPIAdapter.cs:145-165`) creates `_instance = new WindowsAPIAdapter();` and immediately calls `_instance.CreateHandle();`.

Therefore a WinForms window handle can be created **concurrently with** and potentially **before** `Application.SetCompatibleTextRenderingDefault(false)` on the main thread. This race existed before PR #2420, but the main thread typically reached the call quickly enough.

### 2.3 Why PR #2420 changed the timing

PR #2420 added `Application.SetColorMode(SystemColorMode.System)` at `SoundSwitch/Program.cs:129`, immediately before the call at `:131`. In the .NET 10 WinForms source, `SetColorMode` does not itself construct a managed control. However, when the resolved mode is dark (that is, `SystemColorMode.System` plus a dark Windows app-mode), it:

1. Sets `SystemColors.UseAlternativeColorSet = true`.
2. Calls `NotifySystemEventsOfColorChange()`, which finds the `.NET-BroadcastEventWindow.*` window used by `SystemEvents`, sends `WM_SYSCOLORCHANGE`, and then loops on `DoEvents()`/`Thread.Yield()` until the callback completes.

That `DoEvents` pump gives the background STA thread started by `WindowsAPIAdapter.Start()` time to finish `_instance.CreateHandle()`. By the time control returns and reaches `SetCompatibleTextRenderingDefault(false)`, `NativeWindow.AnyHandleCreated` is already true and the API throws. On light-mode machines the alternate color set is not switched, so the extra pump does not run and the race is not usually lost.

### 2.4 Conclusion

The crash is a deterministic manifestation of an existing latent ordering bug: `WindowsAPIAdapter.Start()` creates a hidden form before the app has established the process-wide text-rendering default. PR #2420 didn't introduce a new window on the main thread; it introduced a timing delay that exposed the pre-existing race on dark-mode machines.

## 3. Chosen fix

Move `Application.SetCompatibleTextRenderingDefault(false)` to the very beginning of `Program.OldMain`, **before** `WindowsAPIAdapter.Start()`. This is the first WinForms-relevant call in the process. `Application.SetColorMode(SystemColorMode.System)` and all dark-theme behaviour remain exactly where they are.

### 3.1 Resulting startup order

1. `Application.SetCompatibleTextRenderingDefault(false)`
2. Sentry/logger/exception-handler setup
3. `WindowsAPIAdapter.Start()` — creates the hidden background form
4. Single-instance mutex/IPC check
5. `SetProcessDPIAware()`
6. `Application.EnableVisualStyles()`
7. `Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)`
8. `Application.SetColorMode(SystemColorMode.System)`
9. `Application.Run(appContext)`

### 3.2 Why this and not the alternatives

| Option | Verdict |
|---|---|
| Swap `SetColorMode` and `SetCompatibleTextRenderingDefault` inside the startup block | Rejected. It removes one trigger but still leaves `WindowsAPIAdapter.Start()` creating a hidden form before the process-wide default is established. The race remains. |
| Defer `SetColorMode` until after `SetCompatibleTextRenderingDefault` | Rejected for the same reason; it does not address the pre-existing background-form hazard. |
| Replace `SetColorMode(System)` with explicit mode selection | Rejected. It changes the user-visible dark-theme semantics and is not needed. |
| Ensure `WindowsAPIAdapter` doesn't create a handle | Rejected. That form is load-bearing for hotkeys, device events, restart-manager, session, and theme notifications. |
| **Move `SetCompatibleTextRenderingDefault(false)` before `WindowsAPIAdapter.Start()`** | **Chosen.** Minimal, preserves dark-theme behaviour, and removes the only precondition the API actually requires. |

## 4. Behavioural impact

- No user-visible change is intended. The value passed is still `false`, exactly as before.
- Dark-mode support is unaffected. `SetColorMode(SystemColorMode.System)` still runs before `Application.Run`, so every subsequently created form still follows the OS app-mode.
- `BannerForm` is untouched.

## 5. Files touched

| File | Change |
|---|---|
| `SoundSwitch/Program.cs` | Relocate `Application.SetCompatibleTextRenderingDefault(false)` from line 131 to the beginning of `OldMain`, before `WindowsAPIAdapter.Start()`. Add a comment explaining why. |

## 6. Validation

1. Local Linux build gate (compile-only):
   ```bash
   dotnet build SoundSwitch/SoundSwitch.csproj -c Debug -p:LinuxBuild=true -p:BuildProjectReferences=false
   ```
2. Windows CI build/run must be green.
3. Manual smoke on Windows 11 dark app-mode (maintainer/reviewer): app starts, tray icon appears, settings forms follow the OS theme, theme change is reflected live.

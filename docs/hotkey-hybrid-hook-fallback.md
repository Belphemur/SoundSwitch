# Hotkey Hybrid: RegisterHotKey with WH_KEYBOARD_LL Fallback

## Problem

After system sleep/resume, Windows unregisters all global hotkeys registered via `RegisterHotKey`. The existing `SystemEvents.PowerModeChanged` handler attempts to re-register them, but:

1. `RegisterHotKey` is **first-come-first-served** — if another app grabs the same key combo during resume, the call fails with `ERROR_HOT_KEY_ALREADY_REGISTERED` (1409). There is **no priority mechanism** in the Win32 API.
2. The current `SystemEvents.PowerModeChanged` handler dispatches on the UI thread via `_instance.Invoke()` (synchronous), risking deadlocks if the UI thread is busy during the critical resume window.
3. `SystemEvents` delivery can be unreliable depending on Windows configuration (fast startup, hibernate vs. sleep).

Issue: [#2105](https://github.com/Belphemur/SoundSwitch/issues/2105) — hotkeys lost after restart/wake, requiring app restart.

## Proposed Solution

Use a **hybrid approach**: prefer `RegisterHotKey`, but fall back to a `WH_KEYBOARD_LL` hook when registration fails due to a conflict. This guarantees hotkey capture even when another application holds the same key combination.

### Why This Works

- `WH_KEYBOARD_LL` hooks are **not exclusive** — multiple apps can install them simultaneously without conflict.
- The hook intercepts key events **before** they reach other apps' `RegisterHotKey` registrations, giving SoundSwitch guaranteed capture.
- The same `HotKeyPressed` event is fired from both paths, so all existing subscribers (UI, AppModel, ProfileManager) work transparently with zero changes.

## Implementation Plan

Changes span two files:

- **`SoundSwitch/Framework/WinApi/WindowsAPIAdapter.cs`** — main changes
- **`SoundSwitch/Framework/WinApi/Interop.cs`** — additional P/Invoke imports

### 1. Add P/Invoke Imports (`Interop.cs`)

```csharp
internal static class NativeMethods
{
    // Low-level keyboard hook
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern IntPtr GetModuleHandle(string lpModuleName);
}
```

### 2. Add Constants and Types (`WindowsAPIAdapter.cs`)

```csharp
private const int WH_KEYBOARD_LL = 13;
private const int WM_KEYDOWN = 0x0100;

private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
```

### 3. Add Hook Infrastructure Fields

```csharp
private static readonly HashSet<HotKey> _hookedHotkeys = new();
private static IntPtr _hookHandle = IntPtr.Zero;
private static Thread _hookThread;
private static LowLevelKeyboardProc _hookProc; // static field prevents GC collection
```

### 4. Dedicated Hook Thread

The `WH_KEYBOARD_LL` hook requires a message pump on the installing thread. A dedicated background STA thread handles this:

```csharp
private static void EnsureHookThreadRunning()
{
    if (_hookThread != null && _hookThread.IsAlive) return;

    _hookThread = new Thread(HookThreadProc)
    {
        Name = "KeyboardHookThread",
        IsBackground = true
    };
    _hookThread.SetApartmentState(ApartmentState.STA);
    _hookThread.Start();
}

private static void HookThreadProc()
{
    _hookProc = LowLevelKeyboardHookCallback;
    using var curProcess = Process.GetCurrentProcess();
    using var curModule = curProcess.MainModule;
    _hookHandle = SetWindowsHookEx(
        WH_KEYBOARD_LL, _hookProc,
        GetModuleHandle(curModule.ModuleName), 0);

    // Message pump — required for WH_KEYBOARD_LL to function
    Application.Run();
}
```

### 5. Hook Callback — Hotkey Matching

```csharp
private static IntPtr LowLevelKeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
{
    if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
    {
        int vkCode = Marshal.ReadInt32(lParam);
        var key = (Keys)vkCode;
        var modifier = KeyboardWindowsAPI.GetPressedModifiers();

        lock (_hookedHotkeys)
        {
            foreach (var hotKey in _hookedHotkeys)
            {
                if (hotKey.Keys == key && hotKey.Modifier == modifier)
                {
                    // Fire the same event the RegisterHotKey path uses
                    HotKeyPressed?.Invoke(_instance, new KeyPressedEventArgs(hotKey));
                    return (IntPtr)1; // swallow key, prevent further dispatch
                }
            }
        }
    }
    return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
}
```

### 6. Modify `RegisterHotKey` — Add Fallback Path

When `NativeMethods.RegisterHotKey` fails, track the hotkey in `_hookedHotkeys` and start the hook thread:

```csharp
lock (_instance)
{
    return (bool)_instance.Invoke(() =>
    {
        if (_instance._registeredHotkeys.ContainsKey(hotKey))
            return false;

        var id = _instance._hotKeyId++;
        _instance._registeredHotkeys.Add(hotKey, id);

        if (NativeMethods.RegisterHotKey(_instance.Handle, id,
                (uint)hotKey.Modifier, (uint)hotKey.Keys))
            return true;

        // FAILED: fallback to hook
        _instance._registeredHotkeys.Remove(hotKey);
        lock (_hookedHotkeys)
        {
            _hookedHotkeys.Add(hotKey);
        }
        EnsureHookThreadRunning();
        Log.Warning("RegisterHotKey conflict for {hotkey}, using hook fallback", hotKey);
        return true;
    });
}
```

### 7. Modify `UnRegisterHotKey` — Handle Both Paths

```csharp
lock (_instance)
{
    return (bool)_instance.Invoke(() =>
    {
        if (_instance._registeredHotkeys.TryGetValue(hotKey, out var id))
        {
            _instance._registeredHotkeys.Remove(hotKey);
            return NativeMethods.UnregisterHotKey(_instance.Handle, id);
        }

        lock (_hookedHotkeys)
        {
            var removed = _hookedHotkeys.Remove(hotKey);
            if (removed && _hookedHotkeys.Count == 0)
            {
                // Clean up hook when no hooked hotkeys remain
                Application.ExitThread(); // stops the hook message pump
                _hookThread = null;
            }
            return removed;
        }
    });
}
```

### 8. Improve Power Resume Handling

Add `WM_POWERBROADCAST` handling in `WndProc` as a more reliable companion to the existing `SystemEvents.PowerModeChanged`:

```csharp
private const int WM_POWERBROADCAST = 0x0218;
private const int PBT_APMRESUMEAUTOMATIC = 0x0012;

// In WndProc, before base.WndProc:
case WM_POWERBROADCAST:
    if (m.WParam.ToInt32() == PBT_APMRESUMEAUTOMATIC)
    {
        BeginInvoke((Action)ReRegisterAllHotkeys);
    }
    break;

private void ReRegisterAllHotkeys()
{
    // Re-register RegisterHotKey hotkeys (hook-based survive sleep automatically)
    foreach (var (hotKey, hotKeyId) in _registeredHotkeys)
    {
        NativeMethods.UnregisterHotKey(_instance.Handle, hotKeyId);
        NativeMethods.RegisterHotKey(_instance.Handle, hotKeyId,
            (uint)hotKey.Modifier, (uint)hotKey.Keys);
    }
}
```

Keep the existing `SystemEvents.PowerModeChanged` as a belt-and-suspenders fallback.

### 9. Thread Safety Summary

| Access | Protection |
|---|---|
| `_registeredHotkeys` | `lock(_instance)` (existing pattern) |
| `_hookedHotkeys` | `lock(_hookedHotkeys)` (new) |
| `HotKeyPressed` event | Both RegisterHotKey path (via Task.Factory.StartNew) and LL hook path (via Task.Factory.StartNew) fire events on background threads. UI thread marshaling is handled by BeginInvoke in the hook callback, but subscribers should handle cross-thread dispatch if needed. |
| `_hookProc` delegate | Static field prevents GC collection |

## What Subscribers See

No changes required. `HotKeyTextBox`, `AppModel.HandleHotkeyPress`, and `ProfileHotkeyManager.WindowsAPIAdapterOnHotKeyPressed` all subscribe to the same `WindowsAPIAdapter.HotKeyPressed` static event and work identically regardless of which registration path captured the key.

## Testing Strategy

- **Unit tests**: Verify `RegisterHotKey` returns true when hook fallback activates (mock `RegisterHotKey` to return false)
- **Integration tests**: Manual verification that hotkeys work after sleep/resume with competing hotkey registrations
- **Existing tests**: No existing hotkey tests to update

## Risks

1. **Antivirus false positives**: `WH_KEYBOARD_LL` hooks are commonly flagged by heuristic AV engines. Acceptable tradeoff — the hook is a fallback, not the primary path.
2. **Modifier matching**: Ensure `KeyboardWindowsAPI.GetPressedModifiers()` normalization matches what `RegisterHotKey` expects. Potential edge case with modifier-only hotkeys (e.g., `Shift` alone).
3. **Hook thread lifecycle**: The hook thread must be properly cleaned up on `Stop()`/`Dispose()`. Add `UnhookWindowsHookEx` and `Application.ExitThread` to the cleanup path.
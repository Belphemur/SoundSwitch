/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2017 Antoine Aflalo
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 ********************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Win32;

using Serilog;

using SoundSwitch.Audio.Manager.Interop.Com.User;
using SoundSwitch.Framework.WinApi.Keyboard;

namespace SoundSwitch.Framework.WinApi;

/// <summary>
/// Adapter for Windows API integration, managing global hotkeys, device change notifications,
/// and system power events. Runs on a dedicated STA thread with a message pump.
/// </summary>
public class WindowsAPIAdapter : Form
{
    public enum RestartManagerEventType
    {
        Query,
        EndSession,
        ForceClose
    }

    /**
        #define WM_QUERYENDSESSION              0x0011
        #define WM_ENDSESSION                   0x0016
        #define ENDSESSION_CLOSEAPP         0x00000001
        #define WM_CLOSE                        0x0010
        #define WM_DEVICECHANGE                 0x0219
    */
    private const int WM_QUERYENDSESSION = 0x0011;

    private const int WM_ENDSESSION = 0x0016;
    private const int ENDSESSION_CLOSEAPP = 0x00000001;
    private const int WM_CLOSE = 0x0010;
    private const int WM_DEVICECHANGE = 0x0219;
    private const int WM_HOTKEY = 0x0312;
    private const int WM_POWERBROADCAST = 0x0218;
    private const int PBT_APMRESUMEAUTOMATIC = 0x0012;
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_QUIT = 0x0012;
    private const int ERROR_HOT_KEY_ALREADY_REGISTERED = 1409;

    private static WindowsAPIAdapter _instance;
    private static ThreadExceptionEventHandler _exceptionEventHandler;
    private readonly Dictionary<HotKey, int> _registeredHotkeys = new Dictionary<HotKey, int>();
    private static readonly HashSet<HotKey> _hookedHotkeys = new HashSet<HotKey>();
    private static IntPtr _hookHandle = IntPtr.Zero;
    private static Thread _hookThread;
    private static uint _hookNativeThreadId;
    private static Interop.LowLevelKeyboardProc _hookProc;
    private static readonly object _hookLock = new object();
    private static readonly ManualResetEvent _hookInstallComplete = new ManualResetEvent(false);
    private static bool _hookInstallSucceeded;
    private int _hotKeyId;
    private int _msgNotifyShell;

    private WindowsAPIAdapter()
    {
    }

    public static event EventHandler<RestartManagerEvent> RestartManagerTriggered;
    public static event EventHandler<DeviceChangeEvent> DeviceChanged;
    public static event EventHandler<KeyPressedEventArgs> HotKeyPressed;
    public static event EventHandler<WindowDestroyedEvent> WindowDestroyed;

    /// <summary>
    ///     Start the Adapter thread
    /// </summary>
    public static void Start(ThreadExceptionEventHandler exceptionEventHandler = null)
    {
        if (_instance != null)
            throw new InvalidOperationException("Adapter already started");

        _exceptionEventHandler = exceptionEventHandler;

        var t = new Thread(RunForm) { Name = nameof(WindowsAPIAdapter) };
        t.SetApartmentState(ApartmentState.STA);
        t.IsBackground = true;
        t.Start();
    }

    /// <summary>
    ///     Stop the adapter thread
    /// </summary>
    public static void Stop()
    {
        Log.Information("WindowsApiAdapter asked to stop.");
        if (_instance == null)
        {
            Log.Warning("Windows API Adapter wasn't started");
            return;
        }

        RestartManagerTriggered = null;
        DeviceChanged = null;
        HotKeyPressed = null;

        if (!_instance.IsDisposed)
        {
            try
            {
                _instance.Invoke(new MethodInvoker(_instance.EndForm));
            }
            catch (Exception ex)
            {
                //Can happen when the instance got dispose in its own thread
                //when in the same time the Application thread call the Stop() method.
                Trace.WriteLine("Thread Race Condition: " + ex);
            }
        }
    }

    private static void RunForm()
    {
        Log.Information("Starting WindowsAPIAdapter thread");
        if (_exceptionEventHandler != null)
        {
            Application.ThreadException += _exceptionEventHandler;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => _exceptionEventHandler(sender, new ThreadExceptionEventArgs((Exception)args.ExceptionObject));
        }

        SystemEvents.PowerModeChanged += SystemEventsOnPowerModeChanged;

        _instance = new WindowsAPIAdapter();
        _instance.CreateHandle();
        _instance._msgNotifyShell = Interop.RegisterWindowMessage("SHELLHOOK");
        Interop.RegisterShellHookWindow(User32.NativeMethods.HWND.Cast(_instance.Handle));
        Log.Information("Handle created. Running the application.");
        Application.Run(_instance);
        Log.Information("End of the WindowsAPIAdapter thread");
    }

    private static void SystemEventsOnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode != PowerModes.Resume)
        {
            return;
        }

        Log.Information("Computer coming back from sleep, re-registering hotkeys");

        // Dispatch to adapter thread to avoid race conditions with _registeredHotkeys
        _instance?.BeginInvoke(new Action(_instance.ReRegisterAllHotkeys));
    }

    private void EndForm()
    {
        Close();
        SystemEvents.PowerModeChanged -= SystemEventsOnPowerModeChanged;
        CleanupHook();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_exceptionEventHandler != null)
                Application.ThreadException -= _exceptionEventHandler;
            foreach (var hotKeyId in _registeredHotkeys.Values)
            {
                NativeMethods.UnregisterHotKey(Handle, hotKeyId);
            }

            CleanupHook();

            Interop.DeregisterShellHookWindow(User32.NativeMethods.HWND.Cast(Handle));
        }

        base.Dispose(disposing);
    }

    /// <summary>
    ///     Registers a HotKey in the system.
    ///     If <see cref="NativeMethods.RegisterHotKey"/> fails due to a conflict,
    ///     falls back to monitoring via a low-level keyboard hook (<see cref="WH_KEYBOARD_LL"/>).
    /// </summary>
    /// <param name="hotKey">Represent the hotkey to register</param>
    public static bool RegisterHotKey(HotKey hotKey)
    {
        WaitForHandle();

        if (_instance.IsDisposed)
        {
            //Can happen when the instance got dispose in its own thread
            //when in the same time the Application thread call the Stop() method.
            Trace.WriteLine("Thread Race Condition when registering hotkey");
            return false;
        }

        lock (_instance)
        {
            return (bool)_instance.Invoke(() =>
            {
                Log.Information("Registering Hotkeys: {hotkeys}", hotKey);
                if (_instance._registeredHotkeys.ContainsKey(hotKey))
                {
                    Log.Information("Already registered {hotkeys}", hotKey);
                    return false;
                }

                // Also check if already hooked
                lock (_hookedHotkeys)
                {
                    if (_hookedHotkeys.Contains(hotKey))
                    {
                        Log.Information("Already hooked {hotkeys}", hotKey);
                        return false;
                    }
                }

                var id = _instance._hotKeyId++;
                _instance._registeredHotkeys.Add(hotKey, id);
                // register the hot key.
                if (NativeMethods.RegisterHotKey(_instance.Handle, id, (uint)hotKey.Modifier,
                        (uint)hotKey.Keys))
                    return true;

                // RegisterHotKey failed — check if it's a conflict vs. other error
                var lastError = Marshal.GetLastWin32Error();
                _instance._registeredHotkeys.Remove(hotKey);

                if (lastError != ERROR_HOT_KEY_ALREADY_REGISTERED)
                {
                    Log.Error("RegisterHotKey failed for {hotkey} with Win32Error={error}", hotKey, lastError);
                    return false;
                }

                // Conflict detected — fallback to hook
                Log.Warning("RegisterHotKey conflict (error {error}) for {hotkey}, using hook fallback", lastError, hotKey);

                lock (_hookedHotkeys)
                {
                    _hookedHotkeys.Add(hotKey);
                }

                // Try to start the hook - if it fails, roll back and return false
                if (!EnsureHookThreadRunning())
                {
                    lock (_hookedHotkeys)
                    {
                        _hookedHotkeys.Remove(hotKey);
                    }
                    Log.Error("Failed to install keyboard hook fallback for {hotkey}", hotKey);
                    return false;
                }

                return true;
            });
        }
    }

    /// <summary>
    ///     Unregister a registered HotKey
    /// </summary>
    /// <param name="hotKey"></param>
    /// <returns></returns>
    public static bool UnRegisterHotKey(HotKey hotKey)
    {
        WaitForHandle();

        lock (_instance)
        {
            return (bool)_instance.Invoke(new Func<bool>(() =>
            {
                Log.Information("Unregistering Hotkeys: {hotkeys}", hotKey);
                if (_instance._registeredHotkeys.TryGetValue(hotKey, out var id))
                {
                    _instance._registeredHotkeys.Remove(hotKey);
                    return NativeMethods.UnregisterHotKey(_instance.Handle, id);
                }

                lock (_hookedHotkeys)
                {
                    if (_hookedHotkeys.Remove(hotKey))
                    {
                        Log.Information("Unregistered hooked hotkey {hotkeys}", hotKey);
                        if (_hookedHotkeys.Count == 0)
                        {
                            CleanupHook();
                        }
                        return true;
                    }
                }

                Log.Information("Not registered {hotkeys}", hotKey);
                return false;
            }));
        }
    }

    private static void WaitForHandle()
    {
        var count = 0;
        while (_instance == null || !_instance.IsHandleCreated)
        {
            Thread.Sleep(250);
            if (count++ >= 5)
            {
                throw new ThreadStateException($"Instance isn't set even after waiting {5 * 250} ms");
            }
        }
    }

    protected override void SetVisibleCore(bool value)
    {
        base.SetVisibleCore(false);
    }

    protected override void WndProc(ref Message m)
    {
        //Check for shutdown message from windows
        switch (m.Msg)
        {
            case WM_QUERYENDSESSION:
                var closingEvent = new RestartManagerEvent(RestartManagerEventType.Query);
                RestartManagerTriggered?.Invoke(this, closingEvent);
                m.Result = closingEvent.Result;
                Log.Information($"Received WM_QUERYENDSESSION responded {m.Result}");
                break;
            case WM_ENDSESSION:
                RestartManagerTriggered?.Invoke(this, new RestartManagerEvent(RestartManagerEventType.EndSession));
                Log.Information("Received WM_ENDSESSION");
                break;

            case WM_CLOSE:
                RestartManagerTriggered?.Invoke(this, new RestartManagerEvent(RestartManagerEventType.ForceClose));
                Log.Information("Received WM_CLOSE");
                break;
            case WM_DEVICECHANGE:
                DeviceChanged?.Invoke(this, new DeviceChangeEvent());
                break;
            case WM_HOTKEY:
                ProcessHotKeyEvent(m);
                break;
            case WM_POWERBROADCAST:
                if (m.WParam.ToInt32() == PBT_APMRESUMEAUTOMATIC)
                {
                    // Re-register RegisterHotKey hotkeys asynchronously on UI thread.
                    // Hook-based hotkeys survive sleep automatically.
                    BeginInvoke(new Action(ReRegisterAllHotkeys));
                }
                break;
        }

        if (WindowDestroyed != null && m.Msg == _msgNotifyShell)
        {
            // Receive shell messages
            switch ((Interop.ShellEvents)m.WParam.ToInt32())
            {
                case Interop.ShellEvents.HSHELL_WINDOWDESTROYED:
                    var hwnd = User32.NativeMethods.HWND.Cast(m.LParam);
                    Task.Factory.StartNew(() => { WindowDestroyed?.Invoke(this, new WindowDestroyedEvent(hwnd)); });
                    break;
            }
        }

        base.WndProc(ref m);
    }

    /// <summary>
    ///     To avoid overflow on 64 bit platform use this method
    /// </summary>
    /// <param name="lParam"></param>
    /// <returns></returns>
    private long ConvertLParam(IntPtr lParam)
    {
        try
        {
            return lParam.ToInt32();
        }
        catch (OverflowException)
        {
            return lParam.ToInt64();
        }
    }

    private void ProcessHotKeyEvent(Message m)
    {
        var key = (Keys)((ConvertLParam(m.LParam) >> 16) & 0xFFFF);
        var modifier = (HotKey.ModifierKeys)(ConvertLParam(m.LParam) & 0xFFFF);

        Task.Factory.StartNew(() => { HotKeyPressed?.Invoke(this, new KeyPressedEventArgs(new HotKey(key, modifier))); });
    }

    /// <summary>
    ///     Re-registers all RegisterHotKey-based hotkeys. Called on resume from sleep.
    ///     Hook-based hotkeys survive sleep automatically and don't need re-registration.
    ///     Handles conflicts by falling back to hook-based capture.
    /// </summary>
    /// <remarks>Enumerates current registered hotkeys and attempts to re-register each via
    /// NativeMethods.RegisterHotKey. If registration fails due to conflict (ERROR_HOT_KEY_ALREADY_REGISTERED),
    /// moves the hotkey to the hooked fallback set.</remarks>
    private void ReRegisterAllHotkeys()
    {
        HotKey[] hotKeysToReregister;
        lock (_instance)
        {
            hotKeysToReregister = _registeredHotkeys.Keys.ToArray();
        }

        foreach (var hotKey in hotKeysToReregister)
        {
            int hotKeyId;
            lock (_instance)
            {
                if (!_registeredHotkeys.TryGetValue(hotKey, out hotKeyId))
                    continue;
            }

            NativeMethods.UnregisterHotKey(_instance.Handle, hotKeyId);
            if (!NativeMethods.RegisterHotKey(_instance.Handle, hotKeyId, (uint)hotKey.Modifier, (uint)hotKey.Keys))
            {
                var lastError = Marshal.GetLastWin32Error();
                if (lastError == ERROR_HOT_KEY_ALREADY_REGISTERED)
                {
                    Log.Warning("Re-registration failed for {hotkey} with error {error}, falling back to hook", hotKey, lastError);
                    lock (_instance)
                    {
                        _registeredHotkeys.Remove(hotKey);
                    }
                    lock (_hookedHotkeys)
                    {
                        _hookedHotkeys.Add(hotKey);
                    }
                    if (!EnsureHookThreadRunning())
                    {
                        lock (_hookedHotkeys)
                        {
                            _hookedHotkeys.Remove(hotKey);
                        }
                        Log.Error("Failed to install keyboard hook fallback during re-registration for {hotkey}", hotKey);
                    }
                }
                else
                {
                    Log.Error("Re-registration failed for {hotkey} with Win32Error={error}", hotKey, lastError);
                }
            }
        }
    }

    /// <summary>
    ///     Ensures the low-level keyboard hook thread is running.
    ///     The hook requires a dedicated message pump, so a background STA thread is used.
    /// </summary>
    /// <returns>True if hook is running and installed; false if installation failed.</returns>
    /// <remarks>Synchronized via _hookLock. If _hookThread is null or not alive, starts a new background
    /// STA thread named "KeyboardHook" that runs HookThreadProc. Waits for hook installation to complete
    /// and returns the installation result.</remarks>
    private static bool EnsureHookThreadRunning()
    {
        lock (_hookLock)
        {
            if (_hookThread != null && _hookThread.IsAlive && _hookHandle != IntPtr.Zero)
                return true;

            _hookInstallComplete.Reset();
            _hookInstallSucceeded = false;

            _hookThread = new Thread(HookThreadProc)
            {
                Name = "KeyboardHook",
                IsBackground = true
            };
            _hookThread.SetApartmentState(ApartmentState.STA);
            _hookThread.Start();

            // Wait for hook installation to complete (with timeout)
            if (!_hookInstallComplete.WaitOne(5000))
            {
                Log.Error("Hook installation timed out after 5 seconds");
                return false;
            }

            return _hookInstallSucceeded;
        }
    }

    /// <summary>
    ///     Hook thread procedure: installs the low-level keyboard hook and runs a message pump.
    /// </summary>
    /// <remarks>Captures the native thread ID for cleanup, installs WH_KEYBOARD_LL hook, runs a message pump
    /// (Application.Run) to process hook callbacks, signals installation success/failure via
    /// _hookInstallComplete, and unhooks on the same thread before returning.</remarks>
    private static void HookThreadProc()
    {
        // Capture native thread ID for use in CleanupHook when posting WM_QUIT
        _hookNativeThreadId = Interop.GetCurrentThreadId();

        _hookProc = LowLevelKeyboardHookCallback;
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        _hookHandle = Interop.SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc,
            Interop.GetModuleHandle(curModule.ModuleName), 0);

        if (_hookHandle == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            Log.Error("SetWindowsHookEx failed with Win32Error={error}", error);
            _hookInstallSucceeded = false;
            _hookInstallComplete.Set();
            return; // Don't start message pump if hook failed
        }

        _hookInstallSucceeded = true;
        _hookInstallComplete.Set();

        // Message pump required for WH_KEYBOARD_LL to function
        Application.Run();

        // Unhook on the same thread that installed it (best practice)
        if (_hookHandle != IntPtr.Zero)
        {
            Interop.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }
    }

    /// <summary>
    ///     Low-level keyboard hook callback. Checks pressed keys against hooked hotkeys
    ///     and fires HotKeyPressed for matches. Non-exclusive - key continues to other apps.
    /// </summary>
    /// <param name="nCode">The hook code. Positive values indicate processing is required.</param>
    /// <param name="wParam">The WM_* message ID (e.g., WM_KEYDOWN = 0x0100).</param>
    /// <param name="lParam">A pointer to the KBDLLHOOKSTRUCT structure containing keyboard event data.</param>
    /// <returns>IntPtr.Zero to pass the message to the next hook, or a non-zero value to consume the message.</returns>
    /// <remarks>On WM_KEYDOWN, reads the virtual key code and pressed modifiers, matches against
    /// _hookedHotkeys, fires HotKeyPressed on match (on background thread), and returns after calling
    /// CallNextHookEx to allow non-exclusive monitoring.</remarks>
    private static IntPtr LowLevelKeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            var vkCode = Marshal.ReadInt32(lParam);
            var key = (Keys)vkCode;
            var modifier = PressedModifiersToModifierKeys(KeyboardWindowsAPI.GetPressedModifiers());

            lock (_hookedHotkeys)
            {
                foreach (var hotKey in _hookedHotkeys)
                {
                    if (hotKey.Keys == key && hotKey.Modifier == modifier)
                    {
                        // Fire event on background thread for consistency with RegisterHotKey path
                        Task.Factory.StartNew(() =>
                        {
                            HotKeyPressed?.Invoke(_instance, new KeyPressedEventArgs(hotKey));
                        });
                        break; // Don't suppress key - allow non-exclusive monitoring
                    }
                }
            }
        }

        return Interop.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    /// <summary>
    ///     Converts the enumerable of pressed modifier Keys into a ModifierKeys flags value.
    /// </summary>
    /// <param name="modifiers">An enumerable of Keys representing currently pressed modifier keys.</param>
    /// <returns>A HotKey.ModifierFlags value representing the pressed modifiers as bit flags.</returns>
    /// <remarks>Maps Keys.Alt, Keys.Control, Keys.Shift, and Keys.LWin to corresponding
    /// HotKey.ModifierKeys flags (Alt, Control, Shift, Win). Keys.RWin is also accepted.</remarks>
    private static HotKey.ModifierKeys PressedModifiersToModifierKeys(IEnumerable<Keys> modifiers)
    {
        HotKey.ModifierKeys result = 0;
        foreach (var key in modifiers)
        {
            switch (key)
            {
                case Keys.Alt:
                    result |= HotKey.ModifierKeys.Alt;
                    break;
                case Keys.Control:
                    result |= HotKey.ModifierKeys.Control;
                    break;
                case Keys.Shift:
                    result |= HotKey.ModifierKeys.Shift;
                    break;
                case Keys.LWin:
                    result |= HotKey.ModifierKeys.Win;
                    break;
            }
        }
        return result;
    }

    /// <summary>
    ///     Cleans up the low-level keyboard hook and its thread.
    /// </summary>
    /// <remarks>Synchronized via _hookLock. Uninstalls the hook via UnhookWindowsHookEx (if handle is valid),
    /// posts WM_QUIT to the hook thread's message pump to stop Application.Run, joins the thread with 2s timeout,
    /// and clears the _hookedHotkeys set.</remarks>
    private static void CleanupHook()
    {
        lock (_hookLock)
        {
            if (_hookHandle != IntPtr.Zero)
            {
                Log.Information("Unhooking low-level keyboard hook");
                Interop.UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }

            if (_hookThread != null && _hookThread.IsAlive)
            {
                Log.Information("Stopping hook thread message pump");
                // Post WM_QUIT to the hook thread's message pump using native thread ID
                Interop.PostThreadMessage(_hookNativeThreadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                if (!_hookThread.Join(2000))
                {
                    Log.Warning("Hook thread did not exit cleanly");
                }
                else
                {
                    Log.Information("Hook thread exited cleanly");
                }
            }
            _hookThread = null;

            Log.Information("Clearing {count} hooked hotkeys", _hookedHotkeys.Count);
            lock (_hookedHotkeys)
            {
                _hookedHotkeys.Clear();
            }
        }
    }

    #region WindowsNativeMethods

    public static class NativeMethods
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // Unregisters the hot key with Windows.
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    #endregion

    #region Events

    public class WindowDestroyedEvent(User32.NativeMethods.HWND hwnd) : EventArgs
    {
        public User32.NativeMethods.HWND Hwnd { get; } = hwnd;
    }

    public class RestartManagerEvent(RestartManagerEventType type) : EventArgs
    {
        public IntPtr Result { get; set; } = new(1);
        public RestartManagerEventType Type { get; } = type;
    }

    public class DeviceChangeEvent : EventArgs;

    /// <summary>
    ///     Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs(HotKey hotKey) : EventArgs
    {
        public HotKey HotKey { get; set; } = hotKey;
    }

    #endregion
}

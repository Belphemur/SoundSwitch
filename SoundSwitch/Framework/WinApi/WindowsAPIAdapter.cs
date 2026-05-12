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

    private static WindowsAPIAdapter _instance;
    private static ThreadExceptionEventHandler _exceptionEventHandler;
    private readonly Dictionary<HotKey, int> _registeredHotkeys = new Dictionary<HotKey, int>();
    private static readonly HashSet<HotKey> _hookedHotkeys = new HashSet<HotKey>();
    private static IntPtr _hookHandle = IntPtr.Zero;
    private static Thread _hookThread;
    private static Interop.LowLevelKeyboardProc _hookProc;
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

        // Re-register RegisterHotKey-based hotkeys (hook-based ones survive sleep automatically)
        foreach (var (hotKey, hotKeyId) in _instance._registeredHotkeys.ToArray())
        {
            _instance.BeginInvoke(() =>
            {
                var wasRegistered = NativeMethods.UnregisterHotKey(_instance.Handle, hotKeyId);
                var isRegistered = NativeMethods.RegisterHotKey(_instance.Handle, hotKeyId, (uint)hotKey.Modifier, (uint)hotKey.Keys);
                Log.Information("Re-registering hotkey {Hotkey}: Result(Was: {WasRegistered}, Is:{IsRegistered})", hotKey, wasRegistered, isRegistered);
            });
        }
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

                // RegisterHotKey failed (conflict) — fallback to hook
                _instance._registeredHotkeys.Remove(hotKey);
                Log.Warning("RegisterHotKey conflict for {hotkey}, using hook fallback", hotKey);

                lock (_hookedHotkeys)
                {
                    _hookedHotkeys.Add(hotKey);
                }
                EnsureHookThreadRunning();
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
    /// </summary>
    private void ReRegisterAllHotkeys()
    {
        foreach (var (hotKey, hotKeyId) in _registeredHotkeys.ToArray())
        {
            NativeMethods.UnregisterHotKey(_instance.Handle, hotKeyId);
            NativeMethods.RegisterHotKey(_instance.Handle, hotKeyId, (uint)hotKey.Modifier, (uint)hotKey.Keys);
        }
    }

    /// <summary>
    ///     Ensures the low-level keyboard hook thread is running.
    ///     The hook requires a dedicated message pump, so a background STA thread is used.
    /// </summary>
    private static void EnsureHookThreadRunning()
    {
        if (_hookThread != null && _hookThread.IsAlive)
            return;

        _hookThread = new Thread(HookThreadProc)
        {
            Name = "KeyboardHook",
            IsBackground = true
        };
        _hookThread.SetApartmentState(ApartmentState.STA);
        _hookThread.Start();
    }

    /// <summary>
    ///     Hook thread procedure: installs the low-level keyboard hook and runs a message pump.
    /// </summary>
    private static void HookThreadProc()
    {
        _hookProc = LowLevelKeyboardHookCallback;
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        _hookHandle = Interop.SetWindowsHookEx(13 /* WH_KEYBOARD_LL */, _hookProc,
            Interop.GetModuleHandle(curModule.ModuleName), 0);

        // Message pump required for WH_KEYBOARD_LL to function
        Application.Run();
    }

    /// <summary>
    ///     Low-level keyboard hook callback. Checks pressed keys against hooked hotkeys
    ///     and fires HotKeyPressed for matches.
    /// </summary>
    private static IntPtr LowLevelKeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)0x0100 /* WM_KEYDOWN */)
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
                        _instance.BeginInvoke(() =>
                        {
                            HotKeyPressed?.Invoke(_instance, new KeyPressedEventArgs(hotKey));
                        });
                        return (IntPtr)1; // swallow key to prevent further dispatch
                    }
                }
            }
        }

        return Interop.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }

    /// <summary>
    ///     Converts the enumerable of pressed modifier Keys into a ModifierKeys flags value.
    /// </summary>
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
    private static void CleanupHook()
    {
        if (_hookHandle != IntPtr.Zero)
        {
            Interop.UnhookWindowsHookEx(_hookHandle);
            _hookHandle = IntPtr.Zero;
        }

        if (_hookThread != null && _hookThread.IsAlive)
        {
            // Application.ExitThread will cause the hook thread's Application.Run() to return
            // Post a WM_QUIT to the hook thread's message pump
            Interop.PostThreadMessage((uint)_hookThread.ManagedThreadId, 0x0012 /* WM_QUIT */, IntPtr.Zero, IntPtr.Zero);
            if (!_hookThread.Join(2000))
            {
                Log.Warning("Hook thread did not exit cleanly");
            }
        }
        _hookThread = null;

        lock (_hookedHotkeys)
        {
            _hookedHotkeys.Clear();
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

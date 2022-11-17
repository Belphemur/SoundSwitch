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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Com.User;
using SoundSwitch.Framework.WinApi.Keyboard;

namespace SoundSwitch.Framework.WinApi
{
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
        private static WindowsAPIAdapter _instance;
        private static ThreadExceptionEventHandler _exceptionEventHandler;
        private readonly Dictionary<HotKey, int> _registeredHotkeys = new Dictionary<HotKey, int>();
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

            _instance = new WindowsAPIAdapter();
            _instance.CreateHandle();
            _instance._msgNotifyShell = Interop.RegisterWindowMessage("SHELLHOOK");
            Interop.RegisterShellHookWindow(User32.NativeMethods.HWND.Cast(_instance.Handle));
            Log.Information("Handle created. Running the application.");
            Application.Run(_instance);
            Log.Information("End of the WindowsAPIAdapter thread");
        }

        private void EndForm()
        {
            Close();
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

                Interop.DeregisterShellHookWindow(User32.NativeMethods.HWND.Cast(Handle));
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Registers a HotKey in the system.
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

                    var id = _instance._hotKeyId++;
                    _instance._registeredHotkeys.Add(hotKey, id);
                    // register the hot key.
                    return NativeMethods.RegisterHotKey(_instance.Handle, id, (uint)hotKey.Modifier,
                        (uint)hotKey.Keys);
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
                    if (!_instance._registeredHotkeys.TryGetValue(hotKey, out var id))
                    {
                        Log.Information("Not registered {hotkeys}", hotKey);
                        return false;
                    }

                    _instance._registeredHotkeys.Remove(hotKey);
                    return NativeMethods.UnregisterHotKey(_instance.Handle, id);
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
            }

            if (WindowDestroyed != null && m.Msg == _msgNotifyShell)
            {
                // Receive shell messages
                switch ((Interop.ShellEvents)m.WParam.ToInt32())
                {
                    case Interop.ShellEvents.HSHELL_WINDOWDESTROYED:
                        var hwnd = User32.NativeMethods.HWND.Cast(m.LParam);
                        var (_, windowText, windowClass) = WindowMonitor.ProcessWindowInformation(hwnd);
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

        public class WindowDestroyedEvent : EventArgs
        {
            public User32.NativeMethods.HWND Hwnd { get; }

            public WindowDestroyedEvent(User32.NativeMethods.HWND hwnd)
            {
                Hwnd = hwnd;
            }
        }

        public class RestartManagerEvent : EventArgs
        {
            public RestartManagerEvent(RestartManagerEventType type)
            {
                Result = new IntPtr(1);
                Type = type;
            }

            public IntPtr Result { get; set; }
            public RestartManagerEventType Type { get; }
        }

        public class DeviceChangeEvent : EventArgs
        {
        }

        /// <summary>
        ///     Event Args for the event that is fired after the hot key has been pressed.
        /// </summary>
        public class KeyPressedEventArgs : EventArgs
        {
            public KeyPressedEventArgs(HotKey hotKey)
            {
                HotKey = hotKey;
            }

            public HotKey HotKey { get; set; }
        }

        #endregion
    }
}
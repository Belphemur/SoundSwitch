/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
* Copyright (C) 2015 Antoine Aflalo
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
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SoundSwitch.Util
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
        private readonly Dictionary<HotKeys, int> _registeredHotkeys = new Dictionary<HotKeys, int>();
        private int _hotKeyId;

        private WindowsAPIAdapter()
        {
        }

        public static event EventHandler<RestartManagerEvent> RestartManagerTriggered;
        public static event EventHandler<DeviceChangeEvent> DeviceChanged;
        public static event EventHandler<KeyPressedEventArgs> HotKeyPressed;

        public static void Start()
        {
            var t = new Thread(RunForm);
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }

        public static void Stop()
        {
            if (_instance == null) throw new InvalidOperationException("Notifier not started");
            RestartManagerTriggered = null;
            DeviceChanged = null;
            HotKeyPressed = null;

            if (!_instance.IsDisposed)
                _instance.Invoke(new MethodInvoker(_instance.EndForm));
        }

        private static void RunForm()
        {
            Application.Run(new WindowsAPIAdapter());
        }

        private void EndForm()
        {
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var hotKeyId in _instance._registeredHotkeys.Values)
            {
                NativeMethods.UnregisterHotKey(_instance.Handle, hotKeyId);
            }
            base.Dispose(disposing);
        }

        /// <summary>
        ///     Registers a HotKey in the system.
        /// </summary>
        /// <param name="hotKeys">Represent the hotkey to register</param>
        public static void RegisterHotKey(HotKeys hotKeys)
        {
            _instance.Invoke(new Action(() =>
            {
                if (_instance._registeredHotkeys.ContainsKey(hotKeys))
                    return;

                var id = _instance._hotKeyId++;
                _instance._registeredHotkeys.Add(hotKeys, id);
                // register the hot key.
                if (!NativeMethods.RegisterHotKey(_instance.Handle, id, (uint) hotKeys.Modifier, (uint) hotKeys.Keys))
                    throw new InvalidOperationException("Couldn’t register the hot key.");
            }));
        }

        /// <summary>
        ///     Unregister a registered HotKey
        /// </summary>
        /// <param name="hotKeys"></param>
        /// <returns></returns>
        public static void UnRegisterHotKey(HotKeys hotKeys)
        {
            _instance.Invoke(new Action(() =>
            {
                int id;
                if (!_instance._registeredHotkeys.TryGetValue(hotKeys, out id))
                {
                    return;
                }
                _instance._registeredHotkeys.Remove(hotKeys);
                NativeMethods.UnregisterHotKey(_instance.Handle, id);
            }));
        }

        protected override void SetVisibleCore(bool value)
        {
            // Prevent window getting visible
            if (_instance == null) CreateHandle();
            _instance = this;
            value = false;
            base.SetVisibleCore(value);
        }

        protected override void WndProc(ref Message m)
        {
            //Check for shutdown message from windows
            if (m.Msg == WM_QUERYENDSESSION && m.LParam.ToInt32() == ENDSESSION_CLOSEAPP)
            {
                var closingEvent = new RestartManagerEvent(RestartManagerEventType.Query);
                RestartManagerTriggered?.Invoke(this, closingEvent);
                m.Result = closingEvent.Result;
            }
            else if (m.Msg == WM_ENDSESSION && m.LParam.ToInt32() == ENDSESSION_CLOSEAPP)
            {
                RestartManagerTriggered?.Invoke(this, new RestartManagerEvent(RestartManagerEventType.EndSession));
            }
            else
                switch (m.Msg)
                {
                    case WM_CLOSE:
                        RestartManagerTriggered?.Invoke(this,
                            new RestartManagerEvent(RestartManagerEventType.ForceClose));
                        break;
                    case WM_DEVICECHANGE:
                        DeviceChanged?.Invoke(this, new DeviceChangeEvent());
                        break;
                    case WM_HOTKEY:
                        // get the keys.
                        var key = (Keys) (((int) m.LParam >> 16) & 0xFFFF);
                        var modifier = (HotKeys.ModifierKeys) ((int) m.LParam & 0xFFFF);

                        HotKeyPressed?.Invoke(this, new KeyPressedEventArgs(new HotKeys(key, modifier)));
                        break;
                }

            base.WndProc(ref m);
        }

        #region WindowsNativeMethods

        public static class NativeMethods
        {
            // Registers a hot key with Windows.
            [DllImport("user32.dll")]
            internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

            // Unregisters the hot key with Windows.
            [DllImport("user32.dll")]
            internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        }

        #endregion

        #region Events

        public class RestartManagerEvent : EventArgs
        {
            public RestartManagerEvent(RestartManagerEventType type)
            {
                Result = new IntPtr(0);
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
            public KeyPressedEventArgs(HotKeys hotKeys)
            {
                HotKeys = hotKeys;
            }

            public HotKeys HotKeys { get; set; }
        }

        #endregion
    }
}
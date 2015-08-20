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
using System.Threading;
using System.Windows.Forms;

namespace SoundSwitch.Util
{
    public class WindowsEventNotifier : Form
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
        private static WindowsEventNotifier _instance;

        private WindowsEventNotifier()
        {
        }

        public static event EventHandler<RestartManagerEvent> RestartManagerTriggered;
        public static event EventHandler<DeviceChangeEvent> DeviceChanged;

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
            _instance.EndForm();
        }

        private static void RunForm()
        {
            Application.Run(new WindowsEventNotifier());
        }

        private void EndForm()
        {
            Close();
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
                        RestartManagerTriggered?.Invoke(this, new RestartManagerEvent(RestartManagerEventType.ForceClose));
                        break;
                    case WM_DEVICECHANGE:
                        DeviceChanged?.Invoke(this, new DeviceChangeEvent());
                        break;
                }

            base.WndProc(ref m);
        }
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
        #endregion
    }
}
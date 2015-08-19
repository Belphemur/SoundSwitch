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
    public class CloseNotifier : Form
    {
        private static CloseNotifier _instance;

        public enum ClosingEventType
        {
            Query,
            EndSession
        }

        public class ClosingEvent
        {
            public IntPtr Result { get; set; }
            public ClosingEventType Type { get; }

            public ClosingEvent(ClosingEventType type)
            {
                Result = new IntPtr(0);
                Type = type;
            }
        }
        /**
            #define WM_QUERYENDSESSION              0x0011
            #define WM_ENDSESSION                   0x0016
            #define ENDSESSION_CLOSEAPP         0x00000001
            #define WM_CLOSE                        0x0010
        */
        private const int WM_QUERYENDSESSION = 0x0011;
        private const int WM_ENDSESSION = 0x0016;
        private const int ENDSESSION_CLOSEAPP = 0x00000001;
        private const int WM_CLOSE = 0x0010;

        public delegate void ClosingHandler(object sender, ClosingEvent closingEvent);

        public static event ClosingHandler ApplicationClosing;

        private CloseNotifier()
        {
        }

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
            ApplicationClosing = null;
            _instance.EndForm();
        }
        private static void RunForm()
        {
            Application.Run(new CloseNotifier());
        }

        private void EndForm()
        {
            this.Close();
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
                var closingEvent = new ClosingEvent(ClosingEventType.Query);
                ApplicationClosing?.Invoke(this,closingEvent);
                m.Result = closingEvent.Result;
                return;
            }

            if (m.Msg == WM_CLOSE || (m.Msg == WM_ENDSESSION && m.LParam.ToInt32() == ENDSESSION_CLOSEAPP))
            {
                ApplicationClosing?.Invoke(this, new ClosingEvent(ClosingEventType.EndSession));
            }

            base.WndProc(ref m);
        }
    }
}
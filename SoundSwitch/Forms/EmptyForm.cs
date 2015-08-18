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
using System.Windows.Forms;

namespace SoundSwitch.Forms
{
    public partial class EmptyForm : Form
    {
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
        public EmptyForm()
        {
            InitializeComponent();
        }
        protected override void WndProc(ref Message m)
        {
           //Check for shutdown message from windows
            if (m.Msg == WM_QUERYENDSESSION && m.LParam.ToInt32() == ENDSESSION_CLOSEAPP)
            {
                m.Result = new IntPtr(1);
                return;
            }

            if (m.Msg == WM_CLOSE || (m.Msg == WM_ENDSESSION && m.LParam.ToInt32() == ENDSESSION_CLOSEAPP))
            {
                Application.Exit();
            }

            base.WndProc(ref m);
        }
    }
}
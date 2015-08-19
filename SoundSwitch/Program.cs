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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SoundSwitch.Framework;
using SoundSwitch.Util;

namespace SoundSwitch
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (new Mutex(true, Application.ProductName, out createdNew))
            {
                if (!createdNew) return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += Application_ThreadException;
                WindowsEventNotifier.Start();
                WindowsEventNotifier.EventTriggered += (sender, @event) =>
                {
                    switch (@event.Type)
                    {
                        case WindowsEventNotifier.EventType.Query:
                            @event.Result = new IntPtr(1);
                            break;
                        case WindowsEventNotifier.EventType.EndSession:
                        case WindowsEventNotifier.EventType.ForceClose:
                            Application.Exit();
                            break;
                    }
                    
                };
                var config = ConfigurationManager.LoadConfiguration<SoundSwitchConfiguration>();
                using (var icon = new TrayIcon(new Main(config)))
                {
                    if (config.FirstRun)
                    {
                        icon.ShowSettings();
                       config.FirstRun = false;
                    }

                    Application.Run();
                    WindowsEventNotifier.Stop();
                }

            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var message = string.Format("It seems {1} has crashed.{0}Do you want to save a log of the error that ocurred?{0}This could be useful to fix bugs. Please post this file in the issues section.", Environment.NewLine, Application.ProductName);
            var result = MessageBox.Show(message, $"{Application.ProductName} crashed...", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            if (result == DialogResult.Yes)
            {
                var textToWrite =
                    $"{DateTime.Now}\nException:\n{e.Exception.Message}\n\nInner Exception:\n{e.Exception.InnerException}\n\n\n\n";
                var dialog = new SaveFileDialog();
                dialog.ShowDialog();
                using (var sw = new StreamWriter(dialog.OpenFile()))
                {
                    sw.Write(textToWrite);
                }
            }
        }
    }
}

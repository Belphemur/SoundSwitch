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
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using SoundSwitch.Framework;
using SoundSwitch.Util;

namespace SoundSwitch
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool createdNew;
            AppLogger.Log.Info("Application Starts");
            using (new Mutex(true, Application.ProductName, out createdNew))
            {
                if (!createdNew)
                {
                    AppLogger.Log.Warn("Application already started");
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += Application_ThreadException;
                WindowsAPIAdapter.Start();
                //Manage the Closing events send by Windows
                //Since this app don't use a Form as "main window" the app doesn't close 
                //when it should without this.
                WindowsAPIAdapter.RestartManagerTriggered += (sender, @event) =>
                {
                    using (AppLogger.Log.DebugCall())
                    {
                        AppLogger.Log.Debug("Restart Event recieved", @event);
                        switch (@event.Type)
                        {
                            case WindowsAPIAdapter.RestartManagerEventType.Query:
                                @event.Result = new IntPtr(1);

                                break;
                            case WindowsAPIAdapter.RestartManagerEventType.EndSession:
                            case WindowsAPIAdapter.RestartManagerEventType.ForceClose:
                                AppLogger.Log.Debug("Close Application");
                                Application.Exit();
                                break;
                        }
                    }
                };
                AppLogger.Log.Info("Load Configuration");
                var config = ConfigurationManager.LoadConfiguration<SoundSwitchConfiguration>();
                AppLogger.Log.Info("Set Exception Handler");
                WindowsAPIAdapter.AddThreadExceptionHandler(Application_ThreadException);
                AppLogger.Log.Info("Set Tray Icon with Main");
                try
                {
                    using (var icon = new TrayIcon(new Main(config)))
                    {
                        if (config.FirstRun)
                        {
                            icon.ShowSettings();
                            config.FirstRun = false;
                            AppLogger.Log.Info("First run");
                        }
                        Application.Run();
                        WindowsAPIAdapter.Stop();
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
               
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void HandleException(Exception exception)
        {
            using (AppLogger.Log.FatalCall())
            {
                AppLogger.Log.Fatal("Exception Occured", exception);
                var message =
                    $"It seems {Application.ProductName} has crashed.\nDo you want to save a log of the error that ocurred?\nThis could be useful to fix bugs. Please post this file in the issues section.";
                var result = MessageBox.Show(message, $"{Application.ProductName} crashed...", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                {
                    Process.Start(AppLogger.LogsLocation);
                }
            }
        }
    }
}
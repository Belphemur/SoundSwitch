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
using System.IO;
using System.IO.Compression;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.IPC;
using SoundSwitch.Framework.Minidump;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Model;
using SoundSwitch.Util;

namespace SoundSwitch
{
    internal static class Program
    {
        [HandleProcessCorruptedStateExceptions]
        [STAThread]
        private static void Main()
        {
            bool createdNew;
            AppLogger.Log.Info("Application Starts");
#if !DEBUG
                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    HandleException((Exception)args.ExceptionObject);
                };

                AppLogger.Log.Info("Set Exception Handler");
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
                WindowsAPIAdapter.Start(Application_ThreadException);

#else
            WindowsAPIAdapter.Start();
#endif

            using (new Mutex(true, Application.ProductName, out createdNew))
            {
                if (!createdNew)
                {
                    AppLogger.Log.Warn("Application already started");
                    using (new AppLogger.LogRestartor())
                    {
                        using (var client = new IPCClient(AppConfigs.IPCConfiguration.ClientUrl()))
                        {
                            try
                            {
                                var service = client.GetService();
                                service.StopApplication();
                                RestartApp();
                                return;
                            }
                            catch (RemotingException e)
                            {
                                AppLogger.Log.Error("Can't stop the other app ", e);
                                Application.Exit();
                                return;
                            }
                        }
                    }
                }
                AppModel.Instance.ActiveAudioDeviceLister = new AudioDeviceLister(DeviceState.Active);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

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

                AppLogger.Log.Info("Set Tray Icon with Main");
#if !DEBUG
                try
                {
#endif
                using(var ipcServer = new IPCServer(AppConfigs.IPCConfiguration.ServerUrl()))
                using (var icon = new TrayIcon())
                {
                    var available = false;
                    while (!available)
                    {
                        try
                        {
                            ipcServer.InitServer();
                            available = true;
                        }
                        catch (RemotingException)
                        {
                            Thread.Sleep(250);
                        }
                        
                    }
                    AppModel.Instance.TrayIcon = icon;
                    AppModel.Instance.InitializeMain();
                    AppModel.Instance.NewVersionReleased += (sender, @event) =>
                    {
                        if (@event.UpdateState != UpdateState.Steath)
                        {
                            return;
                        }
                        new AutoUpdater("/VERYSILENT /NOCANCEL /NORESTART", ApplicationPath.Default).Update(@event.Release, true);
                    };
                    if (AppConfigs.Configuration.FirstRun)
                    {
                        icon.ShowSettings();
                        AppConfigs.Configuration.FirstRun = false;
                        AppLogger.Log.Info("First run");
                    }
                    Application.Run();
                }
#if !DEBUG
                }
               
                catch (Exception ex)
                {
                    HandleException(ex);

                }
#endif
            }
            WindowsAPIAdapter.Stop();
        }

        private static void RestartApp()
        {
            var info = new ProcessStartInfo
            {
                Arguments = "/C ping 127.0.0.1 -n 2 && \"" + Application.ExecutablePath + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            Process.Start(info);
            Application.Exit();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private static void HandleException(Exception exception)
        {
            if (exception == null)
                return;
            var zipFile = Path.Combine(ApplicationPath.AppData,
                $"{Application.ProductName}-crashlog-{DateTime.UtcNow.Date.Day}_{DateTime.UtcNow.Date.Month}_{DateTime.UtcNow.Date.Year}.zip");
            var message =
                $"It seems {Application.ProductName} has crashed.\n" +
                $"Do you want to save a log of the error that ocurred?\n" +
                $"This could be useful to fix bugs. Please post this file in the issues section.\n" +
                $"File Location: " + zipFile;
            var result = MessageBox.Show(message, $"{Application.ProductName} crashed...", MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);

            if (result == DialogResult.Yes)
            {
                using (new HourGlass())
                {
                    var fileName = Path.Combine(ApplicationPath.Default, Environment.MachineName + ".dmp");
                    using (
                        var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite,
                            FileShare.Write))
                    {
                        MiniDump.Write(fs.SafeFileHandle,
                            MiniDump.Option.Normal | MiniDump.Option.WithThreadInfo | MiniDump.Option.WithHandleData |
                            MiniDump.Option.WithDataSegs, MiniDump.ExceptionInfo.Present);
                    }
                    AppLogger.Log.Fatal("Exception Occured ", exception);
                    using (new AppLogger.LogRestartor())
                    {
                        if (File.Exists(zipFile))
                        {
                            File.Delete(zipFile);
                        }
                        
                        ZipFile.CreateFromDirectory(ApplicationPath.Default, zipFile);
                    }
                    Process.Start("explorer.exe", "/select," + @zipFile);
                }
            }
        }
    }
}

﻿/********************************************************************
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
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Logger.Configuration;
using SoundSwitch.Framework.Minidump;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Updater;
using SoundSwitch.InterProcess.Communication;
using SoundSwitch.Localization.Factory;
using SoundSwitch.Model;
using SoundSwitch.UI.Component;
using SoundSwitch.Util;
using WindowsAPIAdapter = SoundSwitch.Framework.WinApi.WindowsAPIAdapter;

namespace SoundSwitch
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [HandleProcessCorruptedStateExceptions]
        [STAThread]
        private static void Main()
        {
            InitializeLogger();
            Log.Information("Application Starts");
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                HandleException((Exception) args.ExceptionObject);
            };

            Log.Information("Set Exception Handler");
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            WindowsAPIAdapter.Start(Application_ThreadException);
#else
            WindowsAPIAdapter.Start();
#endif
            Thread.CurrentThread.CurrentUICulture = new LanguageFactory().Get(AppModel.Instance.Language).CultureInfo;
            Thread.CurrentThread.Name = "Main Thread";
            var userMutexName = Application.ProductName + Environment.UserName;

            using var mainMutex = new Mutex(true, Application.ProductName);
            using var userMutex = new Mutex(true, userMutexName, out var userMutexInUse);

            if (KillOtherInstanceAndRestart(userMutexName, userMutexInUse))
            {
                WindowsAPIAdapter.Stop();
                Log.CloseAndFlush();
                return;
            }


            SetProcessDPIAware();

            Application.EnableVisualStyles();
#if NETCORE
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
#endif
            Application.SetCompatibleTextRenderingDefault(false);

            // Manage the Closing events send by Windows
            // Since this app don't use a Form as "main window" the app doesn't close
            // when it should without this.
            WindowsAPIAdapter.RestartManagerTriggered += (sender, @event) =>
            {
                Log.Debug("Restart Event received: {Event}", @event);
                switch (@event.Type)
                {
                    case WindowsAPIAdapter.RestartManagerEventType.Query:
                        @event.Result = new IntPtr(1);

                        break;
                    case WindowsAPIAdapter.RestartManagerEventType.EndSession:
                    case WindowsAPIAdapter.RestartManagerEventType.ForceClose:
                        Log.Debug("Close Application");
                        Application.Exit();
                        break;
                }
            };

            Log.Information("Set Tray Icon with Main");
#if !DEBUG
            try
            {
#endif
                MMNotificationClient.Instance.Register();


                using var ctx = new WindowsFormsSynchronizationContext();

                SynchronizationContext.SetSynchronizationContext(ctx);
                try
                {
                    Application.Run(new SoundSwitchApplicationContext());
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(null);
                }


#if !DEBUG
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
#endif
            AppModel.Instance.ActiveAudioDeviceLister.Dispose();
            AppModel.Instance.ActiveUnpluggedAudioLister.Dispose();
            AppModel.Instance.TrayIcon.Dispose();
            MMNotificationClient.Instance.UnRegister();
            WindowsAPIAdapter.Stop();
            Log.CloseAndFlush();

        }

        private static bool KillOtherInstanceAndRestart(string pipeName, bool createdNew)
        {
            //Mutex used by another instance of the app
            //Ask the other instance to stop and restart this instance to get the mutex again.
            if (!createdNew)
            {
                using var pipeClient = new NamedPipeClient(pipeName);
                Log.Information("Other instance detected.");
                pipeClient.SendMsg("Close");
                Log.Information("Other instance detected: asked to stop, restarting now.");
                RestartApp();
                return true;
            }

            using var pipeServer = new NamedPipeServer(pipeName);
            pipeServer.Start(message =>
            {
                if (message == "Close")
                {
                    Log.Information("Other instance detected and asked to stop.");
                    Application.Exit();
                }
            });
            return false;
        }

        /// <summary>
        /// Initialize the logger
        /// </summary>
        private static void InitializeLogger()
        {
            LoggerConfigurator.ConfigureLogger();
            Log.Information(
                $"{Application.ProductName}  {AssemblyUtils.GetReleaseState()} ({Application.ProductVersion})");
            Log.Information($"OS: {Environment.OSVersion}");
            Log.Information($"Framework: {Environment.Version}");
        }

        /// <summary>
        /// Restarts the application itself.
        /// </summary>
        public static void RestartApp()
        {
            var info = new ProcessStartInfo
            {
                Arguments = $"/C ping 127.0.0.1 -n 3 && \"{ApplicationPath.Executable}\"",
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
            var exceptionMessage = exception.Message;
            if (exception.InnerException != null)
            {
                exceptionMessage += $"\n{exception.InnerException.Message}";
            }

            var message =
                $@"It seems {Application.ProductName} has crashed.

{exceptionMessage}

Do you want to save a log of the error that occurred?
This could be useful to fix bugs. Please post this file in the issues section
File Location: {zipFile}";
            var result = MessageBox.Show(message, $@"{Application.ProductName} crashed...", MessageBoxButtons.YesNo,
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

                    Log.Fatal(exception, "Exception Occurred ");

                    if (File.Exists(zipFile))
                    {
                        File.Delete(zipFile);
                    }

                    Log.CloseAndFlush();

                    ZipFile.CreateFromDirectory(ApplicationPath.Default, zipFile);
                }

                Process.Start("explorer.exe", "/select," + @zipFile);
            }
        }
    }
}
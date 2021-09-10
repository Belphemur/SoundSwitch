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
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ContribSentry;
using Sentry;
using Serilog;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Logger.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Localization.Factory;
using SoundSwitch.Model;
using SoundSwitch.Util;
using SoundSwitch.Util.Url;

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
            var sentryOptions = new SentryOptions
            {
                Dsn = "https://7d52dfb4f6554bf0b58b256337835332@o631137.ingest.sentry.io/5755327",
                Environment = AssemblyUtils.GetReleaseState().ToString(),
                Release = $"{Application.ProductName}@{Application.ProductVersion}",
            };
            var user = new User
            {
                Id = AppConfigs.Configuration.UniqueInstallationId.ToString(),
                Username = Environment.UserName
            };
            //Only track session if Telemetry is enabled
            if (AppConfigs.Configuration.Telemetry)
            {
                var contribOptions = new ContribSentryOptions(true, true, true)
                {
                    GlobalSessionMode = true,
                    CacheDirPath = Path.Combine(ApplicationPath.Default, "Session")
                };
                sentryOptions.AddIntegration(new ContribSentrySdkIntegration(contribOptions));
            }

            using var _ = SentrySdk.Init(sentryOptions);
            //Needs to be started AFTER the init of the main SDK
            //else the ContribSentrySdk isn't enabled and no tracking is done
            if (AppConfigs.Configuration.Telemetry)
            {
                ContribSentrySdk.StartSession(user);
            }


            SentrySdk.ConfigureScope(scope => { scope.User = user; });

            InitializeLogger();
            Log.Information("Application Starts");
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => { HandleException((Exception)args.ExceptionObject); };

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
            using var userMutex = new Mutex(true, userMutexName, out var userMutexHasOwnership);
            if (!userMutexHasOwnership)
            {
                Log.Warning("SoundSwitch is already running for this user {@Mutex}", userMutexName);
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
                        Environment.Exit(0);
                        break;
                }
            };

            Log.Information("Starting Application context");
#if !DEBUG
            try
            {
#endif
            MMNotificationClient.Instance.Register();


            var ctx = new WindowsFormsSynchronizationContext();

            SynchronizationContext.SetSynchronizationContext(ctx);
            Application.Run(new SoundSwitchApplicationContext());


#if !DEBUG
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
#endif
            ContribSentrySdk.EndSession();
            AppModel.Instance.Dispose();
            WindowsAPIAdapter.Stop();
            MMNotificationClient.Instance?.Dispose();
            Log.CloseAndFlush();
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

            SentryId eventId = default;
            SentrySdk.WithScope(scope =>
                {
                    scope.AddAttachment(AppConfigs.Configuration.FileLocation);
                    eventId = SentrySdk.CaptureException(exception);
                }
            );

            var exceptionMessage = exception.Message;
            if (exception.InnerException != null)
            {
                exceptionMessage += $"\n{exception.InnerException.Message}";
            }

            var message =
                $@"It seems {Application.ProductName} has crashed.

{exceptionMessage}

Would you like to share more information with the developers?";
            var result = DialogResult.None;
            SynchronizationContext.Current.Send(state =>
            {
                result = MessageBox.Show(message, $@"{Application.ProductName} crashed...", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
            }, null);
   

            if (result != DialogResult.Yes) return;

            using (new HourGlass())
            {
                BrowserUtil.OpenUrl($"https://soundswitch.aaflalo.me/#sentry?eventId={eventId}");
            }
        }
    }
}
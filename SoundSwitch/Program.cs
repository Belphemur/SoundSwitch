/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2024 Antoine Aflalo
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
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sentry;
using Serilog;
using SoundSwitch.Common.Framework.Pipe;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Logger.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Localization.Factory;
using SoundSwitch.Model;
using SoundSwitch.Util;
using SoundSwitch.Util.Url;

namespace SoundSwitch
{
    internal static class Program
    {
        private static WindowsFormsSynchronizationContext _synchronizationContext;

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        private static async Task Main()
        {
            using var mainCts = new CancellationTokenSource();
            var sentryOptions = new SentryOptions
            {
                Dsn = "https://7d52dfb4f6554bf0b58b256337835332@o631137.ingest.sentry.io/5755327",
                Environment = AssemblyUtils.GetReleaseState().ToString(),
                DefaultTags = { { "ReleaseState", AssemblyUtils.GetReleaseState().ToString() } },
                Release = $"{Application.ProductName}@{Application.ProductVersion}",
                // Only track session if telemetry is enabled
                AutoSessionTracking = AppConfigs.Configuration.Telemetry,
            };
            var user = new SentryUser
            {
                Id = AppConfigs.Configuration.UniqueInstallationId.ToString(),
                Username = Environment.UserName
            };

            using var _ = SentrySdk.Init(sentryOptions);


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
                try
                {
                    var response = await NamedPipe.SendRequestAsync<OpenSettingsResponse>(userMutexName, new OpenSettingsRequest());
                    if (!response.Success)
                    {
                        Log.Error("Failed to open settings in existing instance");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to communicate with existing instance");
                }
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
                        try
                        {
                            mainCts.Cancel();
                        }
                        catch (Exception)
                        {
                            // We're closing anyway
                        }

                        Environment.Exit(0);
                        break;
                }
            };

            Log.Information("Starting Application context");
#if !DEBUG
            try
            {
#endif
            _synchronizationContext = new WindowsFormsSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

            NamedPipe.StartListening(userMutexName, mainCts.Token);

            Application.Run(new SoundSwitchApplicationContext());


#if !DEBUG
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
#endif
            SentrySdk.EndSession();
            AppModel.Instance.Dispose();
            WindowsAPIAdapter.Stop();
            MMNotificationClient.Instance?.Dispose();
            NamedPipe.Cleanup();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            JobScheduler.Instance.StopAsync(cts.Token).GetAwaiter().GetResult();
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

            var eventId = SentrySdk.CaptureException(exception, scope => { scope.AddAttachment(AppConfigs.Configuration.FileLocation); });

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
            var syncContext = _synchronizationContext ?? SynchronizationContext.Current;
            syncContext?.Send(state =>
            {
                try
                {
                    try
                    {
                        result = MessageBox.Show(message, $@"{Application.ProductName} crashed...", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Error);
                    }
                    catch (InvalidOperationException)
                    {
                        result = MessageBox.Show(message, $@"{Application.ProductName} crashed...", MessageBoxButtons.YesNo,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    }
                }
                catch (Exception)
                {
                    Log.Warning("Couldn't warn the user about the crash");
                }
            }, null);


            if (result != DialogResult.Yes) return;

            using (new HourGlass())
            {
                BrowserUtil.OpenUrl($"https://soundswitch.aaflalo.me/#sentry?eventId={eventId}");
            }
        }
    }
}

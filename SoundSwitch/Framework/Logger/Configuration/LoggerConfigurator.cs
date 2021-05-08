using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using SoundSwitch.Framework.Logger.Enricher;
using SoundSwitch.Util;

namespace SoundSwitch.Framework.Logger.Configuration
{
    public static class LoggerConfigurator
    {
        public static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Debug()
                         .Enrich.WithThreadId()
                         .Enrich.WithEnvironmentUserName()
                         .Enrich.WithExceptionDetails()
                         .Enrich.WithCaller()
#if DEBUG
                         .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
#endif
                         .WriteTo.File(Path.Combine(ApplicationPath.Logs, "soundswitch.log"),
                             rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3,
                             flushToDiskInterval: TimeSpan.FromMinutes(10),
                             outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message} (at {Caller}){NewLine}{Exception}")
                         .WriteTo.Sentry(o =>
                         {
                             o.InitializeSdk = false;
                             o.Dsn = "https://4961ba10a02d43209b7ae7c5df56c81e@o631137.ingest.sentry.io/5755327";
                             o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                             o.MinimumEventLevel = LogEventLevel.Error;
                             o.Environment = AssemblyUtils.GetReleaseState().ToString();
                         })
                         .CreateLogger();
            var listener = new global::SerilogTraceListener.SerilogTraceListener();
            Trace.Listeners.Add(listener);
        }
    }
}
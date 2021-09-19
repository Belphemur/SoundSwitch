using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using SoundSwitch.Framework.Logger.Enricher;

namespace SoundSwitch.Framework.Logger.Configuration
{
    public static class LoggerConfigurator
    {
        public static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
#if RELEASE
                         .MinimumLevel.Debug()
#else
                         .MinimumLevel.Verbose()
#endif
                         .Enrich.WithThreadId()
                         .Enrich.WithExceptionDetails()
                         .Enrich.WithCaller()
#if DEBUG
                         .WriteTo.Console(LogEventLevel.Verbose, "[{Timestamp:HH:mm:ss.fff} {Level:u3}][{ThreadId}] {Properties} {Message} (at {Caller}){NewLine}{Exception}", theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
#endif
                         .WriteTo.File(Path.Combine(ApplicationPath.Logs, "soundswitch.log"),
                             rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3,
                             flushToDiskInterval: TimeSpan.FromMinutes(10),
                             outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}][{ThreadId}] {Properties} {Message} (at {Caller}){NewLine}{Exception}")
                         .WriteTo.Sentry(o =>
                         {
                             o.InitializeSdk = false;
                             o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                             o.MinimumEventLevel = LogEventLevel.Error;
                         })
                         .CreateLogger();
            var listener = new global::SerilogTraceListener.SerilogTraceListener();
            Trace.Listeners.Add(listener);
        }
    }
}
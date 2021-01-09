using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using Serilog.Exceptions;
using SoundSwitch.Framework.Logger.Enricher;

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
                         .CreateLogger();
            var listener = new global::SerilogTraceListener.SerilogTraceListener();
            Trace.Listeners.Add(listener);
        }
    }
}
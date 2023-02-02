using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Display;
using SoundSwitch.Framework.Logger.Enricher;

namespace SoundSwitch.Framework.Logger.Configuration
{
    public static class LoggerConfigurator
    {
        public static void ConfigureLogger()
        {
            const string sentryTemplate = "{Message}\n{Properties}";
            const string outputTemplate = "[{Timestamp:HH:mm:ss.fff} {Level:u3}][{ThreadId}]" + sentryTemplate + "(at {Caller}){NewLine}{Exception}";
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
                         .WriteTo.Console(LogEventLevel.Verbose, outputTemplate, theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
#endif
                         .WriteTo.File(Path.Combine(ApplicationPath.Logs, "soundswitch.log"),
                             rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3,
                             flushToDiskInterval: TimeSpan.FromMinutes(5),
                             outputTemplate: outputTemplate)
                         .WriteTo.Sentry(o =>
                         {
                             o.InitializeSdk = false;
                             o.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                             o.MinimumEventLevel = LogEventLevel.Error;
                             o.TextFormatter = new MessageTemplateTextFormatter(sentryTemplate);
                         })
                         .CreateLogger();
            var listener = new global::SerilogTraceListener.SerilogTraceListener();
            Trace.Listeners.Add(listener);
        }
    }
}
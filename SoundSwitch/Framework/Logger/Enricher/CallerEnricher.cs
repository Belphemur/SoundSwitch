using System.Diagnostics;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace SoundSwitch.Framework.Logger.Enricher;

class CallerEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var skip = 3;
        while (true)
        {
            var stack = new StackFrame(skip);
            var method = stack.GetMethod();
            if (method == null)
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
                return;
            }

            if (method.DeclaringType.Assembly != typeof(Log).Assembly &&//Exclude serilog methods
                method.DeclaringType.Name != "SerilogLogger")//Exclude methods from the SerilogLogger implementation class
            {
                var caller = $"{method.DeclaringType.FullName}.{method.Name}";
                logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
                return;
            }

            skip++;
        }
    }
}

static class LoggerCallerEnrichmentConfiguration
{
    public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<CallerEnricher>();
    }
}
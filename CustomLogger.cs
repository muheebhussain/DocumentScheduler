using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.AzureBlobStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

public class CustomLoggerProvider : ILoggerProvider
{
    private readonly Func<string, LogLevel, bool> _filter;
    private readonly string _logFileNameTemplate;
    private readonly ILogEventSink _sink;
    private readonly IDictionary<string, string> _logFileNames;

    public CustomLoggerProvider(string logFileNameTemplate, ILogEventSink sink, Func<string, LogLevel, bool> filter)
    {
        _logFileNameTemplate = logFileNameTemplate;
        _sink = sink;
        _filter = filter;
        _logFileNames = new Dictionary<string, string>();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new CustomLogger(categoryName, _logFileNameTemplate, _sink, _filter, _logFileNames);
    }

    public void Dispose()
    {
    }
}

public class CustomLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logFileNameTemplate;
    private readonly ILogEventSink _sink;
    private readonly Func<string, LogLevel, bool> _filter;
    private readonly IDictionary<string, string> _logFileNames;

    public CustomLogger(string categoryName, string logFileNameTemplate, ILogEventSink sink, Func<string, LogLevel, bool> filter, IDictionary<string, string> logFileNames)
    {
        _categoryName = categoryName;
        _logFileNameTemplate = logFileNameTemplate;
        _sink = sink;
        _filter = filter;
        _logFileNames = logFileNames;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _filter == null || _filter(_categoryName, logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var logEvent = new LogEvent(DateTimeOffset.Now, LogLevelToSerilogLevel(logLevel), exception, state == null ? null : formatter(state, exception));

        var logFileName = GetLogFileName();
        _sink.Emit(logEvent.WithProperty("ServiceName", _categoryName), logFileName);
    }

    private LogEventLevel LogLevelToSerilogLevel(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                return LogEventLevel.Debug;
            case LogLevel.Information:
                return LogEventLevel.Information;
            case LogLevel.Warning:
                return LogEventLevel.Warning;
            case LogLevel.Error:
                return LogEventLevel.Error;
            case LogLevel.Critical:
                return LogEventLevel.Fatal;
            default:
                return LogEventLevel.Verbose;
        }
    }

    private string GetLogFileName()
    {
        if (_logFileNames.TryGetValue(_categoryName, out var logFileName))
        {
            return logFileName;
        }

        var newLogFileName = _logFileNameTemplate.Replace("{ServiceName}", _categoryName);
        _logFileNames.Add(_categoryName, newLogFileName);
        return newLogFileName;
    }
}

public class NullScope : IDisposable
{
    public static NullScope Instance { get; } = new NullScope();

    public void Dispose()
    {
    }
}

public class MyService
{
private readonly ILogger<MyService> _logger;

public MyService(ILogger<MyService> logger)
{
    _logger = logger;
}

public void DoSomething()
{
    // Log a message to the default log file
    _logger.LogInformation("Default log file");

    // Log a message to a separate log file
    using (_logger.BeginScope(new Dictionary<string, object> { { "ServiceName", "OtherService" } }))
    {
        _logger.LogInformation("Separate log file");
    }

    // Log another message to the separate log file
    using (_logger.BeginScope(new Dictionary<string, object> { { "ServiceName", "OtherService" } }))
    {
        _logger.LogInformation("More messages in separate log file");
    }
}
}

public class Program
{
public static void Main()
{
var serviceProvider = new ServiceCollection()
.AddLogging(loggingBuilder =>
{
loggingBuilder.AddProvider(new CustomLoggerProvider("{ServiceName}.log", new AzureBlobStorageSink("<connection-string>", "<container-name>"), null));
})
.AddTransient<MyService>()
.BuildServiceProvider();


    var myService = serviceProvider.GetService<MyService>();
    myService.DoSomething();
}
}



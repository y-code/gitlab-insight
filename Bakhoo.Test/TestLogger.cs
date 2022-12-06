using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Bakhoo.Test;

public class TestLogger : ILogger, IDisposable
{
    public static LogLevel MinimumLevel { get; set; } = LogLevel.Trace;

    private readonly ITestOutputHelper _output;

    public TestLogger(ITestOutputHelper output)
	{
        _output = output;
	}

    public void Dispose()
    {
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return this;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= MinimumLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _output.WriteLine(formatter(state, exception));
    }
}


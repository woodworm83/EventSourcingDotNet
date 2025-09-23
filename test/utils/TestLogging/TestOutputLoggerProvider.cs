using Microsoft.Extensions.Logging;
using System.Text;
using Xunit.Abstractions;

namespace TestLogging;

public sealed class TestOutputLoggerProvider(
    ITestOutputHelper outputHelper,
    IExternalScopeProvider? externalScopeProvider = null) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new Logger(categoryName, outputHelper, externalScopeProvider);

    public void Dispose()
    {
        // nothing to dispose
    }

    private sealed class Logger(
        string categoryName,
        ITestOutputHelper outputHelper,
        IExternalScopeProvider? externalScopeProvider) : ILogger
    {
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var stringBuilder = new StringBuilder();
            
            stringBuilder.AppendLine($"[{FormatLogLevel(logLevel)}] {categoryName}");
            stringBuilder.AppendLine($"  {formatter(state, exception)}");

            if (exception is not null)
            {
                stringBuilder.AppendLine(exception.ToString());
            }
            
            outputHelper.WriteLine(stringBuilder.ToString());
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => externalScopeProvider?.Push(state);

        private static string FormatLogLevel(LogLevel logLevel)
            => logLevel switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "Unknown log level"),
            };
    }
}
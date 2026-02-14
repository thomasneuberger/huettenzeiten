using Microsoft.Extensions.Logging;

namespace HuettenZeiten.Cli.Logging;

public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _logFilePath;
    private static readonly object _lock = new();

    public FileLogger(string categoryName, string logFilePath)
    {
        _categoryName = categoryName;
        _logFilePath = logFilePath;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_categoryName}: {message}";
        
        if (exception != null)
        {
            logEntry += Environment.NewLine + exception;
        }

        lock (_lock)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_logFilePath) ?? string.Empty);
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Silently fail if we can't write to the log file
            }
        }
    }
}

using Microsoft.Extensions.Logging;

namespace HuettenZeiten.Cli.Logging;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;

    public FileLoggerProvider(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _logFilePath);
    }

    public void Dispose()
    {
    }
}

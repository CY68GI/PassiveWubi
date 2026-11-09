using System.Diagnostics;
using System.Text;

namespace PassiveWubi.Services;

public sealed class LogService
{
    private readonly object _sync = new();

    public LogService()
    {
        Directory.CreateDirectory(AppPaths.LogDirectory);
    }

    public string TodayLogFile =>
        Path.Combine(AppPaths.LogDirectory, $"PassiveWubi-{DateTime.Now:yyyyMMdd}.log");

    public void Info(string message) => Write("INFO", message);

    public void Warning(string message) => Write("WARN", message);

    public void Error(string message, Exception? exception = null)
    {
        var detail = exception is null
            ? message
            : $"{message}{Environment.NewLine}{exception}";
        Write("ERROR", detail);
    }

    public void OpenLogFolder()
    {
        Directory.CreateDirectory(AppPaths.LogDirectory);
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{AppPaths.LogDirectory}\"",
            UseShellExecute = true
        });
    }

    public void OpenTodayLog()
    {
        Info("Today's log opened from UI.");
        Process.Start(new ProcessStartInfo
        {
            FileName = TodayLogFile,
            UseShellExecute = true
        });
    }

    private void Write(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.LogDirectory);
            var builder = new StringBuilder()
                .Append(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"))
                .Append(" [")
                .Append(level)
                .Append("] ")
                .AppendLine(message);

            lock (_sync)
            {
                File.AppendAllText(TodayLogFile, builder.ToString(), new UTF8Encoding(false));
            }
        }
        catch
        {
            // Logging must never create a second application failure.
        }
    }
}

using System.Text;

namespace PassiveWubi.Linux.Services;

public sealed class LogService
{
    private readonly object _sync = new();

    public string TodayLogFile =>
        Path.Combine(LinuxAppPaths.LogDirectory, $"PassiveWubi-{DateTime.Now:yyyyMMdd}.log");

    public void Info(string message) => Write("INFO", message);

    public void Error(string message, Exception? exception = null) =>
        Write("ERROR", exception is null ? message : $"{message}{Environment.NewLine}{exception}");

    public void OpenLogFolder() => OpenPath(LinuxAppPaths.LogDirectory);

    public void OpenTodayLog()
    {
        Info("Today's log opened.");
        OpenPath(TodayLogFile);
    }

    private void Write(string level, string message)
    {
        try
        {
            Directory.CreateDirectory(LinuxAppPaths.LogDirectory);
            lock (_sync)
            {
                File.AppendAllText(
                    TodayLogFile,
                    $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [{level}] {message}{Environment.NewLine}",
                    new UTF8Encoding(false));
            }
        }
        catch
        {
            // Logging must not terminate the application.
        }
    }

    private static void OpenPath(string path)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "xdg-open",
            ArgumentList = { path },
            UseShellExecute = false
        });
    }
}

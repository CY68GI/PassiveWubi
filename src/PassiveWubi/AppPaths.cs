using System.Diagnostics;

namespace PassiveWubi;

public static class AppPaths
{
    public static string BaseDirectory =>
        Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);

    public static string SettingsFile => Path.Combine(BaseDirectory, "settings.json");

    public static string LogDirectory => Path.Combine(BaseDirectory, "logs");

    public static string ExecutablePath =>
        Environment.ProcessPath
        ?? Process.GetCurrentProcess().MainModule?.FileName
        ?? Path.Combine(BaseDirectory, "PassiveWubi.exe");
}

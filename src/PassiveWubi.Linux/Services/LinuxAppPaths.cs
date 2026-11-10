namespace PassiveWubi.Linux.Services;

public static class LinuxAppPaths
{
    private static readonly Lazy<string> WritableDataRoot = new(ResolveWritableDataRoot);

    public static string BaseDirectory => Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);

    public static string DataDirectory => WritableDataRoot.Value;

    public static string SettingsFile => Path.Combine(DataDirectory, "settings.json");

    public static string LogDirectory => Path.Combine(DataDirectory, "logs");

    public static string AutostartFile
    {
        get
        {
            var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            if (string.IsNullOrWhiteSpace(configHome))
            {
                configHome = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config");
            }

            return Path.Combine(configHome, "autostart", "passivewubi.desktop");
        }
    }

    public static string ExecutablePath =>
        Environment.ProcessPath
        ?? Path.Combine(BaseDirectory, "PassiveWubi.Linux");

    private static string ResolveWritableDataRoot()
    {
        try
        {
            Directory.CreateDirectory(Path.Combine(BaseDirectory, "logs"));
            var probe = Path.Combine(BaseDirectory, $".write-test-{Environment.ProcessId}");
            using (File.Create(probe, 1, FileOptions.DeleteOnClose))
            {
            }

            return BaseDirectory;
        }
        catch
        {
            var stateHome = Environment.GetEnvironmentVariable("XDG_STATE_HOME");
            if (string.IsNullOrWhiteSpace(stateHome))
            {
                stateHome = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".local", "state");
            }

            var fallback = Path.Combine(stateHome, "passivewubi");
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    }
}

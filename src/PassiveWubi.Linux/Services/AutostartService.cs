namespace PassiveWubi.Linux.Services;

public sealed class AutostartService
{
    private readonly LogService _log;

    public AutostartService(LogService log)
    {
        _log = log;
    }

    public bool IsEnabled() => File.Exists(LinuxAppPaths.AutostartFile);

    public void SetEnabled(bool enabled)
    {
        try
        {
            if (!enabled)
            {
                if (File.Exists(LinuxAppPaths.AutostartFile))
                {
                    File.Delete(LinuxAppPaths.AutostartFile);
                }

                return;
            }

            var directory = Path.GetDirectoryName(LinuxAppPaths.AutostartFile);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                LinuxAppPaths.AutostartFile,
                $"""
                [Desktop Entry]
                Type=Application
                Name=肌忆五笔
                Comment=PassiveWubi 五笔置顶学习键盘
                Exec="{LinuxAppPaths.ExecutablePath}"
                Terminal=false
                X-GNOME-Autostart-enabled=true
                """);
        }
        catch (Exception ex)
        {
            _log.Error("Unable to change Linux autostart entry.", ex);
        }
    }
}

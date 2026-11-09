using Microsoft.Win32;

namespace PassiveWubi.Services;

public sealed class StartupService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "PassiveWubi";
    private readonly LogService _log;

    public StartupService(LogService log)
    {
        _log = log;
    }

    public bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
            return key?.GetValue(ValueName) is string value
                && string.Equals(value, BuildCommand(), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _log.Error("Unable to inspect Windows startup registration.", ex);
            return false;
        }
    }

    public bool SetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKey, true);
            if (enabled)
            {
                key.SetValue(ValueName, BuildCommand(), RegistryValueKind.String);
                _log.Info("Windows startup registration enabled.");
            }
            else
            {
                key.DeleteValue(ValueName, false);
                _log.Info("Windows startup registration disabled.");
            }

            return true;
        }
        catch (Exception ex)
        {
            _log.Error("Unable to change Windows startup registration.", ex);
            return false;
        }
    }

    private static string BuildCommand() => $"\"{AppPaths.ExecutablePath}\"";
}

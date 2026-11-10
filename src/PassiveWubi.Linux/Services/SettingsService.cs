using System.Text.Json;
using System.Text.Json.Serialization;
using PassiveWubi.Linux.Models;

namespace PassiveWubi.Linux.Services;

public sealed class SettingsService
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly LogService _log;

    public SettingsService(LogService log)
    {
        _log = log;
    }

    public LinuxAppSettings Load()
    {
        try
        {
            if (!File.Exists(LinuxAppPaths.SettingsFile))
            {
                return new LinuxAppSettings();
            }

            var settings = JsonSerializer.Deserialize<LinuxAppSettings>(
                File.ReadAllText(LinuxAppPaths.SettingsFile), _options)
                ?? new LinuxAppSettings();
            settings.Normalize();
            return settings;
        }
        catch (Exception ex)
        {
            _log.Error("Unable to load Linux settings.", ex);
            return new LinuxAppSettings();
        }
    }

    public void Save(LinuxAppSettings settings)
    {
        try
        {
            settings.Normalize();
            var directory = Path.GetDirectoryName(LinuxAppPaths.SettingsFile);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                LinuxAppPaths.SettingsFile,
                JsonSerializer.Serialize(settings, _options));
        }
        catch (Exception ex)
        {
            _log.Error("Unable to save Linux settings.", ex);
        }
    }
}

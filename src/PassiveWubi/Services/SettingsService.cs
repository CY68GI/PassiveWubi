using System.Text.Json;
using System.Text.Json.Serialization;
using PassiveWubi.Models;

namespace PassiveWubi.Services;

public sealed class SettingsService
{
    private readonly LogService _log;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public SettingsService(LogService log)
    {
        _log = log;
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(AppPaths.SettingsFile))
            {
                _log.Info("No settings file found; defaults will be used.");
                return new AppSettings();
            }

            var json = File.ReadAllText(AppPaths.SettingsFile);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions)
                ?? new AppSettings();
            settings.Normalize();
            _log.Info($"Settings loaded from {AppPaths.SettingsFile}.");
            return settings;
        }
        catch (Exception ex)
        {
            _log.Error("Unable to load settings; defaults will be used.", ex);
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            settings.Normalize();
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(AppPaths.SettingsFile, json);
        }
        catch (Exception ex)
        {
            _log.Error("Unable to save settings.", ex);
        }
    }
}

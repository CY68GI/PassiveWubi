using System.Text.Json.Serialization;
using PassiveWubi.Core;

namespace PassiveWubi.Linux.Models;

[JsonConverter(typeof(JsonStringEnumConverter<LinuxTheme>))]
public enum LinuxTheme
{
    Dark,
    Light
}

public sealed class LinuxAppSettings
{
    public WubiVersion Version { get; set; } = WubiVersion.V86;
    public LinuxTheme Theme { get; set; } = LinuxTheme.Dark;
    public double KeyboardOpacity { get; set; } = 0.90;
    public double KeyboardScale { get; set; } = 1.0;
    public bool ShowSimpleCodes { get; set; } = true;
    public bool ShowRootTable { get; set; } = true;
    public bool PassiveHighlight { get; set; } = true;
    public bool ShowKeyboardOnStartup { get; set; } = true;
    public bool AutoCheckUpdates { get; set; } = true;
    public bool LaunchAtStartup { get; set; }
    public bool HasCompletedFirstRun { get; set; }
    public double KeyboardLeft { get; set; } = -1;
    public double KeyboardTop { get; set; } = -1;

    public void Normalize()
    {
        KeyboardOpacity = Math.Clamp(KeyboardOpacity, 0.25, 1.0);
        KeyboardScale = Math.Clamp(KeyboardScale, 0.60, 1.45);
    }
}

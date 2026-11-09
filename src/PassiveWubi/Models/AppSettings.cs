using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace PassiveWubi.Models;

[JsonConverter(typeof(JsonStringEnumConverter<WubiVersion>))]
public enum WubiVersion
{
    [Description("86 版")]
    V86,

    [Description("98 版")]
    V98,

    [Description("新世纪版")]
    NewCentury
}

public sealed class AppSettings : INotifyPropertyChanged
{
    public const double DefaultOpacity = 0.90;
    public const double DefaultScale = 1.0;

    private WubiVersion _version = WubiVersion.V86;
    private double _keyboardOpacity = DefaultOpacity;
    private double _keyboardScale = DefaultScale;
    private bool _showSimpleCodes = true;
    private bool _showRootHints = true;
    private bool _passiveHighlight = true;
    private bool _showKeyboardOnStartup = true;
    private bool _autoCheckUpdates = true;
    private bool _launchAtStartup;
    private bool _hasCompletedFirstRun;
    private double _keyboardLeft = -1;
    private double _keyboardTop = -1;

    public event PropertyChangedEventHandler? PropertyChanged;

    public WubiVersion Version
    {
        get => _version;
        set => Set(ref _version, value);
    }

    public double KeyboardOpacity
    {
        get => _keyboardOpacity;
        set => Set(ref _keyboardOpacity, Math.Clamp(value, 0.25, 1.0));
    }

    public double KeyboardScale
    {
        get => _keyboardScale;
        set => Set(ref _keyboardScale, Math.Clamp(value, 0.60, 1.45));
    }

    public bool ShowSimpleCodes
    {
        get => _showSimpleCodes;
        set => Set(ref _showSimpleCodes, value);
    }

    public bool ShowRootHints
    {
        get => _showRootHints;
        set => Set(ref _showRootHints, value);
    }

    public bool PassiveHighlight
    {
        get => _passiveHighlight;
        set => Set(ref _passiveHighlight, value);
    }

    public bool ShowKeyboardOnStartup
    {
        get => _showKeyboardOnStartup;
        set => Set(ref _showKeyboardOnStartup, value);
    }

    public bool AutoCheckUpdates
    {
        get => _autoCheckUpdates;
        set => Set(ref _autoCheckUpdates, value);
    }

    public bool LaunchAtStartup
    {
        get => _launchAtStartup;
        set => Set(ref _launchAtStartup, value);
    }

    public bool HasCompletedFirstRun
    {
        get => _hasCompletedFirstRun;
        set => Set(ref _hasCompletedFirstRun, value);
    }

    public double KeyboardLeft
    {
        get => _keyboardLeft;
        set => Set(ref _keyboardLeft, value);
    }

    public double KeyboardTop
    {
        get => _keyboardTop;
        set => Set(ref _keyboardTop, value);
    }

    public void Normalize()
    {
        KeyboardOpacity = Math.Clamp(KeyboardOpacity, 0.25, 1.0);
        KeyboardScale = Math.Clamp(KeyboardScale, 0.60, 1.45);
    }

    public void ResetKeyboardAppearance()
    {
        KeyboardOpacity = DefaultOpacity;
        KeyboardScale = DefaultScale;
        KeyboardLeft = -1;
        KeyboardTop = -1;
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

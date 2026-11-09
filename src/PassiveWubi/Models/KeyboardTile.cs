using System.ComponentModel;
using System.Windows.Media;

namespace PassiveWubi.Models;

public sealed class WubiKeyDefinition
{
    public required string Key { get; init; }
    public string SimpleCode { get; init; } = string.Empty;
    public string RootMnemonic { get; init; } = string.Empty;
    public required string AccentColor { get; init; }
    public string Zone { get; init; } = string.Empty;
}

public sealed class KeyboardTile : INotifyPropertyChanged
{
    private bool _isHighlighted;
    private bool _showSimpleCode = true;
    private bool _showRootHint = true;

    public KeyboardTile(WubiKeyDefinition definition)
    {
        Key = definition.Key;
        SimpleCode = definition.SimpleCode;
        RootMnemonic = definition.RootMnemonic;
        AccentColor = definition.AccentColor;
        AccentBrush = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(definition.AccentColor));
        AccentBrush.Freeze();
        Zone = definition.Zone;
    }

    public string Key { get; }
    public string SimpleCode { get; }
    public string RootMnemonic { get; }
    public string AccentColor { get; }
    public System.Windows.Media.Brush AccentBrush { get; }
    public string Zone { get; }

    public string AccessibleText =>
        $"{Key} 键，{(string.IsNullOrWhiteSpace(SimpleCode) ? "无一级简码" : $"一级简码 {SimpleCode}")}，{RootMnemonic.Replace("\n", "，")}";

    public bool IsHighlighted
    {
        get => _isHighlighted;
        set
        {
            if (_isHighlighted == value)
            {
                return;
            }

            _isHighlighted = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHighlighted)));
        }
    }

    public bool ShowSimpleCode
    {
        get => _showSimpleCode;
        set
        {
            if (_showSimpleCode == value)
            {
                return;
            }

            _showSimpleCode = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowSimpleCode)));
        }
    }

    public bool ShowRootHint
    {
        get => _showRootHint;
        set
        {
            if (_showRootHint == value)
            {
                return;
            }

            _showRootHint = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowRootHint)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

using System.ComponentModel;
using System.Windows.Media;

namespace PassiveWubi.Models;

public sealed class KeyboardTile : INotifyPropertyChanged
{
    private bool _isHighlighted;
    private bool _showSimpleCode = true;
    private bool _showRootHint = true;

    public KeyboardTile(WubiKeyDefinition definition)
    {
        Key = definition.Key;
        KeyName = definition.KeyName;
        SimpleCode = definition.SimpleCode;
        Roots = definition.Roots;
        AccentColor = definition.AccentColor;
        AccentBrush = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(definition.AccentColor));
        AccentBrush.Freeze();
        Zone = definition.Zone;
    }

    public string Key { get; }
    public string KeyName { get; }
    public string SimpleCode { get; }
    public IReadOnlyList<string> Roots { get; }
    public string RootTableText => string.Join(" ", Roots);
    public string AccentColor { get; }
    public System.Windows.Media.Brush AccentBrush { get; }
    public string Zone { get; }

    public string AccessibleText =>
        $"{Key} 键，键名 {KeyName}，{(string.IsNullOrWhiteSpace(SimpleCode) ? "无一级简码" : $"一级简码 {SimpleCode}")}，字根 {string.Join("、", Roots)}";

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

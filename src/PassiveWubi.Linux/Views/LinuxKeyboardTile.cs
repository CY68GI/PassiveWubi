using System.ComponentModel;
using Avalonia.Media;
using PassiveWubi.Core;

namespace PassiveWubi.Linux.Views;

public sealed class LinuxKeyboardTile : INotifyPropertyChanged
{
    private static readonly IBrush DefaultBackground = Brush.Parse("#172233");
    private static readonly IBrush HighlightBackground = Brush.Parse("#28374D");
    private static readonly IBrush DefaultBorder = Brush.Parse("#30FFFFFF");
    private static readonly IBrush HighlightBorder = Brush.Parse("#D7DFEC");
    private bool _isHighlighted;
    private bool _showSimpleCode = true;
    private bool _showRootTable = true;

    public LinuxKeyboardTile(WubiKeyDefinition definition)
    {
        Key = definition.Key;
        KeyName = definition.KeyName;
        SimpleCode = definition.SimpleCode;
        Roots = definition.Roots;
        Zone = definition.Zone;
        AccentBrush = Brush.Parse(definition.AccentColor);
    }

    public string Key { get; }
    public string KeyName { get; }
    public string SimpleCode { get; }
    public IReadOnlyList<string> Roots { get; }
    public string RootTableText => string.Join(" ", Roots);
    public string Zone { get; }
    public IBrush AccentBrush { get; }
    public IBrush TileBackground => IsHighlighted ? HighlightBackground : DefaultBackground;
    public IBrush TileBorderBrush => IsHighlighted ? AccentBrush : DefaultBorder;

    public string AccessibleText =>
        $"{Key} 键，键名 {KeyName}，一级简码 {SimpleCode}，字根 {string.Join("、", Roots)}";

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
            OnPropertyChanged(nameof(IsHighlighted));
            OnPropertyChanged(nameof(TileBackground));
            OnPropertyChanged(nameof(TileBorderBrush));
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
            OnPropertyChanged(nameof(ShowSimpleCode));
        }
    }

    public bool ShowRootTable
    {
        get => _showRootTable;
        set
        {
            if (_showRootTable == value)
            {
                return;
            }

            _showRootTable = value;
            OnPropertyChanged(nameof(ShowRootTable));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

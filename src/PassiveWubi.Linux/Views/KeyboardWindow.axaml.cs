using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PassiveWubi.Core;

namespace PassiveWubi.Linux.Views;

public partial class KeyboardWindow : Window
{
    private const double BaseWidth = 1240;
    private const double BaseHeight = 520;
    private readonly ObservableCollection<LinuxKeyboardTile> _top = [];
    private readonly ObservableCollection<LinuxKeyboardTile> _home = [];
    private readonly ObservableCollection<LinuxKeyboardTile> _bottom = [];
    private readonly List<LinuxKeyboardTile> _allTiles = [];
    private readonly DispatcherTimer _highlightTimer;
    private WubiVersion? _renderedVersion;
    private int _highlightIndex = -1;
    private bool _allowClose;
    private bool _positioned;

    public KeyboardWindow()
    {
        InitializeComponent();
        TopRowItems.ItemsSource = _top;
        HomeRowItems.ItemsSource = _home;
        BottomRowItems.ItemsSource = _bottom;
        _highlightTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _highlightTimer.Tick += (_, _) => AdvanceHighlight();
        Closing += OnClosing;
        PositionChanged += (_, _) =>
        {
            if (_positioned)
            {
                Host.Settings.KeyboardLeft = Position.X;
                Host.Settings.KeyboardTop = Position.Y;
            }
        };
    }

    private App Host => (App)Application.Current!;

    public void AllowClose() => _allowClose = true;

    public void ApplySettings()
    {
        var settings = Host.Settings;
        Opacity = settings.KeyboardOpacity;
        Width = BaseWidth * settings.KeyboardScale;
        Height = BaseHeight * settings.KeyboardScale;
        VersionChipText.Text = WubiDefinitions.GetVersionName(settings.Version);
        LearningModeText.Text = settings.PassiveHighlight ? "潜移默化 · 每 5 秒" : "静态字根";

        if (_renderedVersion != settings.Version)
        {
            Render(settings.Version);
        }

        foreach (var tile in _allTiles)
        {
            tile.ShowSimpleCode = settings.ShowSimpleCodes;
            tile.ShowRootTable = settings.ShowRootTable;
        }

        UpdateTimer();
        if (!_positioned)
        {
            PositionInitialWindow();
        }
    }

    private void Render(WubiVersion version)
    {
        var definitions = WubiDefinitions.GetLayout(version)
            .ToDictionary(item => item.Key, StringComparer.OrdinalIgnoreCase);
        Fill(_top, definitions, "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P");
        Fill(_home, definitions, "A", "S", "D", "F", "G", "H", "J", "K", "L");
        Fill(_bottom, definitions, "Z", "X", "C", "V", "B", "N", "M");
        _allTiles.Clear();
        _allTiles.AddRange(_top);
        _allTiles.AddRange(_home);
        _allTiles.AddRange(_bottom);
        _highlightIndex = -1;
        _renderedVersion = version;
    }

    private static void Fill(
        ObservableCollection<LinuxKeyboardTile> target,
        IReadOnlyDictionary<string, WubiKeyDefinition> definitions,
        params string[] keys)
    {
        target.Clear();
        foreach (var key in keys)
        {
            if (definitions.TryGetValue(key, out var definition))
            {
                target.Add(new LinuxKeyboardTile(definition));
            }
        }
    }

    private void PositionInitialWindow()
    {
        var screen = Screens.Primary ?? Screens.All.FirstOrDefault();
        if (screen is null)
        {
            return;
        }

        var area = screen.WorkingArea;
        var x = Host.Settings.KeyboardLeft >= 0
            ? (int)Host.Settings.KeyboardLeft
            : area.X + Math.Max(0, (area.Width - (int)Width) / 2);
        var y = Host.Settings.KeyboardTop >= 0
            ? (int)Host.Settings.KeyboardTop
            : area.Bottom - (int)Height - 30;
        Position = new PixelPoint(x, y);
        _positioned = true;
    }

    private void Header_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
            Host.SaveSettings();
        }
    }

    private void OpenSettings_Click(object? sender, RoutedEventArgs e)
    {
        Host.ShowMainWindow();
    }

    private void HideKeyboard_Click(object? sender, RoutedEventArgs e)
    {
        Host.HideKeyboard();
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose || Host.IsExiting)
        {
            _highlightTimer.Stop();
            return;
        }

        e.Cancel = true;
        Hide();
        Host.SaveSettings();
        Host.RestoreKeyboardTopmost();
    }

    private void UpdateTimer()
    {
        if (IsVisible && Host.Settings.PassiveHighlight)
        {
            _highlightTimer.Start();
        }
        else
        {
            _highlightTimer.Stop();
            ClearHighlight();
        }
    }

    private void AdvanceHighlight()
    {
        if (_allTiles.Count == 0)
        {
            return;
        }

        ClearHighlight();
        _highlightIndex = (_highlightIndex + 1) % _allTiles.Count;
        _allTiles[_highlightIndex].IsHighlighted = true;
    }

    private void ClearHighlight()
    {
        if (_highlightIndex >= 0 && _highlightIndex < _allTiles.Count)
        {
            _allTiles[_highlightIndex].IsHighlighted = false;
        }
    }
}

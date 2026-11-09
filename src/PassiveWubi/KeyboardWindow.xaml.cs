using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using PassiveWubi.Data;
using PassiveWubi.Models;

namespace PassiveWubi;

public partial class KeyboardWindow : Window
{
    private const double BaseWidth = 1240;
    private const double BaseHeight = 520;
    private const double MinimumWidth = BaseWidth * 0.60;
    private const double MaximumWidth = BaseWidth * 1.45;

    private readonly ObservableCollection<KeyboardTile> _topRow = [];
    private readonly ObservableCollection<KeyboardTile> _homeRow = [];
    private readonly ObservableCollection<KeyboardTile> _bottomRow = [];
    private readonly DispatcherTimer _highlightTimer;
    private readonly List<KeyboardTile> _allTiles = [];
    private WubiVersion? _renderedVersion;
    private int _highlightIndex = -1;
    private bool _isPositioning;
    private bool _hasBeenPositioned;

    public KeyboardWindow()
    {
        InitializeComponent();

        TopRowItems.ItemsSource = _topRow;
        HomeRowItems.ItemsSource = _homeRow;
        BottomRowItems.ItemsSource = _bottomRow;

        _highlightTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _highlightTimer.Tick += HighlightTimer_Tick;
        IsVisibleChanged += Window_IsVisibleChanged;
    }

    private App Host => (App)Application.Current;

    public void ApplySettings()
    {
        var settings = Host.Settings;
        Opacity = settings.KeyboardOpacity;
        Topmost = true;

        Width = BaseWidth * settings.KeyboardScale;
        Height = BaseHeight * settings.KeyboardScale;

        if (_renderedVersion != settings.Version)
        {
            RenderLayout(settings.Version);
        }

        foreach (var tile in _allTiles)
        {
            tile.ShowSimpleCode = settings.ShowSimpleCodes;
            tile.ShowRootHint = settings.ShowRootHints;
        }

        LearningModeText.Text = settings.PassiveHighlight
            ? "潜移默化模式 · 每 5 秒轻提示"
            : "静态字根模式";

        UpdateHighlightTimer();

        if (!_hasBeenPositioned)
        {
            PositionInitialWindow();
        }
        else
        {
            ClampToWorkArea();
        }
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        if (!_hasBeenPositioned)
        {
            PositionInitialWindow();
        }

        UpdateHighlightTimer();
    }

    private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UpdateHighlightTimer();
    }

    private void RenderLayout(WubiVersion version)
    {
        var definitions = WubiDefinitions.GetLayout(version)
            .ToDictionary(definition => definition.Key, StringComparer.OrdinalIgnoreCase);

        PopulateRow(_topRow, definitions, "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P");
        PopulateRow(_homeRow, definitions, "A", "S", "D", "F", "G", "H", "J", "K", "L");
        PopulateRow(_bottomRow, definitions, "Z", "X", "C", "V", "B", "N", "M");

        _allTiles.Clear();
        _allTiles.AddRange(_topRow);
        _allTiles.AddRange(_homeRow);
        _allTiles.AddRange(_bottomRow);
        _highlightIndex = -1;
        VersionChipText.Text = WubiDefinitions.GetVersionName(version);
        _renderedVersion = version;
    }

    private static void PopulateRow(
        ObservableCollection<KeyboardTile> target,
        IReadOnlyDictionary<string, WubiKeyDefinition> definitions,
        params string[] keys)
    {
        target.Clear();
        foreach (var key in keys)
        {
            if (definitions.TryGetValue(key, out var definition))
            {
                target.Add(new KeyboardTile(definition));
            }
        }
    }

    private void UpdateHighlightTimer()
    {
        var shouldRun = IsVisible && Host.Settings.PassiveHighlight;
        if (shouldRun && !_highlightTimer.IsEnabled)
        {
            _highlightTimer.Start();
        }
        else if (!shouldRun && _highlightTimer.IsEnabled)
        {
            _highlightTimer.Stop();
            ClearHighlight();
        }
    }

    private void HighlightTimer_Tick(object? sender, EventArgs e)
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

    private void PositionInitialWindow()
    {
        _isPositioning = true;
        try
        {
            var workArea = SystemParameters.WorkArea;
            var left = Host.Settings.KeyboardLeft;
            var top = Host.Settings.KeyboardTop;

            if (left < 0 || top < 0)
            {
                left = workArea.Left + Math.Max(0, (workArea.Width - Width) / 2);
                top = workArea.Bottom - Height - 26;
            }

            Left = left;
            Top = top;
            ClampToWorkAreaCore(workArea);
            _hasBeenPositioned = true;
            PersistPosition();
        }
        finally
        {
            _isPositioning = false;
        }
    }

    private void ClampToWorkArea()
    {
        _isPositioning = true;
        try
        {
            ClampToWorkAreaCore(SystemParameters.WorkArea);
            PersistPosition();
        }
        finally
        {
            _isPositioning = false;
        }
    }

    private void ClampToWorkAreaCore(Rect workArea)
    {
        var maxLeft = workArea.Right - Math.Min(Width, workArea.Width);
        var maxTop = workArea.Bottom - Math.Min(Height, workArea.Height);
        Left = Math.Clamp(Left, workArea.Left, Math.Max(workArea.Left, maxLeft));
        Top = Math.Clamp(Top, workArea.Top, Math.Max(workArea.Top, maxTop));
    }

    private void PersistPosition()
    {
        if (_isPositioning || !_hasBeenPositioned)
        {
            return;
        }

        Host.Settings.KeyboardLeft = Left;
        Host.Settings.KeyboardTop = Top;
    }

    private void DragHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
            return;
        }

        PersistPosition();
        Host.SaveSettings();
    }

    private void ResizeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
    {
        var width = Math.Clamp(Width + e.HorizontalChange, MinimumWidth, MaximumWidth);
        Width = width;
        Height = width * (BaseHeight / BaseWidth);
        ClampToWorkAreaCore(SystemParameters.WorkArea);
        Host.Settings.KeyboardScale = width / BaseWidth;
    }

    private void ResizeThumb_DragCompleted(
        object sender,
        System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
        PersistPosition();
        Host.SaveSettings();
    }

    private void Window_LocationChanged(object? sender, EventArgs e)
    {
        if (IsLoaded && !_isPositioning)
        {
            PersistPosition();
        }
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (Host.IsExiting)
        {
            return;
        }

        e.Cancel = true;
        Hide();
        Host.SaveSettings();
        Host.RefreshKeyboardStatus();
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        Host.ShowMainWindow();
    }

    private void HideKeyboard_Click(object sender, RoutedEventArgs e)
    {
        Host.HideKeyboard();
    }
}

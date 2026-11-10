using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PassiveWubi.Core;
using PassiveWubi.Linux.Models;
using PassiveWubi.Linux.Services;

namespace PassiveWubi.Linux.Views;

public partial class MainWindowView : Window
{
    private bool _loading;
    private bool _allowClose;
    private string _releaseUrl = AppInfo.ReleasesUrl;

    public MainWindowView()
    {
        InitializeComponent();
        Closing += OnClosing;
        Opened += (_, _) => RefreshFromSettings();
    }

    private App Host => (App)Application.Current!;

    public void AllowClose() => _allowClose = true;

    public void RefreshFromSettings()
    {
        _loading = true;
        try
        {
            var settings = Host.Settings;
            VersionCombo.SelectedIndex = settings.Version switch
            {
                WubiVersion.V98 => 1,
                WubiVersion.NewCentury => 2,
                _ => 0
            };
            OpacitySlider.Value = settings.KeyboardOpacity;
            ScaleSlider.Value = settings.KeyboardScale;
            SimpleCodesCheck.IsChecked = settings.ShowSimpleCodes;
            RootTableCheck.IsChecked = settings.ShowRootTable;
            HighlightCheck.IsChecked = settings.PassiveHighlight;
            ShowOnStartupCheck.IsChecked = settings.ShowKeyboardOnStartup;
            AutoUpdateCheck.IsChecked = settings.AutoCheckUpdates;
            AutostartCheck.IsChecked = Host.AutostartService.IsEnabled();
            HeroVersionText.Text = WubiDefinitions.GetVersionName(settings.Version);
            AboutVersionText.Text = AppInfo.DisplayVersion;
            ThemeButton.Content = settings.Theme == LinuxTheme.Dark ? "切换为浅色" : "切换为深色";
            LogPathText.Text = LinuxAppPaths.LogDirectory;
            OpacityValueText.Text = $"{settings.KeyboardOpacity:P0}";
            ScaleValueText.Text = $"{settings.KeyboardScale:P0}";
        }
        finally
        {
            _loading = false;
        }
    }

    private void Version_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (_loading || VersionCombo.SelectedItem is not ComboBoxItem item)
        {
            return;
        }

        if (Enum.TryParse<WubiVersion>(item.Tag?.ToString(), out var version))
        {
            Host.Settings.Version = version;
            HeroVersionText.Text = WubiDefinitions.GetVersionName(version);
            ApplyKeyboardSettings();
        }
    }

    private void MainTabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (MainTabs?.SelectedIndex == 1)
        {
            PracticeViewControl?.RefreshForVersion();
        }
    }

    private void Opacity_Changed(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_loading || OpacityValueText is null)
        {
            return;
        }

        Host.Settings.KeyboardOpacity = e.NewValue;
        OpacityValueText.Text = $"{e.NewValue:P0}";
        ApplyKeyboardSettings();
    }

    private void Scale_Changed(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_loading || ScaleValueText is null)
        {
            return;
        }

        Host.Settings.KeyboardScale = e.NewValue;
        ScaleValueText.Text = $"{e.NewValue:P0}";
        ApplyKeyboardSettings();
    }

    private void Option_Changed(object? sender, RoutedEventArgs e)
    {
        if (_loading)
        {
            return;
        }

        Host.Settings.ShowSimpleCodes = SimpleCodesCheck.IsChecked == true;
        Host.Settings.ShowRootTable = RootTableCheck.IsChecked == true;
        Host.Settings.PassiveHighlight = HighlightCheck.IsChecked == true;
        Host.Settings.ShowKeyboardOnStartup = ShowOnStartupCheck.IsChecked == true;
        Host.Settings.AutoCheckUpdates = AutoUpdateCheck.IsChecked == true;
        ApplyKeyboardSettings();
    }

    private void Autostart_Changed(object? sender, RoutedEventArgs e)
    {
        if (_loading)
        {
            return;
        }

        Host.Settings.LaunchAtStartup = AutostartCheck.IsChecked == true;
        Host.AutostartService.SetEnabled(Host.Settings.LaunchAtStartup);
        Host.SaveSettings();
    }

    private void Theme_Click(object? sender, RoutedEventArgs e)
    {
        Host.Settings.Theme = Host.Settings.Theme == LinuxTheme.Dark
            ? LinuxTheme.Light
            : LinuxTheme.Dark;
        Host.ApplyTheme();
        RefreshFromSettings();
    }

    private void ShowKeyboard_Click(object? sender, RoutedEventArgs e) => Host.ShowKeyboard();

    private void HideKeyboard_Click(object? sender, RoutedEventArgs e) => Host.HideKeyboard();

    private void OpenTodayLog_Click(object? sender, RoutedEventArgs e) => Host.Log.OpenTodayLog();

    private void OpenLogFolder_Click(object? sender, RoutedEventArgs e) => Host.Log.OpenLogFolder();

    private void OpenRepository_Click(object? sender, RoutedEventArgs e) => Host.OpenUrl(AppInfo.RepositoryUrl);

    private void OpenReleasePage_Click(object? sender, RoutedEventArgs e) => Host.OpenUrl(_releaseUrl);

    private async void CheckUpdate_Click(object? sender, RoutedEventArgs e)
    {
        CheckUpdateButton.IsEnabled = false;
        UpdateStatusText.Text = "正在连接 GitHub…";
        try
        {
            var result = await Host.CheckForUpdatesAsync();
            _releaseUrl = result.ReleaseUrl;
            ReleaseButton.IsVisible = true;
            UpdateStatusText.Text = result.Success
                ? result.HasUpdate
                    ? $"发现新版本 {result.LatestVersion}，当前版本 {result.CurrentVersion}。"
                    : $"当前已是最新版本 {AppInfo.DisplayVersion}。"
                : result.Error ?? "更新检查失败。";
        }
        finally
        {
            CheckUpdateButton.IsEnabled = true;
        }
    }

    private void Exit_Click(object? sender, RoutedEventArgs e) => Host.ExitApplication();

    private void ApplyKeyboardSettings()
    {
        Host.SaveSettings();
        Host.KeyboardView?.ApplySettings();
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose || Host.IsExiting)
        {
            return;
        }

        e.Cancel = true;
        Host.SaveSettings();
        Hide();
        Host.RestoreKeyboardTopmost();
    }
}

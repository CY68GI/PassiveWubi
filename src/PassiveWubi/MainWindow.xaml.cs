using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PassiveWubi.Models;
using PassiveWubi.Services;

namespace PassiveWubi;

public partial class MainWindow : Window
{
    private bool _isRefreshing;
    private string _latestReleaseUrl = AppInfo.ReleasesUrl;
    private readonly DispatcherTimer _saveDebounce = new()
    {
        Interval = TimeSpan.FromMilliseconds(450)
    };

    public MainWindow()
    {
        InitializeComponent();
        _saveDebounce.Tick += (_, _) =>
        {
            _saveDebounce.Stop();
            Host.SaveSettings();
        };
    }

    private App Host => (App)Application.Current;

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ThemeService.ApplyWindowChrome(this);
        Dispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle,
            new Action(() => ThemeService.ApplyWindowChrome(this)));
        SidebarVersionText.Text = AppInfo.DisplayVersion;
        AboutVersionText.Text = AppInfo.DisplayVersion;
        LogPathText.Text = AppPaths.LogDirectory;
        UpdateStatusText.Text = $"当前版本 {AppInfo.DisplayVersion}";
        RefreshFromSettings();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (Host.IsExiting)
        {
            return;
        }

        e.Cancel = true;
        _saveDebounce.Stop();
        Host.SaveSettings();
        Hide();
        Topmost = false;
        Host.RestoreKeyboardTopmost();
        Host.Log.Info("Main window hidden; application remains in the tray.");
    }

    public void RefreshFromSettings()
    {
        _isRefreshing = true;
        try
        {
            var settings = Host.Settings;
            Version86Radio.IsChecked = settings.Version == WubiVersion.V86;
            Version98Radio.IsChecked = settings.Version == WubiVersion.V98;
            NewCenturyRadio.IsChecked = settings.Version == WubiVersion.NewCentury;
            OpacitySlider.Value = settings.KeyboardOpacity;
            ScaleSlider.Value = settings.KeyboardScale;
            ShowSimpleCodesToggle.IsChecked = settings.ShowSimpleCodes;
            ShowRootHintsToggle.IsChecked = settings.ShowRootHints;
            PassiveHighlightToggle.IsChecked = settings.PassiveHighlight;
            ShowKeyboardOnStartupToggle.IsChecked = settings.ShowKeyboardOnStartup;
            AutoUpdateToggle.IsChecked = settings.AutoCheckUpdates;
            StartupToggle.IsChecked = Host.StartupService.IsEnabled();
            HeroVersionText.Text = WubiDefinitions.GetVersionName(settings.Version);
            OpacityValueText.Text = $"{settings.KeyboardOpacity:P0}";
            ScaleValueText.Text = $"{settings.KeyboardScale:P0}";
            ThemeToggleButton.Content = settings.Theme == AppTheme.Dark
                ? "切换为浅色"
                : "切换为深色";
            OverlayStatusText.Text = Host.KeyboardWindowInstance?.IsVisible == true
                ? "置顶键盘运行中"
                : "置顶键盘已隐藏";
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    public void ShowUpdateAvailable(UpdateCheckResult result)
    {
        NavAbout.IsChecked = true;
        _latestReleaseUrl = result.ReleaseUrl;
        UpdateStatusText.Text = $"发现新版本 {result.LatestVersion}，当前版本 {result.CurrentVersion}。";
        ReleasePageButton.Visibility = Visibility.Visible;
        MessageBox.Show(
            this,
            $"发现新版本 {result.LatestVersion}。\n\n可以点击“打开发布页”下载最新版。",
            "发现更新",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public void SetKeyboardStatus(bool visible)
    {
        OverlayStatusText.Text = visible ? "置顶键盘运行中" : "置顶键盘已隐藏";
    }

    private void Navigation_Checked(object sender, RoutedEventArgs e)
    {
        if (SettingsPage is null)
        {
            return;
        }

        var selected = (sender as RadioButton)?.Tag?.ToString();
        SettingsPage.Visibility = selected == "settings" ? Visibility.Visible : Visibility.Collapsed;
        AboutPage.Visibility = selected == "about" ? Visibility.Visible : Visibility.Collapsed;
        ChangelogPage.Visibility = selected == "changelog" ? Visibility.Visible : Visibility.Collapsed;

        switch (selected)
        {
            case "about":
                PageTitleText.Text = "关于";
                PageSubtitleText.Text = "版本信息、运行日志、开源地址与更新检查。";
                break;
            case "changelog":
                PageTitleText.Text = "更新日志";
                PageSubtitleText.Text = "查看每个版本带来的变化。";
                break;
            default:
                PageTitleText.Text = "键盘与学习";
                PageSubtitleText.Text = "选择你的五笔版本，让键位在视线边缘慢慢变成肌肉记忆。";
                break;
        }
    }

    private void Version_Checked(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing || sender is not RadioButton radio)
        {
            return;
        }

        if (!Enum.TryParse<WubiVersion>(radio.Tag?.ToString(), out var version))
        {
            return;
        }

        Host.Settings.Version = version;
        HeroVersionText.Text = WubiDefinitions.GetVersionName(version);
        Host.ApplySettings();
        Host.Log.Info($"Wubi version changed to {version}.");
    }

    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (OpacityValueText is null)
        {
            return;
        }

        OpacityValueText.Text = $"{e.NewValue:P0}";
        if (_isRefreshing)
        {
            return;
        }

        Host.Settings.KeyboardOpacity = e.NewValue;
        Host.KeyboardWindowInstance?.ApplySettings();
        ScheduleSave();
    }

    private void OpacitySlider_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        Host.SaveSettings();
    }

    private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ScaleValueText is null)
        {
            return;
        }

        ScaleValueText.Text = $"{e.NewValue:P0}";
        if (_isRefreshing)
        {
            return;
        }

        Host.Settings.KeyboardScale = e.NewValue;
        Host.KeyboardWindowInstance?.ApplySettings();
        ScheduleSave();
    }

    private void ScaleSlider_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        Host.SaveSettings();
    }

    private void OptionToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        Host.Settings.ShowSimpleCodes = ShowSimpleCodesToggle.IsChecked == true;
        Host.Settings.ShowRootHints = ShowRootHintsToggle.IsChecked == true;
        Host.Settings.PassiveHighlight = PassiveHighlightToggle.IsChecked == true;
        Host.Settings.ShowKeyboardOnStartup = ShowKeyboardOnStartupToggle.IsChecked == true;
        Host.Settings.AutoCheckUpdates = AutoUpdateToggle.IsChecked == true;
        Host.ApplySettings();
    }

    private void StartupToggle_Click(object sender, RoutedEventArgs e)
    {
        if (_isRefreshing)
        {
            return;
        }

        Host.Settings.LaunchAtStartup = StartupToggle.IsChecked == true;
        Host.ApplyStartupSetting();
        Host.ApplySettings();
        StartupToggle.IsChecked = Host.Settings.LaunchAtStartup;
    }

    private void ShowKeyboard_Click(object sender, RoutedEventArgs e)
    {
        Host.ShowKeyboard();
    }

    private void HideKeyboard_Click(object sender, RoutedEventArgs e)
    {
        Host.HideKeyboard();
    }

    private void HideMainWindow_Click(object sender, RoutedEventArgs e)
    {
        Host.SaveSettings();
        Hide();
        Topmost = false;
        Host.RestoreKeyboardTopmost();
        Host.Log.Info("Main window hidden by header button.");
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (MaximizeButton is not null)
        {
            MaximizeButton.Content = WindowState == WindowState.Maximized ? "❐" : "□";
            MaximizeButton.ToolTip = WindowState == WindowState.Maximized ? "还原" : "最大化";
        }
    }

    private void ToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        Host.Settings.Theme = Host.Settings.Theme == AppTheme.Dark
            ? AppTheme.Light
            : AppTheme.Dark;
        Host.ApplyTheme();
        RefreshFromSettings();
        Dispatcher.BeginInvoke(
            DispatcherPriority.ApplicationIdle,
            new Action(() => ThemeService.ApplyWindowChrome(this)));
    }

    private void ResetAppearance_Click(object sender, RoutedEventArgs e)
    {
        Host.Settings.ResetKeyboardAppearance();
        Host.ApplySettings();
        Host.ShowKeyboard();
        RefreshFromSettings();
    }

    private void OpenLogFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Host.Log.Info("Opening log folder from UI.");
            Host.Log.OpenLogFolder();
        }
        catch (Exception ex)
        {
            Host.Log.Error("Unable to open log folder.", ex);
            MessageBox.Show(this, "无法打开日志目录。", "打开失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OpenTodayLog_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Host.Log.OpenTodayLog();
        }
        catch (Exception ex)
        {
            Host.Log.Error("Unable to open today's log.", ex);
            MessageBox.Show(this, "无法打开今日日志。", "打开失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ScheduleSave()
    {
        _saveDebounce.Stop();
        _saveDebounce.Start();
    }

    private void OpenRepository_Click(object sender, RoutedEventArgs e) => App.OpenUrl(AppInfo.RepositoryUrl);

    private void OpenIssues_Click(object sender, RoutedEventArgs e) => App.OpenUrl(AppInfo.IssuesUrl);

    private void OpenReleasePage_Click(object sender, RoutedEventArgs e) => App.OpenUrl(_latestReleaseUrl);

    private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        CheckUpdateButton.IsEnabled = false;
        CheckUpdateButton.Content = "正在检查…";
        UpdateStatusText.Text = "正在连接 GitHub…";
        try
        {
            var result = await Host.CheckForUpdatesAsync(false);
            if (result is null)
            {
                return;
            }

            _latestReleaseUrl = result.ReleaseUrl;
            if (!result.Success)
            {
                UpdateStatusText.Text = result.Error ?? "更新检查失败。";
                return;
            }

            if (result.HasUpdate)
            {
                UpdateStatusText.Text = $"发现新版本 {result.LatestVersion}，当前版本 {result.CurrentVersion}。";
                ReleasePageButton.Visibility = Visibility.Visible;
            }
            else
            {
                UpdateStatusText.Text = $"当前已是最新版本 {AppInfo.DisplayVersion}。";
                ReleasePageButton.Visibility = Visibility.Visible;
            }
        }
        finally
        {
            CheckUpdateButton.IsEnabled = true;
            CheckUpdateButton.Content = "检查更新";
        }
    }
}

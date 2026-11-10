using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using PassiveWubi.Linux.Models;
using PassiveWubi.Linux.Services;
using PassiveWubi.Linux.Views;

namespace PassiveWubi.Linux;

public sealed partial class App : Application
{
    private Mutex? _singleInstanceMutex;
    private TrayIcon? _trayIcon;
    private bool _isExiting;
    private readonly CancellationTokenSource _lifetime = new();

    public LinuxAppSettings Settings { get; private set; } = new();
    public LogService Log { get; private set; } = null!;
    public SettingsService SettingsService { get; private set; } = null!;
    public AutostartService AutostartService { get; private set; } = null!;
    public UpdateService UpdateService { get; private set; } = null!;
    public MainWindowView? MainView { get; private set; }
    public KeyboardWindow? KeyboardView { get; private set; }
    public bool IsExiting => _isExiting;
    public LinuxUpdateResult? LastUpdateResult { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        _singleInstanceMutex = new Mutex(true, "PassiveWubi.Linux.SingleInstance", out var isFirst);
        Log = new LogService();
        Log.Info($"PassiveWubi Linux {AppInfo.DisplayVersion} starting.");

        if (!isFirst)
        {
            desktop.Shutdown();
            base.OnFrameworkInitializationCompleted();
            return;
        }

        SettingsService = new SettingsService(Log);
        AutostartService = new AutostartService(Log);
        UpdateService = new UpdateService(Log);
        Settings = SettingsService.Load();
        Settings.LaunchAtStartup = AutostartService.IsEnabled();
        ApplyTheme();
        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        CreateTrayIcon();

        if (!Settings.HasCompletedFirstRun)
        {
            ShowMainWindow();
            ShowKeyboard();
            Settings.HasCompletedFirstRun = true;
            SaveSettings();
        }
        else if (Settings.ShowKeyboardOnStartup)
        {
            ShowKeyboard();
        }

        if (Settings.AutoCheckUpdates)
        {
            _ = CheckUpdatesSilentlyAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void SaveSettings()
    {
        SettingsService.Save(Settings);
    }

    public void ApplyTheme()
    {
        RequestedThemeVariant = Settings.Theme == LinuxTheme.Dark
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
        SaveSettings();
    }

    public void ShowMainWindow()
    {
        if (_isExiting)
        {
            return;
        }

        if (KeyboardView is not null)
        {
            KeyboardView.Topmost = false;
        }

        MainView ??= new MainWindowView();
        MainView.Show();
        MainView.WindowState = WindowState.Normal;
        MainView.Topmost = false;
        MainView.Activate();
    }

    public void ShowKeyboard()
    {
        if (_isExiting)
        {
            return;
        }

        KeyboardView ??= new KeyboardWindow();
        KeyboardView.ApplySettings();
        var mainVisible = MainView?.IsVisible == true;
        KeyboardView.Show();
        KeyboardView.Topmost = !mainVisible;
        if (mainVisible)
        {
            MainView!.Activate();
        }
        else
        {
            KeyboardView.Activate();
        }
    }

    public void HideKeyboard()
    {
        KeyboardView?.Hide();
    }

    public void ToggleKeyboard()
    {
        if (KeyboardView?.IsVisible == true)
        {
            HideKeyboard();
        }
        else
        {
            ShowKeyboard();
        }
    }

    public void RestoreKeyboardTopmost()
    {
        if (KeyboardView?.IsVisible == true)
        {
            KeyboardView.Topmost = true;
        }
    }

    public async Task<LinuxUpdateResult> CheckForUpdatesAsync()
    {
        LastUpdateResult = await UpdateService.CheckAsync(_lifetime.Token);
        return LastUpdateResult;
    }

    public void OpenUrl(string url)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "xdg-open",
            ArgumentList = { url },
            UseShellExecute = false
        });
    }

    public void ExitApplication()
    {
        if (_isExiting)
        {
            return;
        }

        _isExiting = true;
        SaveSettings();
        _lifetime.Cancel();
        KeyboardView?.AllowClose();
        MainView?.AllowClose();
        KeyboardView?.Close();
        MainView?.Close();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private void CreateTrayIcon()
    {
        try
        {
            var icon = new WindowIcon(AssetLoader.Open(
                new Uri("avares://PassiveWubi.Linux/Assets/PassiveWubi.ico")));
            var menu = new NativeMenu();

            var openItem = new NativeMenuItem("打开肌忆五笔");
            openItem.Click += (_, _) => Dispatcher.UIThread.Post(ShowMainWindow);
            var keyboardItem = new NativeMenuItem("显示/隐藏置顶键盘");
            keyboardItem.Click += (_, _) => Dispatcher.UIThread.Post(ToggleKeyboard);
            var exitItem = new NativeMenuItem("退出肌忆五笔");
            exitItem.Click += (_, _) => Dispatcher.UIThread.Post(ExitApplication);

            menu.Items.Add(openItem);
            menu.Items.Add(keyboardItem);
            menu.Items.Add(new NativeMenuItemSeparator());
            menu.Items.Add(exitItem);

            _trayIcon = new TrayIcon
            {
                Icon = icon,
                ToolTipText = "肌忆五笔 PassiveWubi",
                Menu = menu
            };
            TrayIcon.SetIcons(this, [_trayIcon]);
        }
        catch (Exception ex)
        {
            Log.Error("Linux tray icon is unavailable; use the in-app buttons.", ex);
        }
    }

    private async Task CheckUpdatesSilentlyAsync()
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(4), _lifetime.Token);
            await CheckForUpdatesAsync();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            Log.Error("Automatic Linux update check failed.", ex);
        }
    }
}

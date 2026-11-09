using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;
using PassiveWubi.Models;
using PassiveWubi.Services;
using Forms = System.Windows.Forms;

namespace PassiveWubi;

public partial class App : System.Windows.Application
{
    private const string MutexName = @"Local\PassiveWubi.SingleInstance";
    private const string ActivationEventName = @"Local\PassiveWubi.Activate";

    private readonly CancellationTokenSource _lifetime = new();
    private Mutex? _singleInstanceMutex;
    private EventWaitHandle? _activationEvent;
    private Forms.NotifyIcon? _trayIcon;
    private Icon? _trayIconImage;
    private Task? _activationListener;
    private bool _exiting;
    private bool _autoUpdateAnnounced;

    public AppSettings Settings { get; private set; } = new();
    public LogService Log { get; private set; } = null!;
    public SettingsService SettingsService { get; private set; } = null!;
    public StartupService StartupService { get; private set; } = null!;
    public UpdateService UpdateService { get; private set; } = null!;
    public MainWindow? MainWindowInstance { get; private set; }
    public KeyboardWindow? KeyboardWindowInstance { get; private set; }
    public bool IsExiting => _exiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Log = new LogService();
        Log.Info($"PassiveWubi {AppInfo.DisplayVersion} starting. Base directory: {AppPaths.BaseDirectory}");
        Log.Info($"Command line: {string.Join(" ", e.Args)}");

        if (!AcquireSingleInstance())
        {
            Log.Info("Another PassiveWubi instance is already running; activation signal sent.");
            Shutdown();
            return;
        }

        SettingsService = new SettingsService(Log);
        StartupService = new StartupService(Log);
        UpdateService = new UpdateService(Log);
        Settings = SettingsService.Load();
        Settings.LaunchAtStartup = StartupService.IsEnabled();

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        CreateTrayIcon();

        if (Settings.LaunchAtStartup && !StartupService.IsEnabled())
        {
            StartupService.SetEnabled(true);
        }

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
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Cleanup();
        base.OnExit(e);
    }

    public void SaveSettings()
    {
        SettingsService.Save(Settings);
    }

    public void ApplySettings()
    {
        SaveSettings();
        KeyboardWindowInstance?.ApplySettings();
    }

    public void ShowMainWindow()
    {
        if (_exiting)
        {
            return;
        }

        MainWindowInstance ??= new MainWindow();
        if (!MainWindowInstance.IsVisible)
        {
            MainWindowInstance.Show();
        }

        if (MainWindowInstance.WindowState == WindowState.Minimized)
        {
            MainWindowInstance.WindowState = WindowState.Normal;
        }

        MainWindowInstance.Activate();
        MainWindowInstance.Topmost = true;
        MainWindowInstance.Activate();
        MainWindowInstance.Focus();
        Log.Info("Main window shown.");
    }

    public void ShowKeyboard()
    {
        if (_exiting)
        {
            return;
        }

        KeyboardWindowInstance ??= new KeyboardWindow();
        KeyboardWindowInstance.ApplySettings();
        KeyboardWindowInstance.Show();
        KeyboardWindowInstance.Activate();
        RefreshKeyboardStatus();
        Log.Info("Keyboard overlay shown.");
    }

    public void HideKeyboard()
    {
        KeyboardWindowInstance?.Hide();
        RefreshKeyboardStatus();
        Log.Info("Keyboard overlay hidden.");
    }

    public void ToggleKeyboard()
    {
        if (KeyboardWindowInstance?.IsVisible == true)
        {
            HideKeyboard();
        }
        else
        {
            ShowKeyboard();
        }

        UpdateTrayMenuText();
    }

    public void SetKeyboardOpacity(double opacity)
    {
        Settings.KeyboardOpacity = Math.Clamp(opacity, 0.25, 1.0);
        ApplySettings();
    }

    public void SetKeyboardVisibleFromSettings(bool visible)
    {
        if (visible)
        {
            ShowKeyboard();
        }
        else
        {
            HideKeyboard();
        }

        UpdateTrayMenuText();
    }

    public void ApplyStartupSetting()
    {
        if (StartupService.SetEnabled(Settings.LaunchAtStartup))
        {
            Settings.LaunchAtStartup = StartupService.IsEnabled();
        }
    }

    public async Task<UpdateCheckResult?> CheckForUpdatesAsync(bool interactive)
    {
        try
        {
            var result = await UpdateService.CheckAsync(_lifetime.Token);
            if (interactive && result.HasUpdate)
            {
                ShowMainWindow();
                MainWindowInstance?.ShowUpdateAvailable(result);
            }

            return result;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            return null;
        }
        catch (Exception ex)
        {
            Log.Error("Unexpected update check error.", ex);
            if (interactive)
            {
                MessageBox.Show(
                    MainWindowInstance ?? (Window?)KeyboardWindowInstance,
                    "更新检查失败，请稍后重试。",
                    "检查更新",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            return UpdateCheckResult.Failed(AppInfo.Version, "更新检查失败，请稍后重试。");
        }
    }

    public void ExitApplication()
    {
        if (_exiting)
        {
            return;
        }

        _exiting = true;
        SaveSettings();
        Log.Info("Exit requested by user.");
        _lifetime.Cancel();
        KeyboardWindowInstance?.Close();
        MainWindowInstance?.Close();
        Shutdown();
    }

    public static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private bool AcquireSingleInstance()
    {
        _singleInstanceMutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew)
        {
            try
            {
                if (EventWaitHandle.TryOpenExisting(ActivationEventName, out var existing))
                {
                    using (existing)
                    {
                        existing.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"Could not signal the existing instance: {ex.Message}");
            }

            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
            return false;
        }

        _activationEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivationEventName);
        _activationListener = Task.Run(ActivationLoop);
        return true;
    }

    private void ActivationLoop()
    {
        try
        {
            while (!_lifetime.IsCancellationRequested)
            {
                var signaled = _activationEvent?.WaitOne(500) ?? false;
                if (signaled)
                {
                    Dispatch(ShowMainWindow);
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // Normal shutdown.
        }
        catch (Exception ex)
        {
            Log.Error("Single-instance activation listener stopped unexpectedly.", ex);
        }
    }

    private void CreateTrayIcon()
    {
        _trayIconImage = TryLoadTrayIcon();
        var menu = new Forms.ContextMenuStrip
        {
            Font = new Font("Microsoft YaHei UI", 9F),
            ShowImageMargin = false
        };

        var openItem = new Forms.ToolStripMenuItem("打开肌忆五笔");
        openItem.Click += (_, _) => Dispatch(ShowMainWindow);

        var keyboardItem = new Forms.ToolStripMenuItem("显示置顶键盘");
        keyboardItem.Click += (_, _) => Dispatch(ToggleKeyboard);

        var updateItem = new Forms.ToolStripMenuItem("检查更新");
        updateItem.Click += (_, _) => Dispatch(() => _ = CheckForUpdatesAsync(true));

        var exitItem = new Forms.ToolStripMenuItem("退出肌忆五笔");
        exitItem.Click += (_, _) => Dispatch(ExitApplication);

        menu.Items.Add(openItem);
        menu.Items.Add(keyboardItem);
        menu.Items.Add(updateItem);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(exitItem);

        _trayIcon = new Forms.NotifyIcon
        {
            Icon = _trayIconImage,
            Text = $"{AppInfo.ChineseName} - 潜移默化学习五笔",
            Visible = true,
            ContextMenuStrip = menu
        };
        _trayIcon.DoubleClick += (_, _) => Dispatch(ShowMainWindow);
        _trayIcon.BalloonTipClicked += (_, _) => Dispatch(ShowMainWindow);

        _trayIcon.Tag = keyboardItem;
        UpdateTrayMenuText();
    }

    private Icon TryLoadTrayIcon()
    {
        try
        {
            var extracted = Icon.ExtractAssociatedIcon(AppPaths.ExecutablePath);
            if (extracted is not null)
            {
                return extracted;
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Unable to extract application icon: {ex.Message}");
        }

        return (Icon)SystemIcons.Application.Clone();
    }

    private void UpdateTrayMenuText()
    {
        if (_trayIcon?.Tag is Forms.ToolStripMenuItem item)
        {
            item.Text = KeyboardWindowInstance?.IsVisible == true
                ? "隐藏置顶键盘"
                : "显示置顶键盘";
        }
    }

    public void RefreshKeyboardStatus()
    {
        UpdateTrayMenuText();
        MainWindowInstance?.SetKeyboardStatus(KeyboardWindowInstance?.IsVisible == true);
    }

    private void Dispatch(Action action)
    {
        Dispatcher.BeginInvoke(action, DispatcherPriority.Normal);
    }

    private async Task CheckUpdatesSilentlyAsync()
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(4), _lifetime.Token);
            var result = await UpdateService.CheckAsync(_lifetime.Token);
            if (result.HasUpdate && !_autoUpdateAnnounced)
            {
                _autoUpdateAnnounced = true;
                _trayIcon?.ShowBalloonTip(
                    5000,
                    "肌忆五笔有新版本",
                    $"发现 {result.LatestVersion}，点击打开主界面查看更新。",
                    Forms.ToolTipIcon.Info);
            }
        }
        catch (OperationCanceledException)
        {
            // Application is exiting.
        }
        catch (Exception ex)
        {
            Log.Warning($"Automatic update check failed: {ex.Message}");
        }
    }

    private void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error("Unhandled UI exception.", e.Exception);
        MessageBox.Show(
            "程序遇到意外错误。详细信息已写入 logs 文件夹。",
            AppInfo.ChineseName,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Error("Unhandled application exception.", e.ExceptionObject as Exception);
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error("Unobserved task exception.", e.Exception);
        e.SetObserved();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        // Work is intentionally performed in OnStartup.
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        Cleanup();
    }

    private void Cleanup()
    {
        _lifetime.Cancel();
        if (_trayIcon is not null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }

        _trayIconImage?.Dispose();
        _trayIconImage = null;

        _activationEvent?.Set();
        _activationEvent?.Dispose();
        _activationEvent = null;

        if (_singleInstanceMutex is not null)
        {
            try
            {
                _singleInstanceMutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // The mutex was not held by this thread; disposal is sufficient.
            }

            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
        }
    }
}

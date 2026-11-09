using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using PassiveWubi.Models;

namespace PassiveWubi.Services;

public static class ThemeService
{
    public static void Apply(AppTheme theme, LogService? log = null)
    {
        try
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            var themeDictionary = dictionaries.FirstOrDefault(dictionary =>
                dictionary.Source?.OriginalString.Contains("Theme.xaml", StringComparison.OrdinalIgnoreCase) == true);
            var replacement = new ResourceDictionary
            {
                Source = new Uri(
                    $"pack://application:,,,/PassiveWubi;component/Themes/{theme}Theme.xaml",
                    UriKind.Absolute)
            };

            if (themeDictionary is null)
            {
                dictionaries.Insert(0, replacement);
            }
            else
            {
                var index = dictionaries.IndexOf(themeDictionary);
                dictionaries[index] = replacement;
            }

            foreach (Window window in Application.Current.Windows)
            {
                ApplyWindowChrome(window, theme == AppTheme.Dark);
            }

            log?.Info($"Application theme changed to {theme}.");
        }
        catch (Exception ex)
        {
            log?.Error($"Unable to apply {theme} theme.", ex);
        }
    }

    public static void ApplyWindowChrome(Window window, bool? darkOverride = null)
    {
        try
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            var isDark = darkOverride
                ?? ((App)Application.Current).Settings.Theme == AppTheme.Dark;
            var enabled = isDark ? 1 : 0;
            var result = DwmSetWindowAttribute(handle, 20, ref enabled, sizeof(int));
            if (result != 0)
            {
                DwmSetWindowAttribute(handle, 19, ref enabled, sizeof(int));
            }

            SetWindowPos(
                handle,
                IntPtr.Zero,
                0,
                0,
                0,
                0,
                0x37);
        }
        catch
        {
            // Older Windows versions can keep the system title bar.
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attribute,
        ref int attributeValue,
        int attributeSize);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hwnd,
        IntPtr insertAfter,
        int x,
        int y,
        int width,
        int height,
        uint flags);
}

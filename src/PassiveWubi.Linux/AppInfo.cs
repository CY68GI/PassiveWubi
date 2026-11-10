using System.Reflection;

namespace PassiveWubi.Linux;

public static class AppInfo
{
    public const string EnglishName = "PassiveWubi";
    public const string ChineseName = "肌忆五笔";
    public const string RepositoryUrl = "https://github.com/CY68GI/PassiveWubi";
    public const string ReleasesUrl = "https://github.com/CY68GI/PassiveWubi/releases";
    public const string LatestReleaseApi = "https://api.github.com/repos/CY68GI/PassiveWubi/releases/latest";

    public static string Version =>
        (Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(1, 1, 0)).ToString(3);

    public static string DisplayVersion => $"v{Version}";
}

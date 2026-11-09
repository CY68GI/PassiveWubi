using System.Reflection;

namespace PassiveWubi;

public static class AppInfo
{
    public const string EnglishName = "PassiveWubi";
    public const string ChineseName = "肌忆五笔";
    public const string Repository = "CY68GI/PassiveWubi";
    public const string RepositoryUrl = "https://github.com/CY68GI/PassiveWubi";
    public const string ReleasesUrl = "https://github.com/CY68GI/PassiveWubi/releases";
    public const string LatestReleaseApi = "https://api.github.com/repos/CY68GI/PassiveWubi/releases/latest";
    public const string IssuesUrl = "https://github.com/CY68GI/PassiveWubi/issues";

    public static string Version
    {
        get
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            return assemblyVersion is null
                ? "1.0.0"
                : assemblyVersion.ToString(3);
        }
    }

    public static string DisplayVersion => $"v{Version}";
}

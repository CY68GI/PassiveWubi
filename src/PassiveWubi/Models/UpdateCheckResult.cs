namespace PassiveWubi.Models;

public sealed record UpdateCheckResult(
    bool Success,
    bool HasUpdate,
    string CurrentVersion,
    string LatestVersion,
    string ReleaseUrl,
    string ReleaseNotes,
    string? Error = null)
{
    public static UpdateCheckResult Failed(string currentVersion, string error) =>
        new(false, false, currentVersion, string.Empty, AppInfo.ReleasesUrl, string.Empty, error);
}

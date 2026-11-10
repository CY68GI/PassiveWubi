namespace PassiveWubi.Linux.Models;

public sealed record LinuxUpdateResult(
    bool Success,
    bool HasUpdate,
    string CurrentVersion,
    string LatestVersion,
    string ReleaseUrl,
    string? Error = null);

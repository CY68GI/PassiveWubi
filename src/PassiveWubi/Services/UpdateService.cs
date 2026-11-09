using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using PassiveWubi.Models;

namespace PassiveWubi.Services;

public sealed partial class UpdateService
{
    private static readonly HttpClient Client = CreateClient();
    private readonly LogService _log;

    public UpdateService(LogService log)
    {
        _log = log;
    }

    public async Task<UpdateCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var currentVersion = AppInfo.Version;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, AppInfo.LatestReleaseApi);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

            using var response = await Client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var release = await JsonSerializer.DeserializeAsync<GitHubRelease>(
                    stream, cancellationToken: cancellationToken);
                if (release is not null && TryParseVersion(release.TagName, out var latest))
                {
                    var hasUpdate = latest > new Version(currentVersion);
                    _log.Info($"Update check completed. Current={currentVersion}; Latest={latest}; HasUpdate={hasUpdate}.");
                    return new UpdateCheckResult(
                        true,
                        hasUpdate,
                        currentVersion,
                        latest.ToString(3),
                        release.HtmlUrl ?? AppInfo.ReleasesUrl,
                        release.Body ?? string.Empty);
                }
            }

            return await CheckFromRedirectAsync(currentVersion, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _log.Warning($"GitHub API update check failed: {ex.Message}");
            return await CheckFromRedirectAsync(currentVersion, cancellationToken);
        }
    }

    private async Task<UpdateCheckResult> CheckFromRedirectAsync(
        string currentVersion,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await Client.GetAsync(
                $"{AppInfo.ReleasesUrl}/latest",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            var finalUrl = response.RequestMessage?.RequestUri?.ToString() ?? string.Empty;
            var match = ReleaseTagRegex().Match(finalUrl);
            if (match.Success && TryParseVersion(match.Groups[1].Value, out var latest))
            {
                var hasUpdate = latest > new Version(currentVersion);
                _log.Info($"Update check completed through release redirect. Latest={latest}; HasUpdate={hasUpdate}.");
                return new UpdateCheckResult(
                    true,
                    hasUpdate,
                    currentVersion,
                    latest.ToString(3),
                    finalUrl,
                    string.Empty);
            }

            return UpdateCheckResult.Failed(currentVersion, "暂时无法从 GitHub 获取版本信息。");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _log.Warning($"Redirect update check failed: {ex.Message}");
            return UpdateCheckResult.Failed(currentVersion, FriendlyNetworkError(ex));
        }
    }

    public static bool TryParseVersion(string? tag, out Version version)
    {
        version = new Version(0, 0, 0);
        if (string.IsNullOrWhiteSpace(tag))
        {
            return false;
        }

        var normalized = tag.Trim().TrimStart('v', 'V');
        var numeric = NumericVersionRegex().Match(normalized).Value;
        if (!Version.TryParse(numeric, out var parsed))
        {
            return false;
        }

        version = parsed;
        return true;
    }

    private static string FriendlyNetworkError(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException
            ? "网络连接失败，请稍后重试。"
            : "更新检查失败，请稍后重试。";

    private static HttpClient CreateClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        };
        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"PassiveWubi/{AppInfo.Version}");
        return client;
    }

    [GeneratedRegex(@"/releases/tag/([^/?#]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ReleaseTagRegex();

    [GeneratedRegex(@"\d+(?:\.\d+){2,3}")]
    private static partial Regex NumericVersionRegex();

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }
    }
}

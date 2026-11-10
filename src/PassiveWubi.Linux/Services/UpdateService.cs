using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using PassiveWubi.Linux.Models;

namespace PassiveWubi.Linux.Services;

public sealed partial class UpdateService
{
    private static readonly HttpClient Client = CreateClient();
    private readonly LogService _log;

    public UpdateService(LogService log)
    {
        _log = log;
    }

    public async Task<LinuxUpdateResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var current = AppInfo.Version;
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
                    var result = new LinuxUpdateResult(
                        true,
                        latest > new Version(current),
                        current,
                        latest.ToString(3),
                        release.HtmlUrl ?? AppInfo.ReleasesUrl,
                        null);
                    _log.Info($"Update check: current={current}, latest={result.LatestVersion}.");
                    return result;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _log.Error("GitHub API update check failed.", ex);
        }

        return await CheckFromRedirectAsync(current, cancellationToken);
    }

    private async Task<LinuxUpdateResult> CheckFromRedirectAsync(
        string current,
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
                return new LinuxUpdateResult(
                    true,
                    latest > new Version(current),
                    current,
                    latest.ToString(3),
                    finalUrl,
                    null);
            }

            return new LinuxUpdateResult(false, false, current, string.Empty, AppInfo.ReleasesUrl,
                "暂时无法获取最新版本。");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _log.Error("Linux update redirect check failed.", ex);
            return new LinuxUpdateResult(false, false, current, string.Empty, AppInfo.ReleasesUrl,
                "网络连接失败，请稍后重试。");
        }
    }

    private static bool TryParseVersion(string? tag, out Version version)
    {
        version = new Version(0, 0, 0);
        var match = NumericVersionRegex().Match(tag?.TrimStart('v', 'V') ?? string.Empty);
        if (!Version.TryParse(match.Value, out var parsed))
        {
            return false;
        }

        version = parsed;
        return true;
    }

    private static HttpClient CreateClient()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        };
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"PassiveWubi-Linux/{AppInfo.Version}");
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
    }
}

#region Purpose
// update-icons network lookups: upstream heroicons release, nuget.org versions, open sync PR
#endregion
#region Design
// GitHub lookup sends GITHUB_TOKEN / GH_TOKEN as a bearer token when set (anonymous calls
// hit the shared 60/h rate limit on hosted runners). Every failure is a clean error line plus
// a non-zero exit code; callers get null and stop.
#endregion

using System.Net;
using System.Net.Http.Headers;

namespace DevCli.Commands;

internal sealed partial class UpdateIconsCommand
{
  internal sealed partial class Handler
  {
    private static HttpClient CreateHttpClient()
    {
      HttpClient client = new() { Timeout = TimeSpan.FromSeconds(60) };
      client.DefaultRequestHeaders.UserAgent.ParseAdd("timewarp-heroicons-dev-cli");
      return client;
    }

    private static string? GitHubToken()
    {
      string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
      if (string.IsNullOrWhiteSpace(token))
      {
        token = Environment.GetEnvironmentVariable("GH_TOKEN");
      }

      return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    private async Task<string?> FetchLatestHeroiconsVersionAsync(CancellationToken ct)
    {
      string? token = GitHubToken();
      using HttpClient client = CreateHttpClient();
      using HttpRequestMessage request = new(HttpMethod.Get, LatestReleaseUrl);
      request.Headers.Accept.ParseAdd("application/vnd.github+json");
      if (token is not null)
      {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
      }

      try
      {
        using HttpResponseMessage response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
          string detail = $"{(int)response.StatusCode} {response.ReasonPhrase}";
          if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests
            && response.Headers.TryGetValues("x-ratelimit-remaining", out IEnumerable<string>? remaining)
            && remaining.FirstOrDefault() == "0")
          {
            detail += " (GitHub API rate limit exceeded)";
          }

          string hint = token is null
            ? " Set GITHUB_TOKEN (or GH_TOKEN) to authenticate."
            : "";
          return FailLookup($"GitHub API returned {detail} for {LatestReleaseUrl}.{hint}");
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(ct);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        if (!document.RootElement.TryGetProperty("tag_name", out JsonElement tagElement)
          || tagElement.GetString() is not { Length: > 0 } tag)
        {
          return FailLookup("heroicons latest release response has no tag_name.");
        }

        return tag.StartsWith('v') ? tag[1..] : tag;
      }
      catch (HttpRequestException ex)
      {
        return FailLookup($"could not reach the GitHub API: {ex.Message}");
      }
      catch (TaskCanceledException) when (!ct.IsCancellationRequested)
      {
        return FailLookup("timed out calling the GitHub API.");
      }
      catch (JsonException ex)
      {
        return FailLookup($"GitHub API returned invalid JSON: {ex.Message}");
      }
    }

    private string? FailLookup(string message)
    {
      Terminal.WriteErrorLine($"Error: upstream heroicons release lookup failed: {message}".Red());
      Environment.ExitCode = 1;
      return null;
    }

    // Props already match (or lead) upstream. Only "up to date" when the pin equals upstream and
    // nuget.org has the identity; a pin ahead of upstream is reported as published, never up to date.
    private async Task ReportPublishStateAsync(string currentVersion, bool aheadOfUpstream, CancellationToken ct)
    {
      string identity = NuGetIdentity(currentVersion);
      bool? published = await IsPublishedOnNuGetAsync(identity, ct);
      if (published is null) return; // error already reported

      if (published.Value)
      {
        Terminal.WriteLine(aheadOfUpstream
          ? $"{PackageId} {identity} is on nuget.org; the props pin still leads upstream heroicons.".Yellow()
          : $"Icons are already up to date ({PackageId} {identity} is on nuget.org).".Green());
        return;
      }

      Terminal.WriteErrorLine(
        $"Release pending: props say {currentVersion} but {PackageId} {identity} is not on nuget.org.".Red());
      Terminal.WriteErrorLine(
        $"Publish it through the normal path: merge any open sync PR, then create a GitHub release with tag v{identity} on the base branch.");
      Environment.ExitCode = 1;
    }

    private async Task<bool?> IsPublishedOnNuGetAsync(string identity, CancellationToken ct)
    {
      using HttpClient client = CreateHttpClient();
      try
      {
        using HttpResponseMessage response = await client.GetAsync(NuGetVersionsUrl, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return false;
        if (!response.IsSuccessStatusCode)
        {
          return FailNuGet($"{(int)response.StatusCode} {response.ReasonPhrase}");
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(ct);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        if (!document.RootElement.TryGetProperty("versions", out JsonElement versions)
          || versions.ValueKind != JsonValueKind.Array)
        {
          return FailNuGet("response has no versions array");
        }

        return versions.EnumerateArray()
          .Select(v => v.GetString())
          .Any(v => string.Equals(v, identity, StringComparison.OrdinalIgnoreCase));
      }
      catch (HttpRequestException ex)
      {
        return FailNuGet(ex.Message);
      }
      catch (TaskCanceledException) when (!ct.IsCancellationRequested)
      {
        return FailNuGet("timed out");
      }
      catch (JsonException ex)
      {
        return FailNuGet($"invalid JSON: {ex.Message}");
      }
    }

    private bool? FailNuGet(string message)
    {
      Terminal.WriteErrorLine($"Error: could not check nuget.org for published versions: {message}".Red());
      Environment.ExitCode = 1;
      return null;
    }

    // Returns the open PR URL, "" when none is open, or null when gh failed.
    private async Task<string?> FindOpenPullRequestAsync(string repoRoot, string branch, CancellationToken ct)
    {
      CommandOutput output = await Shell.Builder("gh")
        .WithArguments("pr", "list", "--head", branch, "--state", "open", "--json", "url", "--jq", ".[0].url // \"\"")
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .CaptureAsync(ct);

      if (!output.Success)
      {
        Terminal.WriteErrorLine($"Error: gh pr list failed (is gh installed and GH_TOKEN set?): {output.Combined.Trim()}".Red());
        Environment.ExitCode = 1;
        return null;
      }

      return output.Stdout.Trim();
    }
  }
}

#region Purpose
// Re-enables a GitHub Actions workflow so its schedule is not disabled for inactivity
#endregion
#region Design
// GitHub disables a scheduled workflow after 60 days with no repository activity. Calling
// PUT /repos/{owner}/{repo}/actions/workflows/{file}/enable resets that timer and returns 204 for
// an already active workflow too (`gh workflow enable` only resolves disabled workflows, so the
// endpoint is called directly). sync-icons.yml runs this from its own `keepalive` job with
// `actions: write` only, so a failing sync never skips it.
// Repo comes from --repo, else GITHUB_REPOSITORY; token from GH_TOKEN, else GITHUB_TOKEN.
// Any missing input or non-2xx response is one clear error line plus a non-zero exit code.
#endregion

using System.Net.Http.Headers;

namespace DevCli.Commands;

[NuruRoute("workflow-keepalive", Description = "Re-enable a GitHub Actions workflow to reset its 60-day inactivity timer")]
internal sealed class WorkflowKeepaliveCommand : ICommand<Unit>
{
  [Option("workflow", Description = "Workflow file name under .github/workflows (for example sync-icons.yml)")]
  public string? Workflow { get; set; }

  [Option("repo", Description = "owner/name of the repository (default: GITHUB_REPOSITORY)")]
  public string? Repo { get; set; }

  internal sealed class Handler : IRequestHandler<WorkflowKeepaliveCommand, Unit>
  {
    private const string ApiBase = "https://api.github.com";
    private readonly ITerminal Terminal;

    public Handler(ITerminal terminal)
    {
      Terminal = terminal;
    }

    public async ValueTask<Unit> Handle(WorkflowKeepaliveCommand command, CancellationToken ct)
    {
      string? workflow = command.Workflow?.Trim();
      if (string.IsNullOrEmpty(workflow))
      {
        return Fail("--workflow is required (for example --workflow sync-icons.yml).");
      }

      string? repo = FirstNonEmpty(command.Repo, Environment.GetEnvironmentVariable("GITHUB_REPOSITORY"));
      if (repo is null || repo.Split('/') is not [{ Length: > 0 }, { Length: > 0 }])
      {
        return Fail("repository not set: pass --repo owner/name or set GITHUB_REPOSITORY.");
      }

      string? token = FirstNonEmpty(
        Environment.GetEnvironmentVariable("GH_TOKEN"),
        Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
      if (token is null)
      {
        return Fail("no token: set GH_TOKEN or GITHUB_TOKEN (needs actions: write on the repository).");
      }

      string url = $"{ApiBase}/repos/{repo}/actions/workflows/{Uri.EscapeDataString(workflow)}/enable";

      using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(60) };
      client.DefaultRequestHeaders.UserAgent.ParseAdd("timewarp-heroicons-dev-cli");
      using HttpRequestMessage request = new(HttpMethod.Put, url);
      request.Headers.Accept.ParseAdd("application/vnd.github+json");
      request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

      try
      {
        using HttpResponseMessage response = await client.SendAsync(request, ct);
        if (response.IsSuccessStatusCode)
        {
          Terminal.WriteLine($"Workflow {workflow} in {repo} is enabled; its inactivity timer is reset ({(int)response.StatusCode}).".Green());
          return Value;
        }

        string body = (await response.Content.ReadAsStringAsync(ct)).Trim();
        string detail = body.Length > 300 ? body[..300] + "..." : body;
        string hint = (int)response.StatusCode switch
        {
          401 => " The token is invalid or expired.",
          403 => " The token needs actions: write on this repository (job permissions: actions: write).",
          404 => $" Check that .github/workflows/{workflow} exists on the default branch of {repo} and the token can see the repository.",
          _ => ""
        };
        return Fail($"PUT {url} returned {(int)response.StatusCode} {response.ReasonPhrase}.{hint} Response: {detail}");
      }
      catch (HttpRequestException ex)
      {
        return Fail($"could not reach the GitHub API: {ex.Message}");
      }
      catch (TaskCanceledException) when (!ct.IsCancellationRequested)
      {
        return Fail("timed out calling the GitHub API.");
      }
    }

    private static string? FirstNonEmpty(params string?[] values) =>
      values.Select(v => v?.Trim()).FirstOrDefault(v => !string.IsNullOrEmpty(v));

    private Unit Fail(string message)
    {
      Terminal.WriteErrorLine($"Error: workflow keepalive failed: {message}".Red());
      Environment.ExitCode = 1;
      return Value;
    }
  }
}

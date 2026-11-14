#region Purpose
// Runs the Jaribu runfile tests under tests/dev-cli
#endregion
#region Design
// The solution has no test project for the dev CLI, so `dotnet test` on the slnx
// does not reach these tests. `dev test` and the `dev workflow` test step call this
// after `dotnet test`. Each *.cs runfile is a standalone Jaribu executable; the first
// non-zero exit code fails the step. A missing folder is not an error.
#endregion

namespace DevCli;

internal static class RunfileTests
{
  public static async Task<int> RunAsync(string repoRoot, ITerminal terminal, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(repoRoot);
    ArgumentNullException.ThrowIfNull(terminal);

    string testsDir = Path.Combine(repoRoot, "tests", "dev-cli");
    if (!Directory.Exists(testsDir)) return 0;

    foreach (string file in Directory.GetFiles(testsDir, "*.cs").Order(StringComparer.Ordinal))
    {
      terminal.WriteLine($"Running runfile tests: {Path.GetRelativePath(repoRoot, file)}");
      int exitCode = await Shell.Builder("dotnet")
        .WithArguments("run", "--file", file)
        .WithWorkingDirectory(repoRoot)
        .WithNoValidation()
        .RunAsync(ct);
      if (exitCode != 0) return exitCode;
    }

    return 0;
  }
}

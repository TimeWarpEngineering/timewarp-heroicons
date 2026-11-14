#region Purpose
// Maps a package <Version> to the upstream heroicons release it tracks
#endregion
#region Design
// Version policy: the first three parts of the package version always equal the
// upstream heroicons release (X.Y.Z). A TimeWarp-only re-ship (packaging, dependency
// pins) adds a fourth part R: X.Y.Z.R, with prereleases X.Y.Z.R-beta.N. NuGet orders
// 2.2.0 < 2.2.0.1-beta.1 < 2.2.0.1 < 2.2.0.2 < 2.2.1, so a revision sorts after the
// upstream release it revises and before the next one.
// Historical numeric +metadata (2.0.19+2.0.18) still names the pin. Older prerelease
// re-ships (2.0.12-update.1, 2.2.0-tw.1) drop the label and keep X.Y.Z.
// Pure and dependency-free so the tests/dev-cli runfiles can compile it directly.
#endregion

namespace DevCli;

internal static class HeroiconsVersion
{
  /// <summary>
  /// Returns the upstream heroicons version (first three numeric parts) that a package version tracks.
  /// </summary>
  /// <param name="version">The package version, e.g. <c>2.2.0.1-beta.1</c>.</param>
  /// <returns>The three-part upstream version, e.g. <c>2.2.0</c>.</returns>
  public static string ExtractUpstream(string version)
  {
    ArgumentNullException.ThrowIfNull(version);

    string pin = version.Trim();
    int plus = pin.IndexOf('+', StringComparison.Ordinal);
    if (plus >= 0)
    {
      string metadata = pin[(plus + 1)..];
      pin = Version.TryParse(metadata, out _) ? metadata : pin[..plus];
    }

    int dash = pin.IndexOf('-', StringComparison.Ordinal);
    if (dash >= 0)
    {
      pin = pin[..dash];
    }

    string[] parts = pin.Split('.');
    return parts.Length > 3 ? string.Join('.', parts[..3]) : pin;
  }
}

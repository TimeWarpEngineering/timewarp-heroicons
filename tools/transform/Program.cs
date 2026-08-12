using Scriban;
using System.Globalization;

string heroIconPath = args[0];
string outputPath = args[1];

Console.WriteLine($"input:{heroIconPath} output:{outputPath}");

if (!Directory.Exists(heroIconPath)) throw new Exception($"{heroIconPath} does not exist");

// Resolve template next to the built binary (CopyToOutputDirectory), not CWD.
// update-icons runs this tool with working directory = repo root, so a relative
// "template.scriban" path fails in CI schedule/sync jobs.
string templatePath = ResolveTemplatePath();
string templateContent = File.ReadAllText(templatePath);
Template template = Template.Parse(templateContent);

TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

foreach (string fullFileName in Directory.EnumerateFiles(heroIconPath, "*.*", SearchOption.AllDirectories))
{
  if (!string.Equals(Path.GetExtension(fullFileName), ".svg", StringComparison.OrdinalIgnoreCase))
  {
    continue;
  }

  string relativeDir = Path.GetDirectoryName(Path.GetRelativePath(heroIconPath, fullFileName)) ?? string.Empty;
  string normalizedDir = relativeDir.Replace('\\', '/');
  string fullOutputPath = Path.Combine(outputPath, relativeDir);

  string iconName = Path.GetFileNameWithoutExtension(fullFileName);
  string componentName = textInfo.ToTitleCase(iconName).Replace("-", string.Empty, StringComparison.Ordinal) + "Icon";
  string componentFullFileName = Path.Combine(fullOutputPath, $"{componentName}.razor");

  int size = ResolveSize(normalizedDir);
  string kind = normalizedDir.Contains("solid", StringComparison.OrdinalIgnoreCase) ? "Solid" : "Outline";
  string theNameSpace = ResolveNamespace(size, kind);

  string fileContent = File.ReadAllText(fullFileName);
  string content = Transform(fileContent, theNameSpace, template);

  Directory.CreateDirectory(fullOutputPath);
  File.WriteAllText(componentFullFileName, content);
}

static int ResolveSize(string normalizedRelativeDir)
{
  // heroicons optimized layout: 16/solid, 20/solid, 24/solid|outline
  if (normalizedRelativeDir.StartsWith("16/", StringComparison.Ordinal) ||
      normalizedRelativeDir.Equals("16", StringComparison.Ordinal))
  {
    return 16;
  }

  if (normalizedRelativeDir.StartsWith("20/", StringComparison.Ordinal) ||
      normalizedRelativeDir.Equals("20", StringComparison.Ordinal))
  {
    return 20;
  }

  return 24;
}

// 16 = Micro, 20 = Mini, 24 = default Solid/Outline (heroicons naming).
static string ResolveNamespace(int size, string kind) => size switch
{
  16 => $"TimeWarp.HeroIcons.Micro.{kind}",
  20 => $"TimeWarp.HeroIcons.Mini.{kind}",
  _ => $"TimeWarp.HeroIcons.{kind}",
};

static string ResolveTemplatePath()
{
  string[] candidates =
  [
    Path.Combine(AppContext.BaseDirectory, "template.scriban"),
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "template.scriban")),
    Path.Combine(Environment.CurrentDirectory, "template.scriban"),
    Path.Combine(Environment.CurrentDirectory, "tools", "transform", "template.scriban"),
  ];

  foreach (string candidate in candidates)
  {
    if (File.Exists(candidate)) return candidate;
  }

  throw new FileNotFoundException(
    "Could not find template.scriban. Expected it next to the transform binary (CopyToOutputDirectory) or under tools/transform/.",
    "template.scriban");
}

static string Transform(string fileContent, string theNamespace, Template template)
{
  // Inject Blazor attribute splatting on the root <svg>. Upstream markup varies
  // (width/height vs viewBox-only, fill none vs currentColor) across heroicons versions.
  string svg = InjectAttributes(fileContent);
  return template.Render(new { svg, theNamespace });
}

static string InjectAttributes(string svgMarkup)
{
  if (svgMarkup.Contains("@attributes", StringComparison.Ordinal))
  {
    return svgMarkup;
  }

  int svgOpen = svgMarkup.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
  if (svgOpen < 0)
  {
    return svgMarkup;
  }

  int tagEnd = svgMarkup.IndexOf('>', svgOpen);
  if (tagEnd < 0)
  {
    return svgMarkup;
  }

  return svgMarkup.Insert(tagEnd, " @attributes=Attributes");
}

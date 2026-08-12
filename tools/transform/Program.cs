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

var fullFileNames = Directory.EnumerateFiles(heroIconPath, "*.*", SearchOption.AllDirectories);

TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

foreach (string? fullFileName in fullFileNames)
{
    var relativePath = Path.GetRelativePath(heroIconPath, fullFileName);
    relativePath = Path.GetDirectoryName(relativePath) ?? string.Empty;
    string fullOutputPath = Path.Combine(outputPath, relativePath);

    string iconName = Path.GetFileNameWithoutExtension(fullFileName);
    string iconExtension = Path.GetExtension(fullFileName);
    if (iconExtension != ".svg") continue;

    char firstChar = iconName[0];
    bool firstCharIsValid = char.IsLetter(firstChar) || firstChar == '_';

    string componentName = firstCharIsValid ? iconName : $"_{iconName}";
    componentName = textInfo.ToTitleCase(iconName).Replace("-", string.Empty);
    string suffix = "Icon";
    componentName = $"{componentName}{suffix}";
    string componentFullFileName = Path.Combine(fullOutputPath, $"{componentName}.razor");

    string? fileContent = File.ReadAllText(fullFileName);
    int size = relativePath.Contains("20") ? 20 : 24;
    string kind = relativePath.Contains("solid") ? "Solid" : "Outline";
    string theNameSpace = size == 20 ? $"TimeWarp.HeroIcons.Mini.{kind}" : $"TimeWarp.HeroIcons.{kind}";
    string content = Transform(fileContent, componentName, size, theNameSpace, template);

    Directory.CreateDirectory(fullOutputPath);
    File.WriteAllText(componentFullFileName, content);
}

static string ResolveTemplatePath()
{
    string[] candidates =
    [
        Path.Combine(AppContext.BaseDirectory, "template.scriban"),
        // Fallback when running from tools/transform source tree
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

static string Transform(string fileContent, string componentName, int size, string theNamespace, Template template)
{
    string search = $"<svg width=\"{size}\" height=\"{size}\" viewBox=\"0 0 {size} {size}\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\">";
    string replacement = $"<svg width=\"{size}\" height=\"{size}\" viewBox=\"0 0 {size} {size}\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\" @attributes=Attributes>";

    var svg = fileContent.Replace(search, replacement);

    var componentContent = template.Render(new { svg, theNamespace });
    return componentContent;
}

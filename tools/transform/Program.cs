// See https://aka.ms/new-console-template for more information
using Scriban;
using System.Globalization;

string heroIconPath = args[0];
string outputPath = args[1];

Console.WriteLine($"input:{heroIconPath} output:{outputPath}");

if (!Directory.Exists(heroIconPath)) throw new Exception($"{heroIconPath} does not exist");
var fullFileNames = Directory.EnumerateFiles(heroIconPath,"*.*", SearchOption.AllDirectories);

TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

foreach (string? fullFileName in fullFileNames)
{
    var relativePath = Path.GetRelativePath(heroIconPath,fullFileName);
    relativePath = Path.GetDirectoryName(relativePath);
    string fullOutputPath = Path.Combine(outputPath, relativePath);

    string iconName = Path.GetFileNameWithoutExtension(fullFileName);
    string iconExtension = Path.GetExtension(fullFileName);
    if (iconExtension != ".svg") return;

    char firstChar = iconName[0];
    bool firstCharIsValid = char.IsLetter(firstChar) || firstChar == '_';
    
    string componentName = firstCharIsValid ? iconName : $"_{iconName}";
    componentName = textInfo.ToTitleCase(iconName).Replace("-", string.Empty);
    string suffix = "Icon";
    componentName = $"{componentName}{suffix}";
    string componentFullFileName = Path.Combine(new[] { fullOutputPath, $"{componentName}.razor" });

    string? fileContent = File.ReadAllText(fullFileName);
    int size = relativePath.Contains("20") ? 20 : 24;
    string kind = relativePath.Contains("solid") ? "Solid" : "Outline";
    string theNameSpace = size == 20 ? $"TimeWarp.HeroIcons.Mini.{kind}" : $"TimeWarp.HeroIcons.{kind}";
    string content = Transform(fileContent, componentName, size, theNameSpace);
    
    Directory.CreateDirectory(fullOutputPath);
    File.WriteAllText(componentFullFileName, content);
}

string Transform(string fileContent, string componentName, int size, string theNamespace)
{
    string search = $"<svg width=\"{size}\" height=\"{size}\" viewBox=\"0 0 {size} {size}\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\">";
    string replacement = $"<svg width=\"{size}\" height=\"{size}\" viewBox=\"0 0 {size} {size}\" fill=\"none\" xmlns=\"http://www.w3.org/2000/svg\" fill=\"currentColor\" @attributes=Attributes>";

    var svg = fileContent.Replace(search, replacement)
        .Replace("fill=\"none\"", "")
        .Replace("fill=\"#0F172A\"", "")
        .Replace("stroke=\"#0F172A\"", "")
        .Replace("fill-rule=\"evenodd\" clip-rule=\"evenodd\"", "stroke-linecap=\"round\" stroke-linejoin=\"round\"");

    var templateContent = File.ReadAllText("template.scriban");
    var template = Template.Parse(templateContent);
    var componentContent = template.Render(new { svg, theNamespace });
    return componentContent;
}

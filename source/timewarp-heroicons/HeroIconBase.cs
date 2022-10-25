namespace TimeWarp.HeroIcons;

using Microsoft.AspNetCore.Components;

public partial class HeroIconBase : ComponentBase
{
  [Parameter(CaptureUnmatchedValues = true)]
  public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
}

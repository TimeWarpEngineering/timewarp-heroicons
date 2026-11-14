#!/usr/bin/env -S dotnet --

#region Purpose
// Tests HeroiconsVersion.ExtractUpstream: package version → upstream heroicons X.Y.Z
#endregion

#if !JARIBU_MULTI
return await RunAllTests();
#endif

namespace HeroiconsVersion_
{
  [TestTag("Version")]
  public class ExtractUpstream_Given_
  {
    [ModuleInitializer]
    internal static void Register() => RegisterTests<ExtractUpstream_Given_>();

    public static async Task ThreePartVersion_Should_ReturnItUnchanged()
    {
      DevCli.HeroiconsVersion.ExtractUpstream("2.2.0").ShouldBe("2.2.0");
      await Task.CompletedTask;
    }

    public static async Task FourPartRevision_Should_ReturnFirstThreeParts()
    {
      DevCli.HeroiconsVersion.ExtractUpstream("2.2.0.1").ShouldBe("2.2.0");
      DevCli.HeroiconsVersion.ExtractUpstream("2.2.0.12").ShouldBe("2.2.0");
      await Task.CompletedTask;
    }

    public static async Task FourPartPrerelease_Should_ReturnFirstThreeParts()
    {
      DevCli.HeroiconsVersion.ExtractUpstream("2.2.0.1-beta.1").ShouldBe("2.2.0");
      await Task.CompletedTask;
    }

    public static async Task ThreePartPrerelease_Should_DropTheLabel()
    {
      DevCli.HeroiconsVersion.ExtractUpstream("2.2.0-tw.1").ShouldBe("2.2.0");
      DevCli.HeroiconsVersion.ExtractUpstream("2.0.12-update.1").ShouldBe("2.0.12");
      await Task.CompletedTask;
    }

    public static async Task NumericMetadataPin_Should_ReturnThePin()
    {
      DevCli.HeroiconsVersion.ExtractUpstream("2.0.19+2.0.18").ShouldBe("2.0.18");
      await Task.CompletedTask;
    }

    public static async Task NonNumericMetadata_Should_BeIgnored()
    {
      DevCli.HeroiconsVersion.ExtractUpstream("2.0.12+build-1").ShouldBe("2.0.12");
      DevCli.HeroiconsVersion.ExtractUpstream("2.2.0.1-beta.1+sha.abc").ShouldBe("2.2.0");
      await Task.CompletedTask;
    }
  }
}

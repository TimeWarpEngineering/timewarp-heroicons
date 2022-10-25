[![Dotnet](https://img.shields.io/badge/dotnet-6.0-blue)](https://dotnet.microsoft.com)
[![Stars](https://img.shields.io/github/stars/TimeWarpEngineering/timewarp-heroicons?logo=github)](https://github.com/TimeWarpEngineering/timewarp-heroicons)
[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)
[![nuget](https://img.shields.io/nuget/v/timewarp-heroicons?logo=nuget)](https://www.nuget.org/packages/timewarp-heroicons/)
[![nuget](https://img.shields.io/nuget/dt/timewarp-heroicons?logo=nuget)](https://www.nuget.org/packages/timewarp-heroicons/)
[![Issues Open](https://img.shields.io/github/issues/TimeWarpEngineering/timewarp-heroicons?logo=github)](https://github.com/TimeWarpEngineering/timewarp-heroicons/issues)
[![Forks](https://img.shields.io/github/forks/TimeWarpEngineering/timewarp-heroicons)](https://github.com/TimeWarpEngineering/timewarp-heroicons)
[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-heroicons?logo=github)](https://unlicense.org)
[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Fgithub.com%2FTimeWarpEngineering%2Ftimewarp-heroicons)](https://twitter.com/intent/tweet?url=https://github.com/TimeWarpEngineering/timewarp-heroicons)

[![Twitter](https://img.shields.io/twitter/follow/StevenTCramer.svg)](https://twitter.com/intent/follow?screen_name=StevenTCramer)
[![Twitter](https://img.shields.io/twitter/follow/TheFreezeTeam1.svg)](https://twitter.com/intent/follow?screen_name=TheFreezeTeam1)


# timewarp-heroicons
All the HeroIcons wrapped as Blazor components.

![TimeWarp Logo](assets/Logo.png)

All 2299 [heroicons](https://github.com/tailwindlabs/heroicons) wrapped as Blazor components.
See and search all at https://heroicons.com/

## Give a Star! :star:

If you like or are using this project please give it a star. Thank you!

## Usage

```razor
<TimeWarp.HeroIcons.Mini.Solid.AcademicCapIcon class="w-10 h-10" />
<TimeWarp.HeroIcons.Solid.AcademicCapIcon class="w-20 h-20" />
<TimeWarp.HeroIcons.Outline.AcademicCapIcon class="w-20 h-20" />
```

Outputs

![](assets/sample-output.png)  

## Installation

You can see the latest NuGet packages from the official [TimeWarp NuGet page](https://www.nuget.org/profiles/TimeWarp.Enterprises).

* [timewarp-heroicons](https://www.nuget.org/packages/timewarp-heroicons/) [![nuget](https://img.shields.io/nuget/v/timewarp-heroicons?logo=nuget)](https://www.nuget.org/packages/timewarp-heroicons/)

```console
dotnet add package timewarp-heroicons
```

## Unlicense

[![License](https://img.shields.io/github/license/TimeWarpEngineering/timewarp-heroicons?logo=github)](https://unlicense.org)

## Contributing

Time is of the essence.  Before developing a Pull Request I recommend opening a [discussion](https://github.com/TimeWarpEngineering/timewarp-heroicons/discussions).

Please feel free to make suggestions and help out with the [documentation](https://timewarpengineering.github.io/timewarp-heroicons/).
Please refer to [Markdown](http://daringfireball.net/projects/markdown/) for how to write markdown files.

## Contact

Sometimes the github notifications get lost in the shuffle.  If you file an [issue](https://github.com/TimeWarpEngineering/timewarp-heroicons/issues) and don't get a response in a timely manner feel free to ping on our [Discord server](https://discord.gg/A55JARGKKP).

[![Discord](https://img.shields.io/discord/715274085940199487?logo=discord)](https://discord.gg/7F4bS2T)

## References

https://github.com/heroicons/heroicons

### Commands used

```PowerShell
dotnet new sln
dotnet new razorclasslib -n timewarp-heroicons
dotnet sln add .\Source\timewarp-heroicons\timewarp-heroicons.csproj
dotnet new tool-manifest
dotnet tool install dotnet-cleanup
dotnet cleanup -y
```
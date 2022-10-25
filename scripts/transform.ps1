Push-Location $PSScriptRoot/../tools/transform
try {
    dotnet run --project transform.csproj -- "C:\git\github\tailwindlabs\heroicons\src\20\solid" "C:\git\github\TimeWarpEngineering\timewarp-heroicons\source\timewarp-heroicons\20\solid"
}
finally {
    Pop-Location
}

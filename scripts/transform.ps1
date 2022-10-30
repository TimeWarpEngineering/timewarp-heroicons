Push-Location $PSScriptRoot/../tools/transform
try {
    if (!$heroicons) { throw "heroicons should be set to the root path of where you cloned the heroicons repo"}
    dotnet run --project transform.csproj -- "$heroicons\src" "$PSScriptRoot\..\source\timewarp-heroicons"
}
finally {
    Pop-Location
}

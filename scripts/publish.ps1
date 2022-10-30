Push-Location $PSScriptRoot
try {
    if (!$Nuget_Key) { throw "Nuget_Key is not set"}
    dotnet pack ../Source/timewarp-heroicons/timewarp-heroicons.csproj -c Release
    Push-Location ../Source/timewarp-heroicons/bin/Release
    dotnet nuget push **/*.nupkg --source https://api.nuget.org/v3/index.json --api-key $Nuget_Key    
    Pop-Location
}
finally {
    Pop-Location
}

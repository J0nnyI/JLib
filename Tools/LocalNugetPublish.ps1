# mkdir  \NuGet\LocalPackages
cd ..
Remove-Item .\bin\local\* -r -force 

# Aktuelle Version extrahieren
$versionPrefix = "0.99"
$packageFiles = Get-ChildItem C:\NuGet\LocalPackages\*.nupkg -ErrorAction SilentlyContinue

# Höchste letzte Ziffer ermitteln
$maxPatch = 0
if ($packageFiles) {
    foreach ($file in $packageFiles) {
        # Ignoriere alles vor der Version, suche nach 0.99.<patch>.nupkg am Ende
        if ($file.Name -match "\.(\d+)\.nupkg$") {
            $patch = [int]$matches[1]
            if ($patch -gt $maxPatch) {
                $maxPatch = $patch
            }
        }
    }
}
$newPatch = $maxPatch + 1
$versionSuffix = "$versionPrefix.$newPatch"

dotnet pack -c Release -o .\bin\local --version-suffix $versionSuffix
cd .\bin\local
#nuget delete * 99.0.0 -Verbosity detailed -noninteractive -source C:\NuGet\LocalPackages 
#Remove-Item C:\NuGet\LocalPackages\JLib* -Force
dotnet nuget push * --source C:\NuGet\LocalPackages
cd ../../tools
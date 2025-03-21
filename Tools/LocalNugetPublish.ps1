# mkdir  \NuGet\LocalPackages
cd ..
Remove-Item .\\bin\\local\\* -r -force 
dotnet pack -c Release -o .\\bin\\local --version-suffix 0.99.17
cd .\\bin\\local
#nuget delete * 99.0.0 -Verbosity detailed -noninteractive -source C:\\NuGet\\LocalPackages 
Remove-Item C:\\NuGet\\LocalPackages\\JLib* -Force
dotnet nuget push * --source C:\\NuGet\\LocalPackages
cd ../../tools
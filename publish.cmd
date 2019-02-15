SET VERSION=1.11
SET KEY=
dotnet pack Arrowgene.Services/Arrowgene.Services.csproj --output ../nupkgs /p:Version=%VERSION%
dotnet nuget push nupkgs/Arrowgene.Services.%VERSION%.nupkg --api-key %KEY% --source https://www.nuget.org/api/v2/package

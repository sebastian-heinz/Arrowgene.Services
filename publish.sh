API_KEY=""
VERSION="1.3.0"

# https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack?tabs=netcore2x
dotnet pack Arrowgene.Services/Arrowgene.Services.csproj --output ../nupkgs /p:Version=$VERSION

# https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push
dotnet nuget push nupkgs/Arrowgene.Services.$VERSION.nupkg --api-key $API_KEY --source https://www.nuget.org/api/v2/package
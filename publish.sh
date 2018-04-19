#!/usr/bin/env bash
API_KEY=""
VERSION="1.6.7"

#MSBUILD="/Library/Frameworks/Mono.framework/Versions/5.4.0/Commands/msbuild"
#MSBUILD="msbuild"
#$MSBUILD Arrowgene.Services/Arrowgene.Services.csproj /t:pack /p:Configuration=Release /p:Version=$VERSION /p:PackageOutputPath=../nupkgs 

# https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack?tabs=netcore2x
dotnet pack Arrowgene.Services/Arrowgene.Services.csproj --output ../nupkgs /p:Version=$VERSION

# https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push
dotnet nuget push nupkgs/Arrowgene.Services.$VERSION.nupkg --api-key $API_KEY --source https://www.nuget.org/api/v2/package

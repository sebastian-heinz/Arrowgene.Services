#!/usr/bin/env bash
# https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish?tabs=netcore2x
VERSION=1
mkdir ./release
for RUNTIME in win-x86 win-x64 linux-x64 osx-x64; do
    # Server
    dotnet publish Arrowgene.Services.PingPong/Arrowgene.Services.PingPong.csproj --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:Version=$VERSION --runtime $RUNTIME --configuration Release --output ./publish/$RUNTIME-$VERSION/
    # Pack
    tar cjf ./release/$RUNTIME-$VERSION.tar.gz ./publish/$RUNTIME-$VERSION
done 
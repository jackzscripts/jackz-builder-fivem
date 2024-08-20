#!/bin/bash
cd Client
dotnet publish -c Release
cd ../Server
dotnet publish -c Release
cd ../

rm -rf dist
mkdir dist

cp fxmanifest.lua dist
mkdir dist/{Client,Server}
cp Client/bin/Release/net452/publish/* dist/Client/
cp -r Client/data/ dist/Client/

rm dist/Client/CitizenFX.Core.Client.dll
cp Server/bin/Release/netstandard2.0/publish/* dist/Server/
#!/bin/bash

if [ -z "$1" ];
then
  echo ----
  echo No version specified! Please specify a valid version like 1.2.3 or 1.2.3-rc1!
  exit 1
fi

echo ----
echo Starting building version $1

echo ----
echo Cleaning up
rm -rf ./artifacts
dotnet tool uninstall -g TravelAgent 2>/dev/null || true

echo ----
echo Restore solution
dotnet restore src/TravelAgent

echo ----
echo Packaging solution with Version = $1
dotnet pack src/TravelAgent -c Release -p:PackageVersion=$1 -p:Version=$1 -o ./artifacts/nupkgs/

echo ----
echo Installing TravelAgent globally with Version = $1
dotnet tool install --global --add-source ./artifacts/nupkgs/ TravelAgent

echo ----
echo Done

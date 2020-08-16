@echo off
set UALBION_PUB=src\UAlbion\bin\Release\netcoreapp3.0\publish
if not exist %UALBION_PUB% goto l1
RD /S /Q %UALBION_PUB%
:l1
dotnet publish -c Release src\ualbion.sln
::copy %UALBION_PUB%\native\win-x64\SDL2.dll src\ualbion\bin\Release\netcoreapp3.0\win-x64\publish
::copy libveldrid-spirv.dll %UALBION_PUB%
pushd %UALBION_PUB%

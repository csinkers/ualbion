@echo off
set UALBION_PUB=UAlbion\bin\Release\netcoreapp3.0\win-x64\publish
rm -r %UALBION_PUB%
dotnet publish -c Release ualbion.sln
copy %UALBION_PUB%\native\win-x64\SDL2.dll ualbion\bin\Release\netcoreapp3.0\win-x64\publish
copy libveldrid-spirv.dll %UALBION_PUB%
pushd %UALBION_PUB%

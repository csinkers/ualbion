@echo off
set BINDIR=%~dp0\build\UAlbion\bin\Release\net6.0

if x%1 == xextract goto extract
if x%1 == xbuild goto build
if x%1 == xpublish goto publish
if x%1 == xsr goto sr
if x%1 == xds goto ds
dotnet run --project src\ualbion -- %*
goto end

:extract
dotnet run --project src\Tools\Exporter
goto end

:build
dotnet build -c Release src\ualbion.full.sln
copy /Y %BINDIR%\native\win-x64\SDL2.dll %BINDIR%
copy /Y deps\veldrid-spirv\bin\Debug\libveldrid-spirv.dll %BINDIR%
pushd %BINDIR%
goto end

:publish
set UALBION_PUB=%BINDIR%\publish
if not exist %UALBION_PUB% goto l1
RD /S /Q %UALBION_PUB%
:l1
dotnet publish -c Release src\ualbion.nodeps.sln
::copy libveldrid-spirv.dll %UALBION_PUB%
pushd %UALBION_PUB%
goto end

:sr
pushd albion
runjob SR-Main
popd
goto end

:ds
dotnet run --project src\Tools\DumpSave -- albion\SAVES\SAVE.%2 %3 %4 %5 %6 %7 %8 %9
goto end

:end

@echo off
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
dotnet build -c Release src\ualbion.sln
copy /Y src\UAlbion\bin\Release\netcoreapp3.1\native\win-x64\SDL2.dll src\UAlbion\bin\Release\netcoreapp3.1
copy /Y deps\veldrid-spirv\bin\Debug\libveldrid-spirv.dll src\UAlbion\bin\Release\netcoreapp3.1
pushd src\UAlbion\bin\Release\netcoreapp3.1
goto end

:publish
set UALBION_PUB=src\UAlbion\bin\Release\netcoreapp3.1\publish
if not exist %UALBION_PUB% goto l1
RD /S /Q %UALBION_PUB%
:l1
dotnet publish -c Release src\ualbion.sln
::copy %UALBION_PUB%\native\win-x64\SDL2.dll src\ualbion\bin\Release\netcoreapp3.0\win-x64\publish
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

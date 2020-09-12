@echo off
if x%1 == xextract goto raw
dotnet run --project src\ualbion -- %*
goto end

:raw
dotnet run --project src\Tools\Exporter

:end

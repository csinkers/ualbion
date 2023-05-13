#!/bin/sh
dotnet run --project src/UAlbion --runtime linux-x64 --no-self-contained -- $*

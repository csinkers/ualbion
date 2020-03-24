# UAlbion
A remake of the 1995 RPG Albion 

Prerequisites: 
* .NET Core 3.0
* Game data from an install of the original game

## Screenshots:
![Example Screenshot 1](/data/Screenshots/1_FirstLevel.png?raw=true)
![Example Screenshot 2](/data/Screenshots/2_3DWorld.png?raw=true)
![Example Screenshot 3](/data/Screenshots/3_Outdoors.png?raw=true)
![Example Screenshot 4](/data/Screenshots/4_Inventory.png?raw=true)
![Example Screenshot 5](/data/Screenshots/5_MainMenu.png?raw=true)

## Current Status:
You can walk around, and move things about in your inventory but you can't interact with the environment much yet. Still a lot to do, e.g.:

- Audio subsystem, file formats are known, but the music is in the old XMI format which is difficult to play without artifacts. Can convert to MIDI, but some notes skip etc and the instruments don't match up too well.
- Handling all the event chains that are baked into the maps which define a lot of the gameplay
- Adding a conversation system (the GUI system it will be built on is mostly done at least)
- Implementing the combat system (huge job, will require lots of reverse engineering)
- Wide variety of miscellaneous gameplay behaviours

Main goal is to improve the interface and make some visual improvements, the original game was very clicky so adding some pathfinding and a more efficient way of examining and manipulating objects should improve usabability.

## Getting started

1. Configure `data/config.json` to set the paths for the files from the original game (currently only version v1.38 where MAIN.EXE is 1,109,655 bytes is supported). If you're running the GOG version, you'll want to mount the `game.gog` file (it's just a .bin format CD image) and then copy the ALBION directory into your UAlbion folder.
2. To compile and run the project, open `ualbion.sln` in the C# IDE of your choice or run `./run.sh` in Linux / `run.bat` in Windows. Any extra parameters to `run` will be passed through to UAlbion, `--help` will show the available options.

## Attributions
Many thanks to Florian Ziesche and the other contributers to the [freealbion wiki](https://github.com/freealbion/freealbion/wiki) for their efforts in discovering and documenting the Albion file formats.

Thanks to IllidanS4 for the ILBM loading code in [AlbLib](https://github.com/IllidanS4/AlbLib) (MIT License) which my InterlacedBitmap implementation was based on.

Thanks also to the authors of and contributers to the dependencies of this project.

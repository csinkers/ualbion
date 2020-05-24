# UAlbion
A remake of the 1995 RPG Albion 

Prerequisites: 
* .NET Core 3.0
* Game data from an install of the original game

Issue tracker at [https://trello.com/b/Tcm2WbU1/ualbion](https://trello.com/b/Tcm2WbU1/ualbion)

## Screenshots:
![Example Screenshot 1](/data/Screenshots/1_FirstLevel.png?raw=true)
![Example Screenshot 2](/data/Screenshots/2_3DWorld.png?raw=true)
![Example Screenshot 3](/data/Screenshots/3_Outdoors.png?raw=true)
![Example Screenshot 4](/data/Screenshots/4_Inventory.png?raw=true)
![Example Screenshot 5](/data/Screenshots/5_MainMenu.png?raw=true)

## Current Status:

Things that are at least somewhat implemented:
- Rendering of 2D and 3D environments
- Player movement and collision detection in 2D environments
- Interaction with the environment, e.g. examining objects, accessing chests and opening doors
- GUI system for menus, dialogs, inventory etc
- Inventory management
- Conversations
- Sound effects and music (music currently requires some manual steps to setup ADLMIDI.NET / libADLMIDI)
- Loading/saving saved games

Currently unimplemented:
- Lighting model for 3D levels
- Event handling and collision detection in 3D environments
- Automap for 3D environments
- Day/night cycle
- Combat system
- Magic system
- NPC movement
- Video playback

Planned improvements / changes from the original gameplay:
- Add hotkeys to streamline the interface, reduce the amount of right clicking required etc
- Add some pathfinding logic to make mouse-based movement easier
- Add a take-all button when looting chests / fallen foes
- Graphical improvements in 3D environments
- Fix bugs in original game (with option to toggle when there is a gameplay impact)
- At some point, modding support

## Getting started

1. Configure `data/config.json` to set the paths for the files from the original game (currently only version v1.38 where MAIN.EXE is 1,109,655 bytes is supported). If you're running the GOG version, you'll want to mount the `game.gog` file (it's just a .bin format CD image) and then copy the ALBION directory into your UAlbion folder.
2. To compile and run the project, open `ualbion.sln` in the C# IDE of your choice or run `./run.sh` in Linux / `run.bat` in Windows. Any extra parameters to `run` will be passed through to UAlbion, `--help` will show the available options.

## Attributions
Many thanks to Florian Ziesche and the other contributers to the [freealbion wiki](https://github.com/freealbion/freealbion/wiki) for their efforts in discovering and documenting the Albion file formats.

Thanks to IllidanS4 for the ILBM loading code in [AlbLib](https://github.com/IllidanS4/AlbLib) (MIT License) which my InterlacedBitmap implementation was based on.

Thanks also to the authors of and contributers to the dependencies of this project.

# UAlbion
A remake of the 1995 RPG Albion 

Prerequisites: 
* .NET Core 3.0
* Game data from an install of the original game

Issue tracker at [https://trello.com/b/Tcm2WbU1/ualbion](https://trello.com/b/Tcm2WbU1/ualbion)

## Screenshots
<details>
  <summary>Click to expand</summary>

  ![Example Screenshot 1](/data/Screenshots/1_FirstLevel.png?raw=true)
  ![Example Screenshot 2](/data/Screenshots/2_3DWorld.png?raw=true)
  ![Example Screenshot 3](/data/Screenshots/3_Outdoors.png?raw=true)
  ![Example Screenshot 4](/data/Screenshots/4_Inventory.png?raw=true)
  ![Example Screenshot 5](/data/Screenshots/5_MainMenu.png?raw=true)
</details>

## Current Status

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
1. To compile and run the project, open `ualbion.sln` in the C# IDE of your choice or run `./run.sh` in Linux / `run.bat` in Windows. Any extra parameters to `run` will be passed through to UAlbion, `--help` will show the available options.
    - To show available options: run -h
    - To run with Vulkan: run -vk
    - To run with OpenGL: run -gl
    - To run with Direct3D: run -d3d

To extract the required game files from the GOG version of the game:

### Linux:
1. Ensure wine, dosbox, dotnet-host, dotnet-runtime and dotnet-sdk are installed
1. Download the Windows installer for Albion from GOG
1. Run the installer using wine (`wine setup_albion_1.38_\(28043\).exe`)
1. Navigate to installed path (if you installed to the default path, `cd ~/.wine/drive_c/GOG\ Games/Albion/`)
1. Run `dosbox`
1. Run the following commands in dosbox to extract the data files: (replace ~/ualbion with wherever you cloned the ualbion repository)
    1. mount c ~/ualbion
    1. C:\Tools\GOG_EXTR.BAT

### Windows:
1. Download the Albion installer from GOG
1. Run installer
1. Open the Albion install directory in file explorer (e.g. C:\GOG Games\Albion)
1. Go into the DOSBOX directory and run DOSBOX.exe
1. Run the following commands in dosbox to extract the data files: (replace C:\Git\ualbion with wherever you cloned the ualbion repository. If the path contains spaces, you may need to surround it with double quotes.)
    1. mount c C:\Git\ualbion
    1. C:\Tools\GOG_EXTR.BAT

## Attributions
Many thanks to Florian Ziesche and the other contributers to the [freealbion wiki](https://github.com/freealbion/freealbion/wiki) for their efforts in discovering and documenting the Albion file formats.

Thanks to IllidanS4 for the ILBM loading code in [AlbLib](https://github.com/IllidanS4/AlbLib) (MIT License) which my InterlacedBitmap implementation was based on.

Thanks also to the authors of and contributers to the dependencies of this project.


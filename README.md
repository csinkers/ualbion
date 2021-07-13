# UAlbion
A remake of the 1995 RPG Albion 

Prerequisites: 
* .NET 5
* Game data from an install of the original game (full version or demo)

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
- Sound effects and music
- Loading/saving saved games
- Video playback
- Exporting assets
- Day/night cycle

Currently unimplemented:
- Lighting model for 3D levels
- Event handling and collision detection in 3D environments
- Automap for 3D environments
- Combat system
- Magic system
- NPC movement

Planned improvements / changes from the original gameplay:
- Add hotkeys to streamline the interface, reduce the amount of right clicking required etc
- Add some pathfinding logic to make mouse-based movement easier
- Add a take-all button when looting chests / fallen foes (done)
- Graphical improvements in 3D environments
- Fix bugs in original game (with option to toggle when there is a gameplay impact)
- Modding support
* A built-in editor for modifying and adding assets

## Getting started

### Game data

You need to have the original Albion game files. These days, Albion can be bought cheaply on [GOG](https://www.gog.com/game/albion).

If you have the GOG version of the game, extract the required game files as following:

#### Linux:
1. Ensure `wine` and `dosbox` are installed
1. Download the [Albion installer for Windows from GOG](https://www.gog.com/game/albion)
1. Run the installer using wine (`wine setup_albion_1.38_\(28043\).exe`). Note that the installer may show some errors, but if the game can be launched in the end, it's okay.
1. Run `dosbox`
1. Run the following commands in dosbox to extract the data files: (Replace `~/ualbion` with wherever you cloned the ualbion repository. Replace `~/.wine/drive_c/GOG Games/Albion/` with wherever you installed your GOG version of Albion. Note the double quotes (`"`), they are necessary if your path contains spaces.)
    1. `mount C "~/ualbion"`
    1. `mount D "~/.wine/drive_c/GOG Games/Albion/"`
    1. `C:\src\Tools\GOG_EXTR.BAT`

#### Windows:
1. Download the [Albion installer from GOG](https://www.gog.com/game/albion)
1. Run installer
1. Open the Albion install directory in file explorer (e.g. `C:\GOG Games\Albion`)
1. Go into the `DOSBOX` directory and run `DOSBOX.exe`
1. Run the following commands in dosbox to extract the data files: (Replace `C:\Git\ualbion` with wherever you cloned the ualbion repository. Replace `C:\GOG Games\Albion` with wherever you installed your GOG version of Albion. Note the double quotes (`"`), they are necessary if your path contains spaces.)
    1. `mount C "C:\Git\ualbion"`
    1. `mount D "C:\GOG Games\Albion"`
    1. `C:\src\Tools\GOG_EXTR.BAT`

If you did not do the above steps and want to select your game files manually, configure `data/config.json` to set the paths for the files from the original game (currently only version v1.38 where `MAIN.EXE` is 1,109,655 bytes and has a SHA256 hash of `476227b0391cf3452166b7a1d52b012ccf6c86bc9e46886dafbed343e9140710` is supported). If you're running the GOG version, you'll want to mount the `game.gog` file (it's just a raw binary dump of the CD contents) using CDemu and then copy the ALBION directory into your UAlbion folder.

### Compile and run

To compile and run the project, open `ualbion.sln` in the C# IDE of your choice or run `./run.sh` in Linux (ensure `dotnet-host`, `dotnet-runtime` and `dotnet-sdk` are installed) or `run.bat` in Windows. Any extra parameters to `run` will be passed through to UAlbion, `--help` will show the available options.
- To show available options: `run -h`
- To run with Vulkan: `run -vk`
- To run with OpenGL: `run -gl`
- To run with Direct3D: `run -d3d`

## Attributions
Many thanks to Florian Ziesche and the other contributers to the [freealbion wiki](https://github.com/freealbion/freealbion/wiki) for their efforts in discovering and documenting the Albion file formats.

Thanks to IllidanS4 for the ILBM loading code in [AlbLib](https://github.com/IllidanS4/AlbLib) (MIT License) which my InterlacedBitmap implementation was based on.

Thanks also to the authors of and contributers to the dependencies of this project:
- [Veldrid](https://github.com/mellinoe/veldrid) graphics API abstraction by Eric Mellino et al
- [ImGui](https://github.com/ocornut/imgui/) immediate mode graphics library by Omar Cornut et al
- [OpenAL](https://www.openal.org/) audio abstraction API from Loki Entertainment / Creative Technology
- [OpenAL-CS](https://github.com/flibitijibibo/OpenAL-CS) C# wrapper for OpenAL (using the NuGet package of the [OpenRA](https://github.com/OpenRA/OpenAL-CS) fork)
- [AdlMidi](https://github.com/Wohlstand/libADLMIDI) OPL-3 synthesiser library by Vitaly Novichkov, Joel Yliluoma et al
- [Json.NET](https://github.com/JamesNK/Newtonsoft.Json) JSON serialisation library by James Newton-King


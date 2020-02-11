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
- Loading and saving of saved games. Currently most of the loaders are only one-way, need to convert them so they can write as well as read.

Main goal is to improve the interface and make some visual improvements, the original game was very clicky so adding some pathfinding and a more efficient way of examining and manipulating objects should improve usabability.

## Getting started

1. Execute `./build.sh` in Linux or `build.bat` in Windows to compile it. Alternatively, open up the `ualbion.sln` with Visual Studio.
2. Configure `data/config.json` to set the paths for the files from the original game (currently only version v1.38 where MAIN.EXE is 1,109,655 bytes is supported)
3. Execute `./run.sh` in Linux or `run.bat` in Windows to play. 


Note: Some images (everything in PICTURE0.XLD) currently need to be:
* manually exported using the export tool
* converted from the old IBM interlaced bitmap format into regular bitmaps using a tool like ImageMagick
* saved in data/PICTURE0.XLD/00.bmp, data/PICTURE0.XLD/01.bmp etc. 

If this hasn't been done, then the menu and status bar backgrounds will not be able to load and will be replaced by a red 'invalid image' graphic.

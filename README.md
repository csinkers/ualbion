# ualbion
A remake of the 1996 RPG Albion (requires data from an install of the original game)

Prerequisites: .NET Core 3.0

![Example Screenshot 1](/data/Screenshots/1_FirstLevel.png?raw=true)
![Example Screenshot 2](/data/Screenshots/2_3DWorld.png?raw=true)
![Example Screenshot 3](/data/Screenshots/3_Outdoors.png?raw=true)
![Example Screenshot 4](/data/Screenshots/4_Inventory.png?raw=true)
![Example Screenshot 5](/data/Screenshots/5_MainMenu.png?raw=true)

See run.bat / run.sh for how to invoke dotnet to build and run it from the command line. Alternatively, open up the solution file with Visual Studio.
Configure data/config.json to set the path for the files from the original game (currently only version v1.38 where MAIN.EXE is 1,109,655 bytes is supported)


Note: Some images (everything in PICTURE0.XLD) currently need to be:
* manually exported using the export tool
* converted from the old IBM interlaced bitmap format into regular bitmaps using a tool like ImageMagick
* saved in data/PICTURE0.XLD/00.bmp, data/PICTURE0.XLD/01.bmp etc. 

If this hasn't been done, then the menu and status bar backgrounds will not be able to load and will be replaced by a red 'invalid image' graphic.

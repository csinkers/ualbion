﻿using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Ids;
using static BuildTestingMaps.Constants;

namespace BuildTestingMaps;

public class TestLab
{
    public int BlankFloorOffset { get; }
    public int TextFloorOffset { get; }
    // public int MonsterObjOffset { get; }
    // public byte SolidWallOffset { get; }
    // public int OneMonsterObjGroupOffset { get; }
    // public int TwoMonstersObjGroupOffset { get; }
    public LabyrinthData Lab { get; }

    const string ValidCharacters = " abcdefghijklmnopqrstuvwxyz0123456789";
    public byte FloorIndexForChar(char c)
    {
        int index = ValidCharacters.IndexOf(c);
        if (index == -1)
            return 0;
        return (byte)(index + TextFloorOffset);
    }

    public TestLab(Dictionary<AssetId, object> assets, IAssetManager assetManager)
    {
        Lab = new LabyrinthData(Labyrinth.Jirinaar)
        {
            CameraHeight = 128,
            BackgroundColour = CFlesh5,
            BackgroundId = DungeonBackground.EarlyGameS,
            BackgroundTileAmount = 1,
            BackgroundYPosition = 1,
            //FogRed = 48,
            //FogGreen = 32,
            //FogBlue = 32,
            //FogDistance = 24,
            //FogMode = 1,
            //MaxLight = 16,
            //Lighting = 1,
            MaxVisibleTiles = 32,
            WallHeight = 196,
            WallWidth = 8,
        };

        BlankFloorOffset = Lab.FloorAndCeilings.Count + 1; // Skip 'blank' pseudo entry
        var brick = T64((SpriteId)Floor.Brick).FillAll(CGrey12).Border(CBlue2).Texture;
        assets[(SpriteId)brick.Id] = brick;
        Lab.FloorAndCeilings.Add(new() { SpriteId = Floor.Brick, });
/*
        MonsterObjOffset = Lab.Objects.Count;
        Assets[(SpriteId)DungeonObject.EvilKangaroo] = TextureBuilder.Create<byte>(48, 72).Border(CFlesh4).Text("!!", CBlueGrey5, 12, 40, bigFont).Texture;
        Lab.Objects.Add(new LabyrinthObject
        {
            SpriteId = DungeonObject.EvilKangaroo,
            Width = 48,
            Height = 72,
            Collision = 8,
            MapWidth = 48,
            MapHeight = 72,
            Properties = 0,
        });

        OneMonsterObjGroupOffset = Lab.ObjectGroups.Count + 1;
        var oneSkrinn = new ObjectGroup { SubObjects = { [0] = new SubObject(0, 0, 0, 0) } };
        Lab.ObjectGroups.Add(oneSkrinn);

        TwoMonstersObjGroupOffset =Lab.ObjectGroups.Count + 1;
        var twoSkrinn =  new ObjectGroup { SubObjects =
        {
            [0] = new SubObject(0, 0, 0, 0),
            [1] = new SubObject(0, 100, 20, 0)
        } };
        Lab.ObjectGroups.Add(twoSkrinn);
*//*
        SolidWallOffset = (byte)(Lab.Walls.Count + LabyrinthData.WallOffset);
        var wall1 = T64((SpriteId)UAlbion.Base.Wall.JiriTileAndStone).FillAll(CGreen2).Texture;
        Assets[(SpriteId)wall1.Id] = wall1;
        Lab.Walls.Add(new Wall
        {
            Width = (ushort)wall1.Width,
            Height = (ushort)wall1.Height,
            SpriteId = UAlbion.Base.Wall.JiriTileAndStone,
            Collision = 8,
            Properties = Wall.WallFlags.WriteOverlay, // Actually type?
        });
*/
        TextFloorOffset = Lab.FloorAndCeilings.Count + 1;
        int gfxIndex = 2;
        var bigFont = assetManager.LoadFont(Font.DebugFont10, Ink.White);
        foreach (var c in ValidCharacters)
        {
            var spriteId = new SpriteId(AssetType.Floor, gfxIndex);
            assets[spriteId] = T64(spriteId).FillAll(CBlueGrey7).Border(COrange3).Text(c.ToString(), CGreen5, 2, 2, bigFont).Texture;
            Lab.FloorAndCeilings.Add(new FloorAndCeiling { Properties = 0, SpriteId = spriteId, });
            gfxIndex++;
        }

        assets[Lab.Id] = Lab;
    }
}
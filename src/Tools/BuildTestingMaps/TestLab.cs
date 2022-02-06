using UAlbion.Base;
using UAlbion.Config;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using static BuildTestingMaps.Constants;
using Wall = UAlbion.Formats.Assets.Labyrinth.Wall;

namespace BuildTestingMaps;

public class TestLab
{
    public int BlankFloorOffset { get; }
    public int TextFloorOffset { get; }
    public int MonsterObjOffset { get; }
    public byte SolidWallOffset { get; }
    public int OneMonsterObjGroupOffset { get; }
    public int TwoMonstersObjGroupOffset { get; }
    public LabyrinthData Lab { get; }
    public Dictionary<AssetId, object> Assets { get; } = new();

    const string ValidCharacters = " abcdefghijklmnopqrstuvwxyz0123456789";
    public byte FloorIndexForChar(char c)
    {
        int index = ValidCharacters.IndexOf(c);
        if (index == -1)
            return 0;
        return (byte)(index + TextFloorOffset);
    }

    public TestLab(ITextureBuilderFont font, ITextureBuilderFont bigFont)
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
        Assets[(SpriteId)Floor.Brick] = T64.FillAll(CGrey12).Border(CBlue2).Texture;
        Lab.FloorAndCeilings.Add(new() { AnimationCount = 0, Properties = 0, SpriteId = Floor.Brick, });

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

        SolidWallOffset = (byte)(Lab.Walls.Count + LabyrinthData.WallOffset);
        Assets[(SpriteId)UAlbion.Base.Wall.JiriTileAndStone] = T64.FillAll(CGreen2).Texture;
        Lab.Walls.Add(new Wall
        {
            Width = 64,
            Height = 64,
            SpriteId = UAlbion.Base.Wall.JiriTileAndStone,
            Collision = 8,
            Properties = 0,
        });

        TextFloorOffset = Lab.FloorAndCeilings.Count + 1;
        int gfxIndex = 2;
        foreach (var c in ValidCharacters)
        {
            var spriteId = new SpriteId(AssetType.Floor, gfxIndex);
            Assets[spriteId] = T64.FillAll(CBlueGrey7).Border(COrange3).Text(c.ToString(), CGreen5, 2, 2, bigFont).Texture;
            Lab.FloorAndCeilings.Add(new() { AnimationCount = 0, Properties = 0, SpriteId = spriteId, });
            gfxIndex++;
        }

        Assets[Lab.Id] = Lab;
    }

}
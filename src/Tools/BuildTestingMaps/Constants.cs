using UAlbion.Api;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Veldrid.Assets;

namespace BuildTestingMaps;

public static class Constants
{
    const string FontInfoPath = @"C:\Depot\bb\ualbion\mods\UATestDev\Fonts.json";
    const string FontPngPath = @"C:\Depot\bb\ualbion\mods\UATestDev\Fonts.png";
    public const int TileWidth = 16;
    public const int TileHeight = 16;

    // Common colors
    public const byte CBlack1 = (byte)CommonColor.Black1;
    public const byte CBlack2 = (byte)CommonColor.Black2;
    public const byte CWhite = (byte)CommonColor.White;
    public const byte CBlueGrey7 = (byte)CommonColor.BlueGrey7;
    public const byte CBlueGrey6 = (byte)CommonColor.BlueGrey6;
    public const byte CBlueGrey5 = (byte)CommonColor.BlueGrey5;
    public const byte CBlueGrey4 = (byte)CommonColor.BlueGrey4;
    public const byte CBlueGrey3 = (byte)CommonColor.BlueGrey3;
    public const byte CBlueGrey2 = (byte)CommonColor.BlueGrey2;
    public const byte CBlueGrey1 = (byte)CommonColor.BlueGrey1;
    public const byte CLavender = (byte)CommonColor.Lavender;
    public const byte CPurple = (byte)CommonColor.Purple;
    public const byte CReddishBlue = (byte)CommonColor.ReddishBlue;
    public const byte CBluishRed = (byte)CommonColor.BluishRed;
    public const byte CLightBurgundy = (byte)CommonColor.LightBurgundy;
    public const byte CBurgundy = (byte)CommonColor.Burgundy;
    public const byte COrange5 = (byte)CommonColor.Orange5;
    public const byte COrange4 = (byte)CommonColor.Orange4;
    public const byte COrange3 = (byte)CommonColor.Orange3;
    public const byte COrange2 = (byte)CommonColor.Orange2;
    public const byte COrange1 = (byte)CommonColor.Orange1;
    public const byte CGreen6 = (byte)CommonColor.Green6;
    public const byte CGreen5 = (byte)CommonColor.Green5;
    public const byte CGreen4 = (byte)CommonColor.Green4;
    public const byte CGreen3 = (byte)CommonColor.Green3;
    public const byte CGreen2 = (byte)CommonColor.Green2;
    public const byte CGreen1 = (byte)CommonColor.Green1;
    public const byte CYellow5 = (byte)CommonColor.Yellow5;
    public const byte CYellow4 = (byte)CommonColor.Yellow4;
    public const byte CYellow3 = (byte)CommonColor.Yellow3;
    public const byte CYellow2 = (byte)CommonColor.Yellow2;
    public const byte CYellow1 = (byte)CommonColor.Yellow1;
    public const byte CTeal4 = (byte)CommonColor.Teal4;
    public const byte CTeal3 = (byte)CommonColor.Teal3;
    public const byte CTeal2 = (byte)CommonColor.Teal2;
    public const byte CTeal1 = (byte)CommonColor.Teal1;
    public const byte CBlue4 = (byte)CommonColor.Blue4;
    public const byte CBlue3 = (byte)CommonColor.Blue3;
    public const byte CBlue2 = (byte)CommonColor.Blue2;
    public const byte CBlue1 = (byte)CommonColor.Blue1;
    public const byte CFlesh8 = (byte)CommonColor.Flesh8;
    public const byte CFlesh7 = (byte)CommonColor.Flesh7;
    public const byte CFlesh6 = (byte)CommonColor.Flesh6;
    public const byte CFlesh5 = (byte)CommonColor.Flesh5;
    public const byte CFlesh4 = (byte)CommonColor.Flesh4;
    public const byte CFlesh3 = (byte)CommonColor.Flesh3;
    public const byte CFlesh2 = (byte)CommonColor.Flesh2;
    public const byte CFlesh1 = (byte)CommonColor.Flesh1;
    public const byte CGrey1 = (byte)CommonColor.Grey1;
    public const byte CGrey2 = (byte)CommonColor.Grey2;
    public const byte CGrey3 = (byte)CommonColor.Grey3;
    public const byte CGrey4 = (byte)CommonColor.Grey4;
    public const byte CGrey5 = (byte)CommonColor.Grey5;
    public const byte CGrey6 = (byte)CommonColor.Grey6;
    public const byte CGrey7 = (byte)CommonColor.Grey7;
    public const byte CGrey8 = (byte)CommonColor.Grey8;
    public const byte CGrey9 = (byte)CommonColor.Grey9;
    public const byte CGrey10 = (byte)CommonColor.Grey10;
    public const byte CGrey11 = (byte)CommonColor.Grey11;
    public const byte CGrey12 = (byte)CommonColor.Grey12;
    public const byte CGrey13 = (byte)CommonColor.Grey13;
    public const byte CGrey14 = (byte)CommonColor.Grey14;
    public const byte CGrey15 = (byte)CommonColor.Grey15;
    public const byte CMidGrey = (byte)CommonColor.MidGrey;

    static readonly uint[] RawPalette = {
                 0,          0,          0, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 
        4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295, 4294967295,

        4278190080, 4278190080, 4294967295, 4292337639, 4290494403, 4288913307, 4287594359, 4285751127,
        4283645751, 4281802519, 4290218907, 4288635763, 4286792507, 4286542743, 4284697483, 4283377527,
        4279474151, 4278215619, 4278206375, 4278198143, 4278195039, 4280526759, 4281303927, 4280509251,
        4280764191, 4280756992, 4279965440, 4285785087, 4281582567, 4281049039, 4279466931, 4279456643,
        4290490231, 4287598403, 4285228835, 4282598163, 4289957691, 4288375587, 4286532367, 4283905792,
        4287609827, 4284979139, 4283135915, 4281555855, 4280238967, 4279185243, 4278655807, 4278194987,
        4278192919, 4278915871, 4279442207, 4280230695, 4280757035, 4281545523, 4282071867, 4282598211,
        4282862411, 4283126615, 4283915103, 4284441447, 4285230963, 4286020475, 4287335303, 4284699483
    };

    public static TextureBuilder<byte> T16(IAssetId? id) => TextureBuilder.Create<byte>(id, TileWidth, TileHeight);
    public static TextureBuilder<byte> T64(IAssetId? id) => TextureBuilder.Create<byte>(id, 64, 64);
    public static MultiFont? Font { get; }
    public static SingleFont Font6 { get; }
    public static SingleFont Font10 { get; }
    public static AlbionPalette PaletteCommon { get; }
    public static PaletteId Palette1Id { get; }
    public static AlbionPalette Palette1 { get; }
    public static TestTilemap Tileset1 { get; }
    public static TestLab Lab1 { get; }

    static Constants()
    {
        Font = MultiFont.Load(FontInfoPath, FontPngPath);
        Font6 = Font.GetFont(6);
        Font10 = Font.GetFont(10);
        Palette1Id = UAlbion.Base.Palette.Toronto2D;
        var commonRaw = new uint[256];
        var torontoRaw = new uint[256];
        Array.Copy(RawPalette, 192, commonRaw, 192, 64);
        Array.Copy(RawPalette, 0, torontoRaw, 0, 255);
        PaletteCommon = new AlbionPalette(((PaletteId)UAlbion.Base.Palette.Common).ToUInt32(), null, commonRaw);
        Palette1 = new AlbionPalette(Palette1Id.ToUInt32(), Palette1Id.ToString(), torontoRaw);
        Tileset1 = new TestTilemap(Font6, Font10);
        Lab1 = new TestLab(Font6, Font10);
    }

    public static void MajMin(int min, int maj, Action<int, int> func)
    {
        for (int j = 0; j < maj; j++)
        for (int i = 0; i < min; i++)
            func(i, j);
    }

    public static NpcWaypoint[] NpcPos(byte x, byte y) => new NpcWaypoint[] { new(x, y) };
    public static NpcWaypoint[] BuildPatrolPath(int x0, int y0)
    {
        var waypoints = new NpcWaypoint[MapNpc.WaypointCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            /* 7654
             * 8  3
             * 9012 */
            var (x, y) = (i % 8) switch
            {
                0 => (0, 0),
                1 => (1, 0),
                2 => (1, -1),
                3 => (1, -2),
                4 => (0, -2),
                5 => (-1, -2),
                6 => (-1, -1),
                _ => (-1, 0),
            };

            waypoints[i] = new NpcWaypoint((byte)(x0 + x), (byte)(y0 + y));
        }

        return waypoints;
    }
}
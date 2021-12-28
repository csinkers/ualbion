using System.Globalization;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using Xunit;

namespace UAlbion.Formats.Tests;

public enum TestTiles
{
    Basic = 1
}

public class MapTests
{
    static readonly AlbionPalette TestPalette = new(
        1, "TestPal", new[]
            {
                0u, // 0 Transparent
                0x000000ffu, // 1 Black
                0xff0000ffu, // 2 Red
                0x00ff00ffu, // 3 Green
                0x0000ffffu, // 4 Blue
                0xff00ffffu, // 5 Magenta
                0xffff00ffu, // 6 Yellow
                0x00ffffffu, // 7 Cyan
                0xffffffffu, // 8 White
            }.Concat(
                Enumerable
                    .Range(0, 256 - 9)
                    .Select(x => (uint)(255 * x / (float)(256 - 9)))
                    .Select(x => x << 24 | x << 16 | x << 8 | 0xff))
            .ToArray());

    static readonly byte[][] Tiles = 
    {
        new byte[] { 0 }, // 0 Blank
        new byte[] { 3 }, // 1 Land
        new byte[] { // 2 Ocean in SE
            3,3,
            3,4 },
        new byte[] { // 3 Ocean S
            3,3,
            4,4 },
        new byte[] { // 4 Ocean SW
            3,3,
            4,3 },
        new byte[] { // 5 Ocean E
            3,4,
            3,4 },
        new byte[] { 4 }, // 6 Ocean
        new byte[] { // 7 Ocean W
            4,3,
            4,3 },
        new byte[] { // 8 Ocean NE
            3,4,
            3,3 },
        new byte[] { // 9 Ocean N
            4,4,
            3,3 },
        new byte[] { // 10 Ocean NW
            4,3,
            3,3 },
        new byte[] { // 11 Land SE
            4,4,
            4,3 },
        new byte[] { // 12 Land SW
            4,4,
            3,4 },
        new byte[] { // 13 Land NE
            4,3,
            4,4 },
        new byte[] { // 14 Land NW
            3,4,
            4,4 },
    };

    static TileData T(string s)
    {
        // Single frame: #:T:L:C:Flags:Unk7
        // Multi frame: #+F:T:L:C:Flags:Unk7

        var parts = s.Split(':');
        var t = new TileData();
        if (parts.Length > 0)
        {
            int index = parts[0].IndexOf('+');
            t.ImageNumber = index == -1 
                ? ushort.Parse(parts[0])
                : ushort.Parse(parts[0].Substring(0, index));

            if (index != -1)
                t.FrameCount = byte.Parse(parts[0].Substring(index));
        }

        if (parts.Length > 1) t.Type = (TileType)byte.Parse(parts[1]);
        if (parts.Length > 2) t.Layer = (TileLayer)byte.Parse(parts[2]);
        if (parts.Length > 3) t.Collision = (Passability)byte.Parse(parts[3]);
        if (parts.Length > 4) t.Flags = (TileFlags)ushort.Parse(parts[4], NumberStyles.HexNumber);
        if (parts.Length > 5) t.Unk7 = byte.Parse(parts[5]);

        return t;
    }

    [Fact]
    public void RoundTrip2DTest()
    {
        AssetMapping.GlobalIsThreadLocal = true;
        var m = AssetMapping.Global;
        m.Clear();
        m.RegisterAssetType(typeof(TestTiles), AssetType.Tileset);
        var tileset = new TilesetData(AssetId.From(TestTiles.Basic))
        {
            UseSmallGraphics = false
        };

        tileset.Tiles.Add(T("0:0:0:8")); // Background / void
        tileset.Tiles.Add(T("1:0:0:0")); // Grass
        tileset.Tiles.Add(T("2:0:0:8")); // Water+Grass
        tileset.Tiles.Add(T("3:0:0:8"));
        tileset.Tiles.Add(T("4:0:0:8"));
        tileset.Tiles.Add(T("5:0:0:8"));
        tileset.Tiles.Add(T("6:0:0:8"));
        tileset.Tiles.Add(T("7:0:0:8"));
        tileset.Tiles.Add(T("8:0:0:8"));
        tileset.Tiles.Add(T("9:0:0:8"));
        tileset.Tiles.Add(T("10:0:0:8"));
        tileset.Tiles.Add(T("11:0:0:8"));
        tileset.Tiles.Add(T("12:0:0:8"));
        tileset.Tiles.Add(T("13:0:0:8"));
        tileset.Tiles.Add(T("14:0:0:8"));
    }
}
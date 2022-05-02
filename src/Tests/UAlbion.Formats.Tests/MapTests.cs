using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Exporters.Tiled;
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

        var raw = (TileFlags)int.Parse(parts[0], NumberStyles.HexNumber);
        return TileData.FromRaw(raw);
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

    [Fact]
    public void RoundTrip3DLayers()
    {
        int nextLayerId = 0;
        var map = new MapData3D(MapId.None, PaletteId.None, LabyrinthId.None, 16, 16, new List<EventNode>(), new List<ushort>(), new List<MapNpc>(), new List<MapEventZone>());
        for (int i = 0; i < 256; i++)
        {
            map.Floors[i] = (byte)i;
            map.Ceilings[i] = (byte)i;
            map.Contents[i] = (byte)i;
        }

        var layers = LayerMapping3D.BuildLayers(map, ref nextLayerId);
        var reloaded = new MapData3D(MapId.None, PaletteId.None, LabyrinthId.None, 16, 16, new List<EventNode>(), new List<ushort>(), new List<MapNpc>(), new List<MapEventZone>());
        LayerMapping3D.ReadLayers(reloaded, layers);

        Assert.True(map.Floors.SequenceEqual(reloaded.Floors));
        Assert.True(map.Ceilings.SequenceEqual(reloaded.Ceilings));
        Assert.True(map.Contents.SequenceEqual(reloaded.Contents));
    }
}
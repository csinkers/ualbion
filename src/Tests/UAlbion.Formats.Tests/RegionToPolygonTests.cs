using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Exporters;
using UAlbion.Formats.Exporters.Tiled;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using Xunit;

namespace UAlbion.Formats.Tests;

public class RegionToPolygonTests
{
    static IList<Geometry.Polygon> Convert(List<(byte x, byte y)> region)
    {
        byte w = (byte)(region.Max(x => x.Item1) + 1);
        byte h = (byte)(region.Max(x => x.Item2) + 1);

        var events = new List<EventNode> { new(0, new DoScriptEvent(new ScriptId(AssetType.Script))) };
        var map = new MapData2D(MapId.None, PaletteId.None, TilesetId.None,
            w, h,
            events,
            new ushort[] { 0 },
            Array.Empty<MapNpc>(),
            region.Select(p => new MapEventZone
            {
                X = p.x,
                Y = p.y,
                Chain = 0,
                Node = events[0],
                Trigger = TriggerTypes.Default
            }).ToArray()
        );

        var zones = TriggerZoneBuilder.BuildZones(map);
        return zones.Select(x => x.Item2).ToList();
    }

    [Fact]
    public void SingleTileTest()
    {
        var region = new List<(byte, byte)> { (0, 0) };
        var polygons = Convert(region);
        Assert.Collection(polygons,
            poly =>
            {
                Assert.Equal(0, poly.OffsetX);
                Assert.Equal(0, poly.OffsetY);
                Assert.Collection(poly.Points,
                    x => Assert.Equal((0, 0), x),
                    x => Assert.Equal((1, 0), x),
                    x => Assert.Equal((1, 1), x),
                    x => Assert.Equal((0, 1), x));
            });
    }

    [Fact]
    public void TwoTileTest()
    {
        var region = new List<(byte, byte)> { (0, 0), (1, 0) };
        var polygons = Convert(region);
        Assert.Collection(polygons,
            poly =>
            {
                Assert.Equal(0, poly.OffsetX);
                Assert.Equal(0, poly.OffsetY);
                Assert.Collection(poly.Points,
                    x => Assert.Equal((0, 0), x),
                    x => Assert.Equal((2, 0), x),
                    x => Assert.Equal((2, 1), x),
                    x => Assert.Equal((0, 1), x));
            });
    }

    [Fact]
    public void TwoByTwoTest()
    {
        var region = new List<(byte, byte)> { (0, 0), (1, 0), (0, 1), (1, 1) };
        var polygons = Convert(region);
        Assert.Collection(polygons,
            poly =>
            {
                Assert.Equal(0, poly.OffsetX);
                Assert.Equal(0, poly.OffsetY);
                Assert.Collection(poly.Points,
                    x => Assert.Equal((0, 0), x),
                    x => Assert.Equal((2, 0), x),
                    x => Assert.Equal((2, 2), x),
                    x => Assert.Equal((0, 2), x));
            });
    }

    [Fact]
    public void LShapeTest()
    {
        var region = new List<(byte, byte)>
        {
            (0, 0), (1, 0),
            (0, 1),
            (0, 2)
        };
        var polygons = Convert(region);
        Assert.Collection(polygons,
            poly =>
            {
                Assert.Equal(0, poly.OffsetX);
                Assert.Equal(0, poly.OffsetY);
                Assert.Collection(poly.Points,
                    x => Assert.Equal((0, 0), x),
                    x => Assert.Equal((2, 0), x),
                    x => Assert.Equal((2, 1), x),
                    x => Assert.Equal((1, 1), x),
                    x => Assert.Equal((1, 3), x),
                    x => Assert.Equal((0, 3), x));
            });
    }

    [Fact]
    public void DoughnutTest()
    {
        var region = new List<(byte, byte)>
        {
            (0,0), (1,0), (2,0),
            (0,1),        (2,1),
            (0,2), (1,2), (2,2),
        };
        var polygons = Convert(region);
        Assert.Collection(polygons,
            poly =>
            {
                Assert.Equal(0, poly.OffsetX);
                Assert.Equal(0, poly.OffsetY);
                Assert.Collection(poly.Points,
                    x => Assert.Equal((0, 0), x),
                    x => Assert.Equal((3, 0), x),
                    x => Assert.Equal((3, 2), x),
                    x => Assert.Equal((2, 2), x),
                    x => Assert.Equal((2, 1), x),
                    x => Assert.Equal((1, 1), x),
                    x => Assert.Equal((1, 2), x),
                    x => Assert.Equal((0, 2), x));
            },
            poly =>
            {
                Assert.Equal(0, poly.OffsetX);
                Assert.Equal(2, poly.OffsetY);
                Assert.Collection(poly.Points,
                    x => Assert.Equal((0, 0), x),
                    x => Assert.Equal((3, 0), x),
                    x => Assert.Equal((3, 1), x),
                    x => Assert.Equal((0, 1), x));
            }
        );
    }

    [Fact]
    public void DiagonallyTouchingTest()
    {
        var region = new List<(byte, byte)>
        {
            (0,0), (1,0),
            (2,1), (3,1),
        };
        var polygons = Convert(region);
        Assert.Collection(polygons,
            poly =>
            {
                Assert.Equal(0, poly.OffsetX);
                Assert.Equal(0, poly.OffsetY);
                Assert.Collection(poly.Points,
                    x => Assert.Equal((0, 0), x),
                    x => Assert.Equal((2, 0), x),
                    x => Assert.Equal((2, 1), x),
                    x => Assert.Equal((0, 1), x));
            },
            poly =>
            {
                Assert.Equal(2, poly.OffsetX);
                Assert.Equal(1, poly.OffsetY);
                Assert.Collection(poly.Points,
                    x => Assert.Equal((0, 0), x),
                    x => Assert.Equal((2, 0), x),
                    x => Assert.Equal((2, 1), x),
                    x => Assert.Equal((0, 1), x));
            });
    }


    static List<(byte x, byte y)> CrashSite1 => RegionFromString(@"
       #####
    ########
  ## #######
  ###### ##
  #########
  ########
### #####
# #   ##
###
").ToList();
    static List<(byte x, byte y)> CrashSite2 => new() { (1, 16) };

    [Fact]
    public void CrashSiteTest1()
    {
        RoundTrip(CrashSite1);
        RoundTrip(CrashSite2);
    }

    [Fact]
    public void CrashSiteTest2()
    {
        var outerKey = new ZoneKey(false, TriggerTypes.Examine | TriggerTypes.Normal, 4);
        var innerKey = new ZoneKey(false, TriggerTypes.Examine, 6);

        List<(ZoneKey key, IList<(int x, int y)> points)> regions = new()
        {
            (outerKey, ToInts(CrashSite1)),
            (innerKey, ToInts(CrashSite2))
        };

        var preRegions = regions.Select(x => TriggerZoneBuilder.PrintRegion(x.points)).ToList();
        TriggerZoneBuilder.RemoveVoids(regions);
        var postRegions = regions.Select(x => TriggerZoneBuilder.PrintRegion(x.points)).ToList();

        Assert.Equal(4, regions.Count);
        var outerRegions = regions.Where(x => x.key == outerKey);
        var reconstructed = (from region in outerRegions from point in region.points select ((byte)point.x, (byte)point.y)).ToList();

        var outer = CrashSite1;
        outer.Sort();
        reconstructed.Sort();
        Assert.True(outer.SequenceEqual(reconstructed));
    }

    static IList<(int x, int y)> ToInts(List<(byte x, byte y)> list) => list.Select(x => ((int)x.x, (int)x.y)).ToList();

    static void RoundTrip(List<(byte, byte)> region)
    {
        region.Sort();
        var roundTripped = new List<(byte, byte)>();
        var polygons = Convert(region);
        foreach (var polygon in polygons)
        {
            var points = TriggerZoneBuilder.GetPointsInsidePolygon(polygon.Points);
            foreach(var point in points)
                roundTripped.Add(((byte)(point.x + polygon.OffsetX), (byte)(point.y + polygon.OffsetY)));
        }

        roundTripped.Sort();
        Assert.True(region.SequenceEqual(roundTripped));
    }

    static IEnumerable<(byte, byte)> RegionFromString(string s)
    {
        var lines = ApiUtil.SplitLines(s, StringSplitOptions.None);
        for (int j = 0; j < lines.Length; j++)
        {
            var line = lines[j];
            for (int i = 0; i < line.Length; i++)
                if (line[i] != ' ')
                    yield return ((byte)i, (byte)j);
        }
    }
}

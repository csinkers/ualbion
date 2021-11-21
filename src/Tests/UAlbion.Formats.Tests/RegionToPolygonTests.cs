using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Exporters;
using UAlbion.Formats.MapEvents;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class RegionToPolygonTests
    {
        static IList<Geometry.Polygon> Convert(List<(byte x, byte y)> region)
        {
            byte w = (byte)(region.Max(x => x.Item1) + 1);
            byte h = (byte)(region.Max(x => x.Item2) + 1);

            var events = new List<EventNode> { new(0, new DoScriptEvent(new ScriptId(AssetType.Script))) };
            var map = new MapData2D(MapId.None, w, h,
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
                        x => Assert.Equal((2, 0), x),
                        x => Assert.Equal((2, 1), x),
                        x => Assert.Equal((1, 1), x),
                        x => Assert.Equal((1, 2), x),
                        x => Assert.Equal((2, 2), x),
                        x => Assert.Equal((2, 3), x),
                        x => Assert.Equal((0, 3), x));
                },
                poly =>
                {
                    Assert.Equal(2, poly.OffsetX);
                    Assert.Equal(0, poly.OffsetY);
                    Assert.Collection(poly.Points,
                        x => Assert.Equal((0, 0), x),
                        x => Assert.Equal((1, 0), x),
                        x => Assert.Equal((1, 3), x),
                        x => Assert.Equal((0, 3), x));
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
    }
}

using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.Exporters;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class EdgesToPolygonTests
    {
        static Edge E(byte x1, byte y1, byte x2, byte y2) => new Edge(x1, y1, x2, y2);
        static IList<Geometry.Polygon> Convert(IEnumerable<Edge> edges) => 
            TriggerZoneBuilder.BuildPolygonsFromSortedEdges(
                edges.OrderBy(x => x).ToList());

        [Fact]
        public void SingleTileTest()
        {
            var edges = new[]
            {
                E(0,0,1,0),
                E(1,0,1,1),
                E(1,1,0,1),
                E(0,1,0,0),
            };
            var polygons = Convert(edges);
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
            var edges = new[]
            {
                E(0,0,2,0),
                E(2,0,2,1),
                E(2,1,0,1),
                E(0,1,0,0),
            };
            var polygons = Convert(edges);
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
            var edges = new[]
            {
                E(0,0,2,0),
                E(2,0,2,2),
                E(2,2,0,2),
                E(0,2,0,0),
            };
            var polygons = Convert(edges);
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
            var edges = new List<Edge>
            {
                E(0,0,2,0),
                E(2,0,2,1),
                E(2,1,1,1),
                E(1,1,1,3),
                E(1,3,0,3),
                E(0,3,0,0),
            };
            var polygons = Convert(edges);
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
/*
        [Fact]
        public void DoughnutTest()
        {
            var edges = new List<Edge>
            {
                E(0,0,3,0), // Outer edge
                E(3,0,3,3),
                E(3,3,0,3),
                E(0,3,0,0),

                E(1,1,2,1), // Inner edge
                E(2,1,2,2),
                E(2,2,1,2),
                E(1,2,1,1)
            };
            var polygons = Convert(edges);
            Assert.Collection(polygons,
                poly =>
                {
                    Assert.Equal(0, poly.OffsetX);
                    Assert.Equal(0, poly.OffsetY);
                    Assert.Collection(poly.Points,
                        x => Assert.Equal((0, 0), x),
                        x => Assert.Equal((3, 0), x),
                        x => Assert.Equal((3, 3), x),
                        x => Assert.Equal((0, 3), x),
                        x => Assert.Equal((0, 2), x),
                        x => Assert.Equal((2, 2), x),
                        x => Assert.Equal((2, 1), x),
                        x => Assert.Equal((0, 1), x));
                },
                poly =>
                {
                    Assert.Equal(0, poly.OffsetX);
                    Assert.Equal(0, poly.OffsetY);
                    Assert.Collection(poly.Points,
                        x => Assert.Equal((0, 1), x),
                        x => Assert.Equal((1, 1), x),
                        x => Assert.Equal((1, 2), x),
                        x => Assert.Equal((0, 2), x));
                }
            );
        }

        [Fact]
        public void DiagonallyTouchingTest()
        {
            var edges = new List<Edge>
            {
                E(0,0,2,0), // ----
                E(2,0,2,1), //     |
                E(0,1,0,0), // |
                E(2,1,0,1), // ----

                E(2,1,4,1), //      ----
                E(4,1,4,2), //          |
                E(2,2,2,1), //      |
                E(4,2,2,2), //      ----
            };
            var polygons = Convert(edges);
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
                    Assert.Equal(0, poly.OffsetX);
                    Assert.Equal(0, poly.OffsetY);
                    Assert.Collection(poly.Points,
                        x => Assert.Equal((2, 1), x),
                        x => Assert.Equal((4, 1), x),
                        x => Assert.Equal((4, 2), x),
                        x => Assert.Equal((2, 2), x));
                });
        }
*/
    }
}
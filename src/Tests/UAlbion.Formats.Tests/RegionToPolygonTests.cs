using System.Collections.Generic;
using Xunit;

namespace UAlbion.Formats.Tests
{
    #if false
    public class RegionToPolygonTests
    {
        [Fact]
        public void SingleTileTest()
        {
            var region = new List<(int, int)> { (0, 0) };
            var polygons = GeometryHelper.RegionToPolygons(region);
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
            var region = new List<(int, int)> { (0, 0), (1, 0) };
            var polygons = GeometryHelper.RegionToPolygons(region);
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
            var region = new List<(int, int)> { (0, 0), (1, 0), (0, 1), (1, 1) };
            var polygons = GeometryHelper.RegionToPolygons(region);
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
            var region = new List<(int, int)>
            {
                (0, 0), (1, 0), 
                (0, 1), 
                (0, 2)
            };
            var polygons = GeometryHelper.RegionToPolygons(region);
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
            var region = new List<(int, int)>
            {
                (0,0), (1,0), (2,0),
                (0,1),        (2,1),
                (0,2), (1,2), (2,2),
            };
            var polygons = GeometryHelper.RegionToPolygons(region);
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
            var region = new List<(int, int)>
            {
                (0,0), (1,0),
                              (2,1), (3,1),
            };
            var polygons = GeometryHelper.RegionToPolygons(region);
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
    }
    #endif
}

using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.Exporters;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class EdgeMergingTests
    {
        static Edge E(ushort x1, ushort y1, ushort x2, ushort y2) => new Edge(x1, y1, x2, y2);

        [Fact]
        public void TwoHorizontalTest()
        {
            IList<Edge> edges = new[]
            {
                E(0, 0, 1, 0),
                E(1, 0, 2, 0),
            };
            edges = TriggerZoneBuilder.MergeEdges(edges);
            Assert.Collection(edges, e => Assert.Equal(E(0, 0, 2, 0), e));

            edges = new[]
            {
                E(1, 0, 2, 0),
                E(1, 0, 0, 0),
            };
            edges = TriggerZoneBuilder.MergeEdges(edges);
            Assert.Collection(edges, e => Assert.Equal(E(0, 0, 2, 0), e));

            edges = new[]
            {
                E(0, 0, 1, 0),
                E(2, 0, 3, 0),
            };
            edges = TriggerZoneBuilder.MergeEdges(edges);
            Assert.Collection(edges,
                e => Assert.Equal(E(0, 0, 1, 0), e),
                e => Assert.Equal(E(2, 0, 3, 0), e)
                );
        }

        [Fact]
        public void TwoVerticalTest()
        {
            IList<Edge> edges = new[]
            {
                E(0, 0, 0, 1),
                E(0, 1, 0, 2),
            };
            edges = TriggerZoneBuilder.MergeEdges(edges);
            Assert.Collection(edges,
                e => Assert.Equal(E(0, 0, 0, 2), e)
            );
        }

        [Fact]
        public void CornerTest()
        {
            IList<Edge> edges = new[]
            {
                E(0, 0, 1, 0),
                E(1, 0, 1, 1),
            };
            edges = TriggerZoneBuilder.MergeEdges(edges);
            Assert.Collection(edges,
                e => Assert.Equal(E(0, 0, 1, 0), e),
                e => Assert.Equal(E(1, 0, 1, 1), e)
            );

            edges = new[]
            {
                E(1, 0, 1, 1),
                E(1, 1, 0, 1),
            };
            edges = TriggerZoneBuilder.MergeEdges(edges);
            Assert.Collection(edges,
                e => Assert.Equal(E(1, 0, 1, 1), e),
                e => Assert.Equal(E(1, 1, 0, 1), e)
            );

            edges = new[]
            {
                E(1, 1, 0, 1),
                E(0, 1, 0, 0),
            };
            edges = TriggerZoneBuilder.MergeEdges(edges);
            Assert.Collection(edges,
                e => Assert.Equal(E(0, 1, 0, 0), e),
                e => Assert.Equal(E(1, 1, 0, 1), e)
            );

            edges = new[]
            {
                E(0, 1, 0, 0),
                E(0, 0, 1, 0),
            };
            edges = TriggerZoneBuilder.MergeEdges(edges);
            Assert.Collection(edges,
                e => Assert.Equal(E(0, 0, 1, 0), e),
                e => Assert.Equal(E(0, 1, 0, 0), e)
            );
        }

        [Fact]
        public void BoxTest()
        {
            IList<Edge> edges = new[]
            {
                E(0, 0, 1, 0),
                E(1, 0, 2, 0),
                E(2, 0, 2, 1),
                E(2, 1, 2, 2),
                E(2, 2, 1, 2),
                E(1, 2, 0, 2),
                E(0, 2, 0, 1),
                E(0, 1, 0, 0),
            };
            edges = TriggerZoneBuilder.MergeEdges(edges);
            // edges = edges.OrderBy(x => x).ToList();
            Assert.Collection(edges,
                e => Assert.Equal(E(0, 0, 2, 0), e),
                e => Assert.Equal(E(0, 0, 0, 2), e),
                e => Assert.Equal(E(2, 0, 2, 2), e),
                e => Assert.Equal(E(0, 2, 2, 2), e)
            );
        }

        [Fact]
        public void EdgeSortTest()
        {
            IList<Edge> edges = new[]
            {
                E(0, 0, 0, 0),
                E(0, 0, 0, 1),
                E(0, 0, 1, 0),
                E(0, 0, 1, 1),
                E(0, 1, 0, 0),
                E(0, 1, 0, 1),
                E(0, 1, 1, 0),
                E(0, 1, 1, 1),
                E(1, 0, 0, 0),
                E(1, 0, 0, 1),
                E(1, 0, 1, 0),
                E(1, 0, 1, 1),
                E(1, 1, 0, 0),
                E(1, 1, 0, 1),
                E(1, 1, 1, 0),
                E(1, 1, 1, 1),
            };
            var r = new Random();
            edges = edges.Distinct().ToList();
            Assert.Equal(10, edges.Count);
            edges = edges.OrderBy(x => r.Next()).ToList(); // Shuffle
            edges = edges.OrderBy(x => x).ToList();
            Assert.Collection(edges,
                e => Assert.Equal(E(0, 0, 0, 0), e),
                e => Assert.Equal(E(0, 0, 1, 0), e),
                e => Assert.Equal(E(1, 0, 1, 0), e),
                e => Assert.Equal(E(0, 0, 0, 1), e),
                e => Assert.Equal(E(0, 0, 1, 1), e),
                e => Assert.Equal(E(1, 0, 0, 1), e),
                e => Assert.Equal(E(1, 0, 1, 1), e),
                e => Assert.Equal(E(0, 1, 0, 1), e),
                e => Assert.Equal(E(0, 1, 1, 1), e),
                e => Assert.Equal(E(1, 1, 1, 1), e)
            );
        }

        [Fact]
        public void EdgePackingTest()
        {
            IList<Edge> edges = new[]
            {
                E(0, 0, 0, 0),
                E(0, 1, 2, 3),
                E(3, 2, 1, 0),
                E(255, 255, 255, 255),
                E(256, 256, 256, 256),
                E(0xffff,0xffff,0xffff,0xffff),
            };

            Assert.Equal((0,0,0,0), edges[0].Tuple);
            Assert.Equal((0,1,2,3), edges[1].Tuple);
            Assert.Equal((1,0,3,2), edges[2].Tuple);
            Assert.Equal((255,255,255,255), edges[3].Tuple);
            Assert.Equal((256,256,256,256), edges[4].Tuple);
            Assert.Equal((0xffff,0xffff,0xffff,0xffff), edges[5].Tuple);

            Assert.Equal(0, edges[1].X1);
            Assert.Equal(1, edges[1].Y1);
            Assert.Equal(2, edges[1].X2);
            Assert.Equal(3, edges[1].Y2);

            Assert.Equal(0u, edges[0].Packed);
            Assert.Equal(0u, edges[0].ColumnMajorPacked);
            Assert.Equal(0x00ff00ff_00ff00ffUL, edges[3].Packed);
            Assert.Equal(0x01000100_01000100UL, edges[4].Packed);
            Assert.Equal(ulong.MaxValue, edges[5].Packed);
            Assert.Equal(ulong.MaxValue, edges[5].ColumnMajorPacked);
        }
    }
}
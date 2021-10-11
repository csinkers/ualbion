using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.Scripting;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class ControlFlowTests
    {
        static void Verify(IEnumerable<int> results, params int[] expected)
        {
            Assert.NotNull(results);
            Assert.Collection(results,
                expected.Select(x => (Action<int>)(
                    y => Assert.Equal(x, y))
                ).ToArray());
        }

        static void Verify(ControlFlowGraph expected, ControlFlowGraph result)
        {
            expected = expected.Canonicalize();
            result = result.Canonicalize();
            Assert.Equal(expected.Nodes.Count, result.Nodes.Count);
            Assert.Equal(expected.Edges.Count(), result.Edges.Count());

            for (int i = 0; i < expected.Nodes.Count; i++)
                Assert.Equal(expected.Nodes[i].ToString(), result.Nodes[i].ToString());

            var i1 = expected.Edges.GetEnumerator();
            var i2 = result.Edges.GetEnumerator();
            while (i1.MoveNext())
            {
                i2.MoveNext();
                Assert.Equal(i1.Current.Item1, i2.Current.Item1);
                Assert.Equal(i1.Current.Item2, i2.Current.Item2);
            }
        }

        [Fact]
        public void OrderTest()
        {
            var graph = TestGraphs.Graph1;
            var order = graph.GetDfsOrder();
            Assert.Collection(order,
                x => Assert.Equal(0, x),
                x => Assert.Equal(1, x),
                x => Assert.Equal(2, x),
                x => Assert.Equal(3, x),
                x => Assert.Equal(4, x),
                x => Assert.Equal(5, x));
        }

        [Fact]
        public void PostOrderTest()
        { 
            var graph = TestGraphs.Graph2;
            var postOrder = graph.GetDfsPostOrder();
            Verify(postOrder, 5, 7, 6, 3, 2, 4, 1, 0);
        }

        [Fact]
        public void ReverseTest()
        {
            var reversed = TestGraphs.NoMoreGotos3.Reverse().Canonicalize();
            var expected = TestGraphs.NoMoreGotos3Reversed.Canonicalize();
            Verify(expected, reversed);
        }

        [Fact]
        public void IsCyclicTest()
        {
            Assert.False(TestGraphs.Sequence.IsCyclic());
            Assert.False(TestGraphs.IfThen.IsCyclic());
            Assert.False(TestGraphs.IfThenElse.IsCyclic());
            Assert.True(TestGraphs.WhileLoop.IsCyclic());
            Assert.True(TestGraphs.DoWhileLoop.IsCyclic());
            Assert.True(TestGraphs.Graph1.IsCyclic());
            Assert.True(TestGraphs.Graph2.IsCyclic());
            Assert.True(TestGraphs.NoMoreGotos3.IsCyclic());
        }

        [Fact]
        public void GetComponentTest()
        {
            var components =
                TestGraphs.NoMoreGotos3
                .GetAllStronglyConnectedComponents()
                .Where(x => x.Count > 1)
                .OrderBy(x => x.Sum())
                .ToList();

            foreach (var component in components)
                component.Sort();

            Assert.NotNull(components);
            Assert.Collection(components,
                x => Verify(x, 3, 4, 5, 9, 11),
                x => Verify(x, 6, 7, 8, 16)
            );
        }

        [Fact]
        public void GetCyclesTest()
        {
            var components =
                TestGraphs.NoMoreGotos3
                .GetAllStronglyConnectedComponents()
                .Where(x => x.Count > 1)
                .OrderBy(x => x.Sum())
                .ToList();

            // (3, 9), (3,4,11,5)
            var cycles =
                TestGraphs.NoMoreGotos3
                    .GetAllSimpleCyclePaths(components[0])
                    .OrderBy(x => x.Sum());

            Assert.NotNull(cycles);
            Assert.Collection(cycles,
                x => Verify(x, 3, 9),
                x => Verify(x, 3, 4, 11, 5)
            );

            // (6,7,16), (6, 8, 16)
            cycles =
                TestGraphs.NoMoreGotos3
                    .GetAllSimpleCyclePaths(components[1])
                    .OrderBy(x => x.Sum());
            Assert.NotNull(cycles);
            Assert.Collection(cycles,
                x => Verify(x, 6, 7, 16),
                x => Verify(x, 6, 8, 16)
            );
        }
    }
}

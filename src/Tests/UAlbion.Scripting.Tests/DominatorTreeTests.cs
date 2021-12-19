using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace UAlbion.Scripting.Tests
{
    public class DominatorTreeTests
    {
        static GenericTreeNode<int> N(int n) => new(n);
        static void Verify(IEnumerable<int> results, params int[] expected)
        {
            Assert.NotNull(results);
            Assert.Collection(results,
                expected.Select(x => (Action<int>)(
                    y => Assert.Equal(x, y))
                ).ToArray());
        }

        [Fact]
        public void AddPathTest()
        {
            var tree = DominatorTree.Empty;
            tree = tree.AddPath(0, 3, 5, 6);
            Verify(tree.Values, 0, 3, 5, 6);

            tree = tree.AddPath(0, 3, 4, 1);
            Verify(tree.Values, 0, 3, 5, 6, 4, 1);

            tree = tree.AddPath(0, 2, 7);
            Verify(tree.Values, 0, 3, 5, 6, 4, 1, 2, 7);

            tree = tree.AddPath(0, 3, 5, 6, 8);
            Verify(tree.Values, 0, 3, 5, 6, 8, 4, 1, 2, 7);
        }

        [Fact]
        public void AddChildTest()
        {
            var tree = N(0);
            Verify(tree.Values, 0);

            var tree2 = tree.AddChild(N(1));
            Verify(tree.Values, 0);
            Verify(tree2.Values, 0, 1);

            tree2 = tree2.AddChild(N(2));
            Verify(tree2.Values, 0, 1, 2);
        }

        [Fact]
        public void RemoveChildTest()
        {
            var node = N(0).AddChild(N(1)).AddChild(N(2));
            Verify(node.Values, 0, 1, 2);

            var tree2 = node.RemoveChild(node.Children[0]);
            Verify(node.Values, 0, 1, 2);
            Verify(tree2.Values, 0, 2);
        }

        [Fact]
        public void NmgFig3DominatorTest()
        {
            /*
            +--A      =  0
            |+--b1    =  1
            ||+--n4   = 12
            ||+--n5   = 13
            ||+--b2   =  2
            |||+--n6  = 14
            ||+--n7   = 15
            |||+--d1  =  6
            ||||+--d2 =  7
            ||||+--d3 =  8
            ||||+--n8 = 16
            |+--c1    =  3
            ||+--c2   =  4
            |||+--n2  = 10
            |||+--n3  = 11
            ||||+--c3 =  5
            ||+--n1   =  9
            |+n9      = 17
            */

            var tree = DominatorTree.Empty;
            tree = tree.AddPath(0, 1, 12);
            tree = tree.AddPath(0, 1, 13);
            tree = tree.AddPath(0, 1, 2, 14);
            tree = tree.AddPath(0, 1, 15, 6, 7);
            tree = tree.AddPath(0, 1, 15, 6, 8);
            tree = tree.AddPath(0, 1, 15, 6, 16);
            tree = tree.AddPath(0, 3, 4, 10);
            tree = tree.AddPath(0, 3, 4, 11, 5);
            tree = tree.AddPath(0, 3, 9);
            tree = tree.AddPath(0, 17);

            var graph = TestGraphs.NoMoreGotos3;
            var calculated = graph.GetDominatorTree();
            for (int i = 0; i < 18; i++)
                for (int j = 0; j < 18; j++)
                    Assert.Equal(tree.Dominates(i, j), calculated.Dominates(i, j));
        }

        [Fact]
        public void NmgFig3PostDominatorTest()
        {
            /*
            +--n9     = 17
            |+--d2    =  7
            |+--d3    =  8
            |+--d1    =  6
            ||+--n8   = 16
            ||+--n7   = 15
            |||+--n5  = 13
            ||||+--n4 = 12
            |||+--n6  = 14
            |||+--b2  =  2
            |||+--b1  =  1
            |+--c3    =  5
            ||+--n3   = 11
            |+--n2    = 10
            |+--c2    =  4
            ||+--c1   =  3
            |||+--n1  =  9
            |+--A     =  0
            */
            var tree = DominatorTree.Empty;
            tree = tree.AddPath(17, 7);
            tree = tree.AddPath(17, 8);
            tree = tree.AddPath(17, 6, 16);
            tree = tree.AddPath(17, 6, 15, 13, 12);
            tree = tree.AddPath(17, 6, 15, 14);
            tree = tree.AddPath(17, 6, 15, 2);
            tree = tree.AddPath(17, 6, 15, 1);
            tree = tree.AddPath(17, 5, 11);
            tree = tree.AddPath(17, 10);
            tree = tree.AddPath(17, 4, 3, 9);
            tree = tree.AddPath(17, 0);

            var graph = TestGraphs.NoMoreGotos3;
            var calculated = graph.GetPostDominatorTree();
            for (int i = 0; i < 18; i++)
                for (int j = 0; j < 18; j++)
                    Assert.Equal(tree.Dominates(i, j), calculated.Dominates(i, j));
        }

        [Fact]
        public void ImmediateDominatorTest()
        {
            var tree = DominatorTree.Empty;
            tree = tree.AddPath(13, 5, 4).AddPath(13, 1, 0).AddPath(13, 2);

            Assert.Equal(13, tree.ImmediateDominator(5));
            Assert.Equal(5, tree.ImmediateDominator(4));
            Assert.Equal(13, tree.ImmediateDominator(1));
            Assert.Equal(1, tree.ImmediateDominator(0));
            Assert.Equal(13, tree.ImmediateDominator(2));
            Assert.Equal((int?)null, tree.ImmediateDominator(13));
        }

        [Fact]
        public void ImmediateDominatorTest2()
        {
            var tree = DominatorTree.Empty
                .AddPath(11, 9, 8, 12, 7, 6, 3, 2, 13, 4)
                .AddPath(11, 9, 8, 12, 10)
                .AddPath(11, 9, 8, 12, 7, 1, 0)
                .AddPath(11, 9, 8, 12, 7, 5);

            Assert.Equal(1, tree.ImmediateDominator(0));
            Assert.Equal(7, tree.ImmediateDominator(1));
            Assert.Equal(3, tree.ImmediateDominator(2));
            Assert.Equal(6, tree.ImmediateDominator(3));
            Assert.Equal(13, tree.ImmediateDominator(4));
            Assert.Equal(7, tree.ImmediateDominator(5));
            Assert.Equal(7, tree.ImmediateDominator(6));
            Assert.Equal(12, tree.ImmediateDominator(7));
            Assert.Equal(9, tree.ImmediateDominator(8));
            Assert.Equal(11, tree.ImmediateDominator(9));
            Assert.Equal(12, tree.ImmediateDominator(10));
            Assert.Equal(8, tree.ImmediateDominator(12));
            Assert.Equal(2, tree.ImmediateDominator(13));
            Assert.Equal((int?)null, tree.ImmediateDominator(11));
        }
    }
}

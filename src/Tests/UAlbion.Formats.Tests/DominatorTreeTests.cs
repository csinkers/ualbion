using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.Scripting;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class DominatorTreeTests
    {
        static TreeNode<int> N(int n) => new(n);
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
            var tree = TreeNode<int>.AddPath(null, new List<int>
            {
                0, 3, 5, 6
            }, (x, y) => x == y);
            Verify(tree.Values, 0, 3, 5, 6);

            tree = TreeNode<int>.AddPath(tree, new List<int>
            {
                0, 3, 4, 1
            }, (x, y) => x == y);
            Verify(tree.Values, 0, 3, 5, 6, 4, 1);

            tree = TreeNode<int>.AddPath(tree, new List<int>
            {
                0, 2, 7
            }, (x, y) => x == y);
            Verify(tree.Values, 0, 3, 5, 6, 4, 1, 2, 7);

            tree = TreeNode<int>.AddPath(tree, new List<int>
            {
                0, 3, 5, 6, 8
            }, (x, y) => x == y);
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
            var tree = N(0).AddChild(N(1)).AddChild(N(2));
            Verify(tree.Values, 0, 1, 2);

            var tree2 = tree.RemoveChild(tree.Children[0]);
            Verify(tree.Values, 0, 1, 2);
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

            TreeNode<int> tree = null;
            void Add(params int[] path) => tree = TreeNode<int>.AddPath(tree, path, (x, y) => x == y);
            Add(0, 1, 12);
            Add(0, 1, 13);
            Add(0, 1, 2, 14);
            Add(0, 1, 15, 6, 7);
            Add(0, 1, 15, 6, 8);
            Add(0, 1, 15, 6, 16);
            Add(0, 3, 4, 10);
            Add(0, 3, 4, 11, 5);
            Add(0, 3, 9);
            Add(0, 17);

            static bool eq(int x, int y) => x == y;
            var graph = TestGraphs.NoMoreGotos3;
            var calculated = graph.GetDominatorTree();
            for (int i = 0; i < 18; i++)
                for (int j = 0; j < 18; j++)
                    Assert.Equal(tree.Dominates(i, j, eq), calculated.Dominates(i, j, eq));
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
            TreeNode<int> tree = null;
            void Add(params int[] path) => tree = TreeNode<int>.AddPath(tree, path, (x, y) => x == y);
            Add(17, 7);
            Add(17, 8);
            Add(17, 6, 16);
            Add(17, 6, 15, 13, 12);
            Add(17, 6, 15, 14);
            Add(17, 6, 15, 2);
            Add(17, 6, 15, 1);
            Add(17, 5, 11);
            Add(17, 10);
            Add(17, 4, 3, 9);
            Add(17, 0);

            static bool eq(int x, int y) => x == y;
            var graph = TestGraphs.NoMoreGotos3;
            var calculated = graph.GetPostDominatorTree();
            for (int i = 0; i < 18; i++)
                for (int j = 0; j < 18; j++)
                    Assert.Equal(tree.Dominates(i, j, eq), calculated.Dominates(i, j, eq));
        }
    }
}
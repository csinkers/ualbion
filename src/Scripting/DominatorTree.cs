using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Scripting
{
    public class DominatorTree
    {
        public static DominatorTree Empty { get; } = new(null);
        static bool NodeEquality(int a, int b) => a == b;

        GenericTreeNode<int> Root { get; }
        DominatorTree(GenericTreeNode<int> root) => Root = root;

        public bool StrictlyDominates(int a, int b) => Root != null && Dominates(a, b);
        public bool Dominates(int a, int b)
        {
            if (Root == null)
                return false;

            GenericTreeNode<int> node = Root;
            if (node.Value != a)
                node = node.FindChild(a, NodeEquality, true);

            return node.FindChild(b, NodeEquality, true) != null;
        }

        public DominatorTree AddPath(IList<int> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (path.Count == 0)
                return this;

            var root = Root ?? new GenericTreeNode<int>(path[0]);

            if (root.Value != path[0])
                throw new ArgumentException($"Path begins with \"{path[0]}\", but the tree is rooted at \"{root.Value}\"");

            return new DominatorTree(root.AddPath(path, 1, NodeEquality));
        }

        public IEnumerable<int> Values => Root?.Values ?? Enumerable.Empty<int>();
    }
}
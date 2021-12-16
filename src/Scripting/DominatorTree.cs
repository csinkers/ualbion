using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public int? ImmediateDominator(int index)
        {
            if (Root.Value == index)
                return null; // Supplied index dominates all other nodes

            var path = Root.FindPath(index, NodeEquality);
            return path.Count > 1
                ? path[^1]
                : path[0];
        }

        public string ExportToDot(int dpi = 150)
        {
            var sb = new StringBuilder();
            ExportToDot(sb, dpi);
            return sb.ToString();
        }

        public void ExportToDot(StringBuilder sb, int dpi = 150)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.AppendLine("digraph G {");
            sb.AppendLine($"    graph [ dpi = {dpi} ];");
            var stack = new Stack<GenericTreeNode<int>>();
            stack.Push(Root);
            while (stack.TryPop(out var node))
            {
                foreach (var child in node.Children)
                {
                    sb.AppendLine($"    {node.Value} -> {child.Value};");
                    stack.Push(child);
                }
            }
            sb.AppendLine("}");
        }
    }
}
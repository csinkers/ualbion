using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Formats.Scripting
{
    public class DominatorTree : DominatorTree<int>
    {
        public static DominatorTree<int> Empty { get; } = new DominatorTree();
        DominatorTree() : base(null, (x,y) => x == y) { }
    }

    public class DominatorTree<T>
    {
        readonly DominatorTreeNode<T> _root;
        readonly Func<T, T, bool> _equalityFunc;

        protected DominatorTree(DominatorTreeNode<T> root, Func<T, T, bool> equalityFunc)
        {
            _root = root;
            _equalityFunc = equalityFunc ?? throw new ArgumentNullException(nameof(equalityFunc));
        }

        DominatorTreeNode<T> AddPathInner(DominatorTreeNode<T> node, IList<T> path, int pathOffset)
        {
            if (path.Count <= pathOffset)
                return node;

            DominatorTreeNode<T> child = node.FindChild(path[pathOffset], _equalityFunc);

            return child == null 
                ? node.AddChild(AddPathInner(new DominatorTreeNode<T>(path[pathOffset]), path, pathOffset + 1)) 
                : node.ReplaceChild(child, AddPathInner(child, path, pathOffset + 1));
        }

        public DominatorTree<T> AddPath(IList<T> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (path.Count == 0)
                return this;

            var root = _root ?? new DominatorTreeNode<T>(path[0]);

            if (!_equalityFunc(root.Value, path[0]))
                throw new ArgumentException($"Path begins with \"{path[0]}\", but the tree is rooted at \"{root.Value}\"");

            return new DominatorTree<T>(AddPathInner(root, path, 1), _equalityFunc);
        }

        public bool Dominates(T a, T b) => _equalityFunc(a, b) || StrictlyDominates(a, b);
        public bool StrictlyDominates(T a, T b) => _root != null && _root.Dominates(a, b, _equalityFunc);
        public IEnumerable<T> Values => _root?.Values ?? Enumerable.Empty<T>();
    }
}
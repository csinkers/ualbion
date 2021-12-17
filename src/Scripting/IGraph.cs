using System.Collections.Generic;

namespace UAlbion.Scripting
{
    public interface IGraph
    {
        int NodeCount { get; }
        int ActiveNodeCount { get; }
        bool IsNodeActive(int index);
        IEnumerable<(int start, int end)> Edges { get; }
        IEnumerable<(int, int)> GetBackEdges();
        IList<int> Children(int i);
        IList<int> Parents(int i);
        IList<int> GetDfsOrder();
        IList<int> GetDfsPostOrder();
        DominatorTree GetDominatorTree();
        DominatorTree GetPostDominatorTree();
        string ExportToDot(bool showContent = true, int dpi = 180);
        IGraph Defragment();
        IGraph Reverse();
    }

    public interface IGraph<out TNode, out TLabel> : IGraph
    {
        TNode GetNode(int i);
        TLabel GetEdgeLabel(int start, int end);
    }
}
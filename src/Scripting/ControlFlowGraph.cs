using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UAlbion.Api.Eventing;
using UAlbion.Scripting.Ast;

#pragma warning disable 8321 // Stop warnings about Vis() debug functions
namespace UAlbion.Scripting;

public delegate ControlFlowGraph RecordFunc(string description, ControlFlowGraph graph);

public class ControlFlowGraph : IGraph<ICfgNode, CfgEdge>
{
    const double DefragThreshold = 0.5; // If the majority are deleted then defrag
    static readonly ImmutableDictionary<int, ImmutableArray<int>> EmptyEdges = ImmutableDictionary<int, ImmutableArray<int>>.Empty;
    readonly ImmutableDictionary<int, ImmutableArray<int>> _edgesByStart;
    readonly ImmutableDictionary<int, ImmutableArray<int>> _edgesByEnd;
    readonly ImmutableDictionary<(int start, int end), CfgEdge> _labels; // N.B. Only includes non-true labels
    readonly ImmutableStack<int> _deletedNodes;
    readonly int _deletedNodeCount;

    // Memoised results
    ControlFlowGraph _cachedReverse;
    ImmutableArray<int>? _cachedOrder;
    ImmutableArray<int>? _cachedPostOrder;
    ImmutableArray<(int start, int end)>? _cachedBackEdges;

    public int EntryIndex { get; }
    public int ExitIndex { get; }
    public ImmutableList<ICfgNode> Nodes { get; }
    public ICfgNode GetNode(int i) => Nodes[i];
    public ICfgNode Entry => EntryIndex == -1 ? null : Nodes[EntryIndex];

    public IEnumerable<(int start, int end)> Edges =>
        from kvp in _edgesByStart
        from end in kvp.Value
        select (kvp.Key, end);

    public IEnumerable<(int start, int end, CfgEdge label)> LabelledEdges =>
        from kvp in _edgesByStart
        from end in kvp.Value
        select (kvp.Key, end, GetEdgeLabel(kvp.Key, end));

    public int ActiveNodeCount => Nodes.Count - _deletedNodeCount;
    public int NodeCount => Nodes.Count;
    public bool IsNodeActive(int index) => index >= 0 && index < Nodes.Count && Nodes[index] != null;
    public ControlFlowGraph SetEntry(int i) => new(i, ExitIndex, Nodes, _edgesByStart, _edgesByEnd, _labels, _deletedNodes, _deletedNodeCount);
    public ControlFlowGraph SetExit(int i) => new(EntryIndex, i, Nodes, _edgesByStart, _edgesByEnd, _labels, _deletedNodes, _deletedNodeCount);
    public ImmutableArray<int> Children(int i) => _edgesByStart.TryGetValue(i, out var nodes) ? nodes : ImmutableArray<int>.Empty;
    public ImmutableArray<int> Parents(int i) => _edgesByEnd.TryGetValue(i, out var nodes) ? nodes : ImmutableArray<int>.Empty;
    IList<int> IGraph.Children(int i) => Children(i);
    IList<int> IGraph.Parents(int i) => Parents(i);
    public CfgEdge GetEdgeLabel(int startNode, int endNode) => _labels.TryGetValue((startNode, endNode), out var label) ? label : CfgEdge.True;
    public DominatorTree GetDominatorTree() => this.GetDominatorTree(EntryIndex);
    public DominatorTree GetPostDominatorTree() => Reverse().GetDominatorTree();
    public bool IsCyclic() => GetBackEdges().Any();

    public ControlFlowGraph()
    {
        EntryIndex = -1;
        ExitIndex = -1;
        Nodes = ImmutableList<ICfgNode>.Empty;
        _edgesByStart = EmptyEdges;
        _edgesByEnd = EmptyEdges;
    }

    ControlFlowGraph(
        int entryIndex,
        int exitIndex,
        ImmutableList<ICfgNode> nodes,
        ImmutableDictionary<int, ImmutableArray<int>> edgesByStart,
        ImmutableDictionary<int, ImmutableArray<int>> edgesByEnd,
        ImmutableDictionary<(int, int), CfgEdge> labels,
        ImmutableStack<int> deletedNodes,
        int deletedNodeCount)
    {
        Nodes = nodes;
        _edgesByStart = edgesByStart;
        _edgesByEnd = edgesByEnd;
        _labels = labels;
        _deletedNodes = deletedNodes;
        _deletedNodeCount = deletedNodeCount;
        EntryIndex = entryIndex;
        ExitIndex = exitIndex;

#if DEBUG
        if (Nodes == null) throw new ArgumentNullException(nameof(nodes));
        if (_edgesByStart == null) throw new ArgumentNullException(nameof(edgesByStart));
        if (_edgesByEnd == null) throw new ArgumentNullException(nameof(edgesByEnd));
        if (_labels == null) throw new ArgumentNullException(nameof(labels));
        if (_deletedNodes == null) throw new ArgumentNullException(nameof(deletedNodes));

        string message = null;
        if (entryIndex < 0) message = $"Invalid entry index given ({entryIndex})";
        if (entryIndex > nodes.Count) message = $"Entry index {entryIndex} given, but there are only {nodes.Count} nodes";
        if (nodes[entryIndex] == null) message = $"Entry index {entryIndex} given, but it has been deleted";
        // if (_edgesByEnd.TryGetValue(entryIndex, out _)) message = $"Entry index {entryIndex} given, but it does not have 0 indegree";
        // if (GetEntryNode() != entryIndex) message = $"Entry index {entryIndex} given, but it is not the unique entry node";

        if (exitIndex < 0) message = $"Invalid exit index given ({exitIndex})";
        if (exitIndex > nodes.Count) message = $"Exit index {exitIndex} given, but there are only {nodes.Count} nodes";
        if (nodes[exitIndex] == null) message = $"Exit index {exitIndex} given, but it has been deleted";
        if (message != null)
            throw new ControlFlowGraphException(message, this);
#endif
    }

    ControlFlowGraph(
        int entryIndex,
        int exitIndex,
        ImmutableList<ICfgNode> nodes,
        ImmutableDictionary<int, ImmutableArray<int>> edgesByStart,
        ImmutableDictionary<int, ImmutableArray<int>> edgesByEnd,
        ImmutableDictionary<(int, int), CfgEdge> labels,
        ImmutableStack<int> deletedNodes,
        int deletedNodeCount,
        ControlFlowGraph reversed)
        : this(entryIndex, exitIndex, nodes, edgesByStart, edgesByEnd, labels, deletedNodes, deletedNodeCount)
    {
        _cachedReverse = reversed;
    }

    public ControlFlowGraph(IEnumerable<ICfgNode> nodes, IEnumerable<(int start, int end, CfgEdge label)> edges) : this(-1, -1, nodes, edges) { }
    public ControlFlowGraph(int entryIndex, int exitIndex, IEnumerable<ICfgNode> nodes, IEnumerable<(int start, int end, CfgEdge label)> edges)
    {
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));
        if (edges == null) throw new ArgumentNullException(nameof(edges));

        Nodes = ImmutableList<ICfgNode>.Empty.AddRange(nodes);
        var starts = EmptyEdges.ToBuilder();
        var ends = EmptyEdges.ToBuilder();
        var labels = ImmutableDictionary<(int, int), CfgEdge>.Empty.ToBuilder();

        foreach (var edge in edges)
        {
            if (!IsNodeActive(edge.start)) throw new ArgumentException($"Edge had invalid start node {edge.start}", nameof(edges));
            if (!IsNodeActive(edge.end)) throw new ArgumentException($"Edge had invalid end node {edge.end}", nameof(edges));

            if (!starts.TryGetValue(edge.start, out var endsForStart))
                endsForStart = ImmutableArray<int>.Empty;
            if (!ends.TryGetValue(edge.end, out var startsForEnd))
                startsForEnd = ImmutableArray<int>.Empty;

            if (endsForStart.Contains(edge.end))
                throw new ControlFlowGraphException($"Tried to add the edge ({edge.start}, {edge.end}) twice", this);

            starts[edge.start] = endsForStart.Add(edge.end);
            ends[edge.end] = startsForEnd.Add(edge.start);
            if (edge.label != CfgEdge.True)
                labels.Add((edge.start, edge.end), edge.label);
        }

        _edgesByStart = starts.ToImmutable();
        _edgesByEnd = ends.ToImmutable();
        _labels = labels.ToImmutable();
        EntryIndex = entryIndex == -1 ? this.GetEntryNode() : entryIndex;
        ExitIndex = exitIndex == -1 ? this.GetExitNode() : exitIndex;

        var deleted = new List<int>();
        for (var index = 0; index < Nodes.Count; index++)
            if (Nodes[index] == null)
                deleted.Add(index);

        _deletedNodes = ImmutableStack.CreateRange(deleted);
        _deletedNodeCount = deleted.Count;

        if (_edgesByEnd.TryGetValue(entryIndex, out _))
            throw new ControlFlowGraphException($"Entry index {entryIndex} given, but it does not have 0 indegree", this);

#if DEBUG
        //if (GetEntryNode() != headIndex)
        //    throw new ControlFlowGraphException($"Entry index {headIndex} given, but it is not the unique entry node", this);
#endif
    }

    IGraph IGraph.Reverse() => Reverse();
    public ControlFlowGraph Reverse() =>
        _cachedReverse ??=
            new ControlFlowGraph(
                ExitIndex,
                EntryIndex,
                Nodes,
                _edgesByEnd,
                _edgesByStart,
                _labels.ToImmutableDictionary(x => (x.Key.end, x.Key.start), x => x.Value),
                _deletedNodes,
                _deletedNodeCount,
                this);

    public ControlFlowGraph AddNode(ICfgNode node, out int index)
    {
        if (_deletedNodes.IsEmpty)
        {
            index = Nodes.Count;
            return new ControlFlowGraph(EntryIndex, ExitIndex, Nodes.Add(node), _edgesByStart, _edgesByEnd, _labels, _deletedNodes, 0);
        }

        var deletedNodes = _deletedNodes.Pop(out index);
        Debug.Assert(Nodes[index] == null);
        var nodes = Nodes.SetItem(index, node);
        return new ControlFlowGraph(EntryIndex, ExitIndex, nodes, _edgesByStart, _edgesByEnd, _labels, deletedNodes, _deletedNodeCount - 1);
    }

    public ControlFlowGraph AddEdge(int start, int end, CfgEdge label)
    {
        if (!IsNodeActive(start)) throw new ArgumentException($"Edge had invalid start node {start}", nameof(start));
        if (!IsNodeActive(end)) throw new ArgumentException($"Edge had invalid end node {end}", nameof(end));

        var edgesByStart = _edgesByStart.TryGetValue(start, out var byStart)
            ? _edgesByStart.SetItem(start, byStart.Add(end))
            : _edgesByStart.Add(start, ImmutableArray<int>.Empty.Add(end));

        var edgesByEnd = _edgesByEnd.TryGetValue(end, out var byEnd)
            ? _edgesByEnd.SetItem(end, byEnd.Add(start))
            : _edgesByEnd.Add(end, ImmutableArray<int>.Empty.Add(start));

        var labels = label == CfgEdge.True ? _labels : _labels.Add((start, end), label);

        return new ControlFlowGraph(
            EntryIndex,
            ExitIndex,
            Nodes,
            edgesByStart,
            edgesByEnd,
            labels,
            _deletedNodes,
            _deletedNodeCount);
    }

    public ControlFlowGraph ReplaceNode(int index, ICfgNode newNode) =>
        new(EntryIndex, ExitIndex,
            Nodes.SetItem(index, newNode),
            _edgesByStart, _edgesByEnd, _labels,
            _deletedNodes, _deletedNodeCount);

    public ControlFlowGraph ReplaceNode(int index, ControlFlowGraph replacement)
    {
        var parents = Parents(index);
        var children = Children(index);

        var (graph, mapping) = Merge(replacement);

        int start = mapping[replacement.GetEntryNode()];
        int end = mapping[replacement.GetExitNode()];

        foreach (var parent in parents)
            graph = graph.AddEdge(parent, start, GetEdgeLabel(parent, index));

        foreach (var child in children)
            graph = graph.AddEdge(end, child, GetEdgeLabel(index, child));

        return graph.RemoveNode(index);
    }

    public ControlFlowGraph RemoveNode(int i)
    {
        if (Nodes[i] == null)
            throw new ControlFlowGraphException($"Tried to remove a non-existent node ({i})", this);

        var nodes = Nodes.ToBuilder();
        var byStart = _edgesByStart.ToBuilder();
        var byEnd = _edgesByEnd.ToBuilder();
        var labels = _labels.ToBuilder();
        var deletedNodes = _deletedNodes;
        int deletedCount = _deletedNodeCount;
        int newEntry = EntryIndex;
        int newExit = ExitIndex;

        if (EntryIndex == i)
        {
            if (Parents(i).Length > 0) throw new ControlFlowGraphException($"Tried to remove entry node {i}, but it has parents", this);
            var children = Children(i);
            if (children.Length != 1) throw new ControlFlowGraphException($"Tried to remove entry node {i}, but it does not have a single child", this);
            newEntry = children[0];
        }

        if (ExitIndex == i)
        {
            if (Children(i).Length > 0) throw new ControlFlowGraphException($"Tried to remove exit node {i}, but it has children", this);
            var parents = Parents(i);
            if (parents.Length != 1) throw new ControlFlowGraphException($"Tried to remove exit node {i}, but it does not have a single parent", this);
            newExit = parents[0];
        }

        nodes[i] = null;
        deletedNodes = deletedNodes.Push(i);
        deletedCount++;
        BuilderRemoveEdges(i, byStart, byEnd, labels);

        return new ControlFlowGraph(
            newEntry,
            newExit,
            nodes.ToImmutable(),
            byStart.ToImmutable(),
            byEnd.ToImmutable(),
            labels.ToImmutable(),
            deletedNodes,
            deletedCount);
    }

    static void BuilderAddEdge(int start, int end, CfgEdge label,
        ImmutableDictionary<int, ImmutableArray<int>>.Builder byStart,
        ImmutableDictionary<int, ImmutableArray<int>>.Builder byEnd,
        ImmutableDictionary<(int, int), CfgEdge>.Builder labels)
    {
        byStart[start] = byStart.TryGetValue(start, out var starts)
            ? starts.Add(end)
            : ImmutableArray<int>.Empty.Add(end);

        byEnd[end] = byEnd.TryGetValue(end, out var ends)
            ? ends.Add(start)
            : ImmutableArray<int>.Empty.Add(start);

        if (label != CfgEdge.True)
            labels.Add((start, end), label);
    }

    static void BuilderRemoveEdges(int i,
        ImmutableDictionary<int, ImmutableArray<int>>.Builder byStart,
        ImmutableDictionary<int, ImmutableArray<int>>.Builder byEnd,
        ImmutableDictionary<(int, int), CfgEdge>.Builder labels)
    {
        void RemoveHelper(int start, int end)
        {
            var newEnds = byStart[start].Remove(end);
            if (newEnds.IsEmpty) byStart.Remove(start);
            else byStart[start] = newEnds;

            var newStarts = byEnd[end].Remove(start);
            if (newStarts.IsEmpty) byEnd.Remove(end);
            else byEnd[end] = newStarts;

            labels.Remove((start, end));
        }

        if (byStart.TryGetValue(i, out var ends))
            foreach (var end in ends)
                RemoveHelper(i, end);

        if (byEnd.TryGetValue(i, out var starts))
            foreach (var start in starts)
                RemoveHelper(start, i);
    }

    IGraph IGraph.Defragment() => Defragment(true);
    public ControlFlowGraph Defragment(bool force = false)
    {
        if (_deletedNodeCount == 0 && EntryIndex == 0 && ExitIndex == NodeCount) return this;
        if (!force && (double)_deletedNodeCount / Nodes.Count <= DefragThreshold) return this;

        int[] mapping = new int[Nodes.Count];
        Array.Fill(mapping, -1);
        mapping[EntryIndex] = 0;

        int index = 1;
        for (int i = 0; i < Nodes.Count; i++)
            if (Nodes[i] != null && i != EntryIndex && i != ExitIndex)
                mapping[i] = index++;

        if (EntryIndex != ExitIndex)
            mapping[ExitIndex] = index++;

        var nodes = new ICfgNode[index];
        for (int i = 0; i < Nodes.Count; i++)
            if (Nodes[i] != null)
                nodes[mapping[i]] = Nodes[i];

        var edges =
            from start in _edgesByStart
            from end in start.Value
            select (mapping[start.Key], mapping[end], GetEdgeLabel(start.Key, end));

        return new ControlFlowGraph(mapping[EntryIndex], mapping[ExitIndex], nodes, edges);
    }

    public ControlFlowGraph RemoveEdge(int start, int end)
    {
        var byStart = _edgesByStart;
        var byEnd = _edgesByEnd;
        var labels = _labels.Remove((start, end));

        if (_edgesByStart.TryGetValue(start, out var ends))
        {
            ends = ends.Remove(end);
            byStart = ends.IsEmpty ? _edgesByStart.Remove(start) : _edgesByStart.SetItem(start, ends);
        }

        if (_edgesByEnd.TryGetValue(end, out var starts))
        {
            starts = starts.Remove(start);
            byEnd = starts.IsEmpty ? _edgesByEnd.Remove(end) : _edgesByEnd.SetItem(end, starts);
        }

        if (byStart == _edgesByStart)
            throw new ControlFlowGraphException($"Tried to remove edge ({start}, {end}), but it does not exist");

        return new ControlFlowGraph(EntryIndex, ExitIndex, Nodes, byStart, byEnd, labels, _deletedNodes, _deletedNodeCount);
    }

    public ControlFlowGraph InsertBefore(int position, ICfgNode node) => InsertBefore(position, node, out _);
    public ControlFlowGraph InsertBefore(int position, ICfgNode node, out int newIndex)
    {
        var result = AddNode(node, out newIndex);
        var edges = new List<(int, int, CfgEdge)>();
        foreach (var parent in Parents(position))
        {
            edges.Add((parent, newIndex, result.GetEdgeLabel(parent, position)));
            result = result.RemoveEdge(parent, position);
        }

        result = result.AddEdge(newIndex, position, CfgEdge.True);
        foreach (var edge in edges)
            result = result.AddEdge(edge.Item1, edge.Item2, edge.Item3);
        return result;
    }

    public ControlFlowGraph InsertAfter(int position, ICfgNode node)
    {
        var result = AddNode(node, out var newIndex);
        var edges = new List<(int, int, CfgEdge)>();
        foreach (var child in Children(position))
        {
            edges.Add((newIndex, child, result.GetEdgeLabel(position, child)));
            result = result.RemoveEdge(position, child);
        }

        result = result.AddEdge(position, newIndex, CfgEdge.True);
        foreach (var edge in edges)
            result = result.AddEdge(edge.Item1, edge.Item2, edge.Item3);
        return result;
    }

    public int GetFirstEmptyExitNode()
    {
        int exitNode = -1;
        foreach (var candidate in this.GetExitNodes())
            if (Nodes[candidate] is EmptyNode)
                exitNode = candidate;

        if (exitNode == -1)
            throw new ControlFlowGraphException("Could not find an empty exit node", this);

        return exitNode;
    }

    static (ImmutableArray<int>, ImmutableArray<(int start, int end)>) ToImmutable((List<int> order, List<(int, int)> backEdges) result) 
        => (result.order.ToImmutableArray(), result.backEdges.ToImmutableArray());

    IEnumerable<(int, int)> IGraph.GetBackEdges() => GetBackEdges();
    public ImmutableArray<(int start, int end)> GetBackEdges()
    {
        if (!_cachedBackEdges.HasValue)
            (_cachedPostOrder, _cachedBackEdges) = ToImmutable(GetDfsOrderAndBackEdges(true, false)); // Use post-order, more likely we'll need the cached order later

        return _cachedBackEdges.Value;
    }

    IList<int> IGraph.GetDfsOrder() => GetDfsOrder();
    public ImmutableArray<int> GetDfsOrder()
    {
        if (!_cachedOrder.HasValue)
            (_cachedOrder, _cachedBackEdges) = ToImmutable(GetDfsOrderAndBackEdges(false, false));

        return _cachedOrder.Value;
    }

    public List<int> GetDfsOrderComplete() => GetDfsOrderAndBackEdges(false, true).Item1;

    IList<int> IGraph.GetDfsPostOrder() => GetDfsPostOrder();
    public ImmutableArray<int> GetDfsPostOrder()
    {
        if (!_cachedPostOrder.HasValue)
            (_cachedPostOrder, _cachedBackEdges) = ToImmutable(GetDfsOrderAndBackEdges(true, false));

        return _cachedPostOrder.Value;
    }

    (List<int>, List<(int, int)>) GetDfsOrderAndBackEdges(bool postOrder, bool allNodes)
    {
        var results = new List<int>();
        var backEdges = new List<(int, int)>();
        var visited = new bool[Nodes.Count];
        var stack = new List<int>();

        this.DepthFirstSearch(EntryIndex, visited, stack, results, backEdges, postOrder);
        if (allNodes)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null || visited[i])
                    continue;
                this.DepthFirstSearch(i, visited, stack, results, backEdges, postOrder);
            }
        }

        return (results, backEdges);
    }

    public ControlFlowGraph RemoveBackEdges()
    {
        var backEdges = GetBackEdges();
        var graph = this;
        foreach (var (start, end) in backEdges)
            graph = graph.RemoveEdge(start, end);
        return graph;
    }

    public ControlFlowGraph Canonicalize()
    {
        var nodesByIndex = Nodes
            .Select((x, curIndex) => (curIndex, node: x))
            .Where(x => x.node != null)
            .OrderBy(x => x.node.ToString())
            .Select((x, newIndex) => (x.curIndex, newIndex));

        var mapping = new int[Nodes.Count];
        var nodes = new ICfgNode[Nodes.Count - _deletedNodeCount];
        foreach (var (curIndex, newIndex) in nodesByIndex)
        {
            mapping[curIndex] = newIndex;
            nodes[newIndex] = Nodes[curIndex];
        }

        var edges = Edges
            .Select(x => (mapping[x.start], mapping[x.end], GetEdgeLabel(x.start, x.end)))
            .OrderBy(x => x.Item1)
            .ThenBy(x => x.Item2);

        return new ControlFlowGraph(mapping[EntryIndex], mapping[ExitIndex], nodes, edges);
    }

    public CfgCutResult Cut(HashSet<int> selectedNodes, int entry, int exit)
    {
        if (selectedNodes == null) throw new ArgumentNullException(nameof(selectedNodes));
        List<ICfgNode> remainderNodes = new();
        List<(int, int, CfgEdge)> remainderEdges = new();
        var remainderMapping = new int[Nodes.Count];

        List<ICfgNode> cutNodes = new();
        List<(int, int, CfgEdge)> cutEdges = new();
        var cutMapping = new int[Nodes.Count];

        List<(int, CfgEdge)> cutToRemainderEdges = new();
        List<(int, CfgEdge)> remainderToCutEdges = new();

        for (int i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i] == null)
                continue;

            if (selectedNodes.Contains(i))
            {
                cutMapping[i] = cutNodes.Count;
                cutNodes.Add(Nodes[i]);
            }
            else
            {
                remainderMapping[i] = remainderNodes.Count;
                remainderNodes.Add(Nodes[i]);
            }
        }

        foreach (var edge in Edges)
        {
            bool isStartInCut = selectedNodes.Contains(edge.start);
            bool isEndInCut = selectedNodes.Contains(edge.end);
            CfgEdge edgeLabel = GetEdgeLabel(edge.start, edge.end);
            switch (isStartInCut, isEndInCut)
            {
                case (true, true): cutEdges.Add((cutMapping[edge.start], cutMapping[edge.end], edgeLabel)); break;
                case (false, true): remainderToCutEdges.Add((remainderMapping[edge.start], edgeLabel)); break;
                case (true, false): cutToRemainderEdges.Add((remainderMapping[edge.end], edgeLabel)); break;
                case (false, false): remainderEdges.Add((remainderMapping[edge.start], remainderMapping[edge.end], edgeLabel)); break;
            }
        }

        int remainderEntry = !selectedNodes.Contains(EntryIndex) ? remainderMapping[EntryIndex] : -1;
        int remainderExit = !selectedNodes.Contains(ExitIndex) ? remainderMapping[ExitIndex] : -1;

        int cutEntryIndex = cutNodes.Count;
        cutNodes.Add(new EmptyNode());
        cutEdges.Add((cutEntryIndex, cutMapping[entry], CfgEdge.True));

        var cut = new ControlFlowGraph(cutEntryIndex, cutMapping[exit], cutNodes, cutEdges);
        var remainder = new ControlFlowGraph(remainderEntry, remainderExit, remainderNodes, remainderEdges);
        return new CfgCutResult(cut, remainder, remainderToCutEdges, cutToRemainderEdges);
    }

    public (ControlFlowGraph result, int[] mapping) Merge(ControlFlowGraph other)
    {
        var result = this;
        var mapping = new int[other.Nodes.Count];
        Array.Fill(mapping, -1);

        for (int i = 0; i < other.Nodes.Count; i++)
        {
            var node = other.Nodes[i];
            if (node == null)
                continue;

            result = result.AddNode(node, out var newIndex);
            mapping[i] = newIndex;
        }

        foreach (var (start, end) in other.Edges)
            result = result.AddEdge(mapping[start], mapping[end], other.GetEdgeLabel(start, end));

        return (result, mapping);
    }

    public (int? trueChild, int? falseChild) GetBinaryChildren(int index)
    {
        var children = Children(index);
        int? trueChild = null;
        int? falseChild = null;

        foreach (var child in children)
        {
            switch ( GetEdgeLabel(index, child))
            {
                case CfgEdge.True:
                    if (trueChild != null)
                        throw new ControlFlowGraphException($"Node {index} has 2 true children!", this);
                    trueChild = child;
                    break;

                case CfgEdge.False:
                    if (falseChild != null)
                        throw new ControlFlowGraphException($"Node {index} has 2 false children!", this);
                    falseChild = child;
                    break;
            }
        }

        return (trueChild, falseChild);
    }

    public void Accept(IAstVisitor visitor)
    {
        foreach (var index in GetDfsOrder())
            Nodes[index].Accept(visitor);
    }

    public ControlFlowGraph AcceptBuilder(IAstBuilderVisitor visitor)
    {
        var result = this;
        foreach (var index in GetDfsOrder())
        {
            result.Nodes[index].Accept(visitor);
            if (visitor.Result != null)
                result = result.ReplaceNode(index, visitor.Result);
        }

        return result;
    }

    // Used by https://github.com/hediet/vscode-debug-visualizer
    // ReSharper disable once UnusedMember.Global
    public string Visualize() => ToVis().ToString();
    public DebugVisualizerGraphData ToVis() => DebugVisualizerGraphData.FromCfg(this);
    public string ExportToDot(bool showContent = true, int dpi = 180) // For rendering graphs nicely in GraphViz
    {
        var sb = new StringBuilder();
        sb.Append("# ");
        sb.AppendLine(ToString());
        sb.AppendLine("digraph G {");
        sb.AppendLine($"    graph [ dpi = {dpi} ];");

        var builder = new UnformattedScriptBuilder(false);
        for (int i = 0; i < Nodes.Count; i++)
        {
            if (Nodes[i] == null) continue;
            sb.Append("    "); sb.Append(i);
            sb.Append(" [");
            if (showContent)
                sb.Append("shape=box, ");

            sb.Append("fontname=\"Consolas\", fontsize=8, fillcolor=azure2, style=filled, label=\"");
            sb.Append(i);
            if (i == EntryIndex) sb.Append(" <IN>");
            if (i == ExitIndex) sb.Append(" <OUT>");

            if (showContent)
            {
                sb.Append("\\l");

                builder.Clear();
                var visitor = new FormatScriptVisitor(builder);
                Nodes[i].Accept(visitor);
                sb.Append(builder.Build().Replace(Environment.NewLine, "\\l", StringComparison.Ordinal));
            }

            sb.AppendLine("\\l\"];");
        }

        foreach (var kvp in _edgesByStart)
        {
            var start = kvp.Key;
            foreach (var end in kvp.Value)
            {
                var color = GetEdgeLabel(start, end) switch
                {
                    CfgEdge.True => this.OutDegree(start) > 1 ? "green4" : null,
                    CfgEdge.False => "red3",
                    CfgEdge.DisjointGraphFixup => "purple",
                    CfgEdge.LoopSuccessor => "gold3",
                    CfgEdge.EntryPoint => "blue",
                    _ => null
                };

                sb.Append("    "); sb.Append(start);
                sb.Append(" -> "); sb.Append(end);
                sb.AppendLine(string.IsNullOrEmpty(color) ? " [];" : $" [color={color}];");
            }
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public static ControlFlowGraph FromString(string s) // For test case graphs where we don't care about node contents, just the structure.
    {
        // Syntax: [NodeCount, Entry, Exit, Edges]
        // Edges: 0+1 0-2 2+3 etc. Use + for true, - for false
        // e.g. [5,0,4,0+1 1+2 2+3 3+4]
        s = s.Trim('[', ']');
        var parts = s.Split(',');
        if (parts.Length != 4)
            throw new FormatException("Expected 4 parts in graph description");
        var entry = int.Parse(parts[0].Trim());
        var exit = int.Parse(parts[1].Trim());
        var nodeCount = int.Parse(parts[2].Trim());
        bool[] active = new bool[nodeCount];
        var edges = new List<(int, int, CfgEdge)>();
        foreach (var edgePart in parts[3].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var (index, label) = FindSymbol(edgePart);
            if (index == -1)
                throw new FormatException($"Bad edge \"{edgePart}\", expected two numbers separated by a label symbol (+, -, d, l or e)");

            int start = int.Parse(edgePart[..index]);
            int end = int.Parse(edgePart[(index + 1)..]);
            active[start] = true;
            active[end] = true;
            edges.Add((start, end, label));
        }

        var nodes = BuildTestNodes(nodeCount).ToArray();
        for (int i = 0; i < nodeCount; i++)
            if (!active[i] && i != entry && i != exit)
                nodes[i] = null;

        return new ControlFlowGraph(entry, exit, nodes, edges);
    }

    static (int index, CfgEdge label) FindSymbol(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            switch (s[i])
            {
                case '+': return (i, CfgEdge.True);
                case '-': return (i, CfgEdge.False);
                case 'd': return (i, CfgEdge.DisjointGraphFixup);
                case 'l': return (i, CfgEdge.LoopSuccessor);
                case 'e': return (i, CfgEdge.EntryPoint);
            }
        }

        return (-1, CfgEdge.True);
    }

    static IEnumerable<ICfgNode> BuildTestNodes(int count)
    {
        if (count < 2)
            throw new InvalidOperationException("All control flow graphs require an entry and exit node");

        yield return UAEmit.Empty();
        for (int i = 1; i < count - 1; i++)
            yield return UAEmit.Statement(UAEmit.Const(i));
        yield return UAEmit.Empty();
    }

    public override string ToString() // Emit structural representation without details of node contents
    {
        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(EntryIndex);
        sb.Append(',');
        sb.Append(ExitIndex);
        sb.Append(',');
        sb.Append(NodeCount);
        sb.Append(',');

        bool first = true;
        foreach (var (start, end, label) in LabelledEdges)
        {
            var symbol = label switch {
                CfgEdge.True => '+',
                CfgEdge.False => '-',
                CfgEdge.DisjointGraphFixup => 'd',
                CfgEdge.LoopSuccessor => 'l',
                CfgEdge.EntryPoint => 'n',
                _ => throw new ArgumentOutOfRangeException()
            };

            if (!first)
                sb.Append(' ');
            sb.Append(start);
            sb.Append(symbol);
            sb.Append(end);
            first = false;
        }

        sb.Append(']');
        return sb.ToString();
    }
}
#pragma warning restore 8321

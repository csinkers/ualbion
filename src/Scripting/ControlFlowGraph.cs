﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UAlbion.Scripting.Ast;

#pragma warning disable 8321 // Stop warnings about Vis() debug functions
namespace UAlbion.Scripting
{
    public delegate ControlFlowGraph RecordFunc(string description, ControlFlowGraph graph);
    public interface IGraph<out TNode, out TLabel>
    {
        int NodeCount { get; }
        TNode GetNode(int i);
        TLabel GetEdgeLabel(int start, int end);
        IEnumerable<(int start, int end)> Edges { get; }
        IEnumerable<int> Children(int i);
        IEnumerable<int> Parents(int i);
    }

    public enum CfgEdge
    {
        True,
        False,
        DisjointGraphFixup
    }

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
        DominatorTree _cachedDominatorTree;
        ImmutableArray<int> _cachedOrder;
        ImmutableArray<int> _cachedPostOrder;
        ImmutableArray<(int start, int end)> _cachedBackEdges;
        ImmutableArray<CfgLoop> _cachedLoops;

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

        public ControlFlowGraph SetEntry(int i) => new(i, ExitIndex, Nodes, _edgesByStart, _edgesByEnd, _labels, _deletedNodes, _deletedNodeCount);
        public ControlFlowGraph SetExit(int i) => new(EntryIndex, i, Nodes, _edgesByStart, _edgesByEnd, _labels, _deletedNodes, _deletedNodeCount);
        public ControlFlowGraph(IEnumerable<ICfgNode> nodes, IEnumerable<(int start, int end, CfgEdge label)> edges) : this(-1, -1, nodes, edges) { }
        public ControlFlowGraph(int entryIndex, int exitIndex, IEnumerable<ICfgNode> nodes, IEnumerable<(int start, int end, CfgEdge label)> edges)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (edges == null) throw new ArgumentNullException(nameof(edges));

            Nodes = ImmutableList<ICfgNode>.Empty.AddRange(nodes);
            var starts = EmptyEdges.ToBuilder();
            var ends = EmptyEdges.ToBuilder();
            var labels = ImmutableDictionary<(int, int), CfgEdge>.Empty.ToBuilder();
            string error = null;

            foreach (var edge in edges)
            {
                if (edge.start >= Nodes.Count)
                    throw new ArgumentException($"Edge starts at node {edge.start}, but the graph only contains {Nodes.Count}", nameof(edges));
                if (edge.end >= Nodes.Count)
                    throw new ArgumentException($"Edge ends at node {edge.end}, but the graph only contains {Nodes.Count}", nameof(edges));

                if (!starts.TryGetValue(edge.start, out var endsForStart))
                    endsForStart = ImmutableArray<int>.Empty;
                if (!ends.TryGetValue(edge.end, out var startsForEnd))
                    startsForEnd = ImmutableArray<int>.Empty;

                if (endsForStart.Contains(edge.end))
                {
                    error = $"Tried to add the edge ({edge.start}, {edge.end}) twice";
                    break;
                }

                starts[edge.start] = endsForStart.Add(edge.end);
                ends[edge.end] = startsForEnd.Add(edge.start);
                if (edge.label != CfgEdge.True)
                    labels.Add((edge.start, edge.end), edge.label);
            }

            _edgesByStart = starts.ToImmutable();
            _edgesByEnd = ends.ToImmutable();
            _labels = labels.ToImmutable();
            EntryIndex = entryIndex == -1 ? GetEntryNode() : entryIndex;
            ExitIndex = exitIndex == -1 ? GetExitNode() : exitIndex;

            var deleted = new List<int>();
            for (var index = 0; index < Nodes.Count; index++)
                if (Nodes[index] == null)
                    deleted.Add(index);

            _deletedNodes = ImmutableStack.CreateRange(deleted);
            _deletedNodeCount = deleted.Count;

            if (error != null)
                throw new ControlFlowGraphException(error, this);

            if (_edgesByEnd.TryGetValue(entryIndex, out _))
                error = $"Entry index {entryIndex} given, but it does not have 0 indegree";
#if DEBUG
            //if (GetEntryNode() != headIndex)
            //    error = $"Entry index {headIndex} given, but it is not the unique entry node";
#endif

            if (error != null)
                throw new ControlFlowGraphException(error, this);
        }

        public ImmutableArray<int> Children(int i) => _edgesByStart.TryGetValue(i, out var nodes) ? nodes : ImmutableArray<int>.Empty;
        public ImmutableArray<int> Parents(int i) => _edgesByEnd.TryGetValue(i, out var nodes) ? nodes : ImmutableArray<int>.Empty;
        int InDegree(int node) => Parents(node).Length;
        int OutDegree(int node) => Children(node).Length;

        IEnumerable<int> IGraph<ICfgNode, CfgEdge>.Children(int i) => Children(i);
        IEnumerable<int> IGraph<ICfgNode, CfgEdge>.Parents(int i) => Parents(i);

        public CfgEdge GetEdgeLabel(int start, int end) => _labels.TryGetValue((start, end), out var label) ? label : CfgEdge.True;
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
            if (start < 0) throw new ArgumentOutOfRangeException(nameof(start), $"Tried to add edge with invalid start index {start}");
            if (end < 0) throw new ArgumentOutOfRangeException(nameof(end), $"Tried to add edge with invalid end index {end}");
            if (start >= Nodes.Count) throw new ArgumentOutOfRangeException(nameof(start), $"Tried to add edge with start index {start}, but there are only {Nodes.Count} nodes");
            if (end >= Nodes.Count) throw new ArgumentOutOfRangeException(nameof(end), $"Tried to add edge with end index {end}, but there are only {Nodes.Count} nodes");

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

        public ControlFlowGraph Defragment(bool force = false)
        {
            if (!force && (double)_deletedNodeCount / Nodes.Count < DefragThreshold)
                return this;

            int[] mapping = new int[Nodes.Count];
            int index = 0;
            for (int i = 0; i < Nodes.Count; i++)
                if (Nodes[i] != null)
                    mapping[i] = index++;

            var edges =
                from start in _edgesByStart
                from end in start.Value
                select (mapping[start.Key], mapping[end], GetEdgeLabel(start.Key, end));

            return new ControlFlowGraph(mapping[EntryIndex], mapping[ExitIndex], Nodes.Where(x => x != null), edges);
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

        public ControlFlowGraph InsertBefore(int position, ICfgNode node)
        {
            var result = AddNode(node, out var newIndex);
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

        public bool IsCyclic()
        {
            if (_cachedBackEdges != null)
                return _cachedBackEdges.Any();

            // 0: White, 1: Grey, 2: Black
            int whiteCount = Nodes.Count;
            var state = new int[Nodes.Count];

            bool CyclicInner(int current)
            {
                Debug.Assert(state[current] == 0);
                state[current] = 1;
                whiteCount--;

                if (Nodes[current] != null)
                {
                    foreach (int i in Children(current))
                    {
                        switch (state[i])
                        {
                            case 0 when CyclicInner(i):
                            case 1: return true;
                        }
                    }
                }

                state[current] = 2;
                return false;
            }

            while (whiteCount > 0)
                for (int i = 0; i < Nodes.Count; i++)
                    if (state[i] == 0 && CyclicInner(i))
                        return true;

            return false;
        }

        public int GetEntryNode()
        {
            // Should be the only node with indegree 0
            int result = -1;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null || !Parents(i).IsEmpty)
                    continue;
                if (result != -1)
                    throw new ControlFlowGraphException("Multiple entry nodes were found in control flow graph!", this);
                result = i;
                // return result;
            }

            return result;
        }

        public int GetExitNode()
        {
            // Should be the only node with outdegree 0
            int result = -1;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null || !Children(i).IsEmpty)
                    continue;
                if (result != -1)
                    throw new ControlFlowGraphException("Multiple exit nodes were found in control flow graph!", this);
                result = i;
                // return result;
            }

            return result;
        }

        public IEnumerable<int> GetExitNodes()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null || !Children(i).IsEmpty) continue;
                yield return i;
            }
        }

        public ImmutableArray<(int, int)> GetBackEdges()
        {
            if (_cachedBackEdges == null)
                GetDfsOrder();
            return _cachedBackEdges;
        }

        void DepthFirstSearch(
            int index,
            bool[] visited,
            List<int> stack,
            List<int> results,
            List<(int, int)> backEdges,
            bool postOrder)
        {
            visited[index] = true;
            stack.Add(index);
            if (!postOrder && results != null)
                results.Add(index);

            foreach (int i in Children(index))
            {
                if (!visited[i])
                    DepthFirstSearch(i, visited, stack, results, backEdges, postOrder);
                else if (stack.Contains(i) && backEdges != null)
                    backEdges.Add((index, i));
            }

            stack.RemoveAt(stack.Count - 1);
            if (postOrder && results != null)
                results.Add(index);
        }

        public ImmutableArray<int> GetDfsOrder()
        {
            if (_cachedOrder == null)
                (_cachedOrder, _cachedBackEdges) = GetDfsOrderInner(false);
            return _cachedOrder;
        }

        public ImmutableArray<int> GetDfsPostOrder()
        {
            if (_cachedPostOrder == null)
                (_cachedPostOrder, _cachedBackEdges) = GetDfsOrderInner(true);

            return _cachedPostOrder;
        }

        (ImmutableArray<int>, ImmutableArray<(int, int)>) GetDfsOrderInner(bool postOrder)
        {
            var results = new List<int>();
            var backEdges = new List<(int, int)>();
            var visited = new bool[Nodes.Count];
            var stack = new List<int>();

            DepthFirstSearch(EntryIndex, visited, stack, results, backEdges, postOrder);
#if DEBUG
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null || visited[i]) continue;
                stack.Clear();
                DepthFirstSearch(i, visited, stack, results, backEdges, postOrder);
                // throw new ControlFlowGraphException("Disconnected graph found during depth-first sort!", this);
            }
#endif

            return (results.ToImmutableArray(), backEdges.ToImmutableArray());
        }

        public DominatorTree GetPostDominatorTree() => Reverse().GetDominatorTree();
        public DominatorTree GetDominatorTree()
        {
            // TODO: Use more efficient Lengauer-Tarjan algorithm if required
            if (_cachedDominatorTree != null)
                return _cachedDominatorTree;

            if (Parents(EntryIndex).Any())
                throw new ControlFlowGraphException("Tried to obtain dominator tree of graph, but the entry was not parentless.", this);

            var dominators = new List<int>[Nodes.Count];
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null)
                    continue;

                dominators[i] = new List<int>(Nodes.Count);
                if (i == EntryIndex)
                    dominators[i].Add(i);
                else
                    for (int j = 0; j < Nodes.Count; j++)
                        if (Nodes[j] != null)
                            dominators[i].Add(j); // Start off with everything dominating everything then remove the ones that aren't
            }

            var postOrder = GetDfsPostOrder();
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var index in postOrder)
                {
                    if (index == EntryIndex)
                        continue;

                    List<int> currentDominators;
                    var parents = Parents(index);
                    if (parents.Length > 0)
                    {
                        currentDominators = dominators[parents[0]].ToList(); // make a copy to avoid modifying the parent
                        for (var i = 1; i < parents.Length; i++)
                            currentDominators = currentDominators.Intersect(dominators[parents[i]]).ToList();
                    }
                    else currentDominators = new List<int>();

                    if (!currentDominators.Contains(index))
                        currentDominators.Add(index);

                    if (!ListEquals(dominators[index], currentDominators))
                    {
                        dominators[index] = currentDominators;
                        changed = true;
                    }
                }
            }

            var tree = DominatorTree.Empty;
            foreach (List<int> list in dominators)
            {
                if (list == null)
                    continue;

                // The dominator lists could be out of order, need to make sure.
                list.Sort((x, y) => x == y ? 0 : dominators[y].Contains(x) ? -1 : 1);
                if (list[0] != EntryIndex)
                    throw new ControlFlowGraphException("Disjoint graph detected while computing dominator tree", this);

                tree = tree.AddPath(list);
            }

            _cachedDominatorTree = tree;
            return tree;
        }

        public ControlFlowGraph RemoveBackEdges()
        {
            var backEdges = GetBackEdges();
            var graph = this;
            foreach (var (start, end) in backEdges)
                graph = graph.RemoveEdge(start, end);
            return graph;
        }

        static bool ListEquals(List<int> x, List<int> y) => x.Count == y.Count && !x.Except(y).Any();

        public string ExportToDot(bool showContent = true, int dpi = 180)
        {
            var sb = new StringBuilder();
            ExportToDot(sb, showContent, dpi);
            return sb.ToString();
        }

        public void ExportToDot(StringBuilder sb, bool showContent = true, int dpi = 180)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.AppendLine("digraph G {");
            sb.AppendLine($"    graph [ dpi = {dpi} ];");

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null) continue;
                sb.Append("    "); sb.Append(i);
                sb.Append(" [");
                if (showContent)
                    sb.Append("shape=box, ");

                sb.Append("fontname = \"Consolas\", fontsize=8, fillcolor=azure2, style=filled, label=\"");
                sb.Append(i);
                if (i == EntryIndex) sb.Append(" <IN>");
                if (i == ExitIndex) sb.Append(" <OUT>");

                if (showContent)
                {
                    sb.Append("\\l");

                    var visitor = new FormatScriptVisitor();
                    Nodes[i].Accept(visitor);
                    sb.Append(visitor.Code.Replace(Environment.NewLine, "\\l", StringComparison.InvariantCulture));
                }

                sb.AppendLine("\\l\"];");
            }

            foreach (var kvp in _edgesByStart)
            {
                var start = kvp.Key;
                foreach (var end in kvp.Value)
                {
                    sb.Append("    "); sb.Append(start);
                    sb.Append(" -> "); sb.Append(end);
                    switch (GetEdgeLabel(start, end))
                    {
                        case CfgEdge.True:
                            if (OutDegree(start) > 1)
                                sb.Append(" [color=green4];");
                            else sb.Append(" [];");
                            break;
                        case CfgEdge.False: sb.Append(" [color=red3];"); break;
                        case CfgEdge.DisjointGraphFixup: sb.Append(" [color=purple]"); break;
                        default: sb.Append(" [];"); break;
                    }
                }
            }
            sb.AppendLine("}");
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

        public int[] GetComponentMapping()
        {
            int componentIndex = 0;
            var mapping = new int[NodeCount];
            var stack = new Stack<int>();
            Array.Fill(mapping, -1);

            for (int i = 0; i < NodeCount; i++)
            {
                if (Nodes[i] == null || mapping[i] != -1)
                    continue;

                stack.Push(i);
                while (stack.TryPop(out var current))
                {
                    if (mapping[current] != -1)
                        continue;

                    mapping[current] = componentIndex;
                    foreach (int child in Children(current))
                        stack.Push(child);
                }

                componentIndex++;
            }

            return mapping;
        }

        public bool[] GetReachability(int start, out int reachableCount)
        {
            reachableCount = 0;
            var visited = new bool[NodeCount];
            var stack = new Stack<int>();

            stack.Push(start);
            while (stack.TryPop(out var current))
            {
                if (visited[current])
                    continue;

                visited[current] = true;
                reachableCount++;
                foreach (int child in Children(current))
                    stack.Push(child);
            }

            return visited;
        }

        public List<List<int>> GetStronglyConnectedComponents()
        {
            List<List<int>> result = new();
            List<int> finished = new();
            var visited = new bool[Nodes.Count];

            void Inner(int node)
            {
                visited[node] = true;
                var children = Children(node);
                foreach (int child in children)
                    if (!visited[child])
                        Inner(child);
                finished.Add(node);
            }

            Array.Clear(visited, 0, visited.Length);
            for (int i = 0; i < Nodes.Count; i++)
                if (!visited[i])
                    Inner(i);

            var reversedEdges = Reverse();
            List<int> Inner2(int node)
            {
                var componentResult = new List<int> { node };
                visited[node] = true;
                var children = reversedEdges.Children(node);
                foreach (int child in children)
                    if (!visited[child])
                        componentResult.AddRange(Inner2(child));
                return componentResult;
            }

            Array.Clear(visited, 0, visited.Length);
            for (int i = finished.Count - 1; i >= 0; i--)
                if (!visited[finished[i]])
                    result.Add(Inner2(finished[i]));

            return result;
        }

        public List<List<int>> GetAllSimpleCyclePaths(IList<int> component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));
            List<int> stack = new();
            List<int> blocked = new();
            List<List<int>> result = new();
            List<(int, int)> blockMap = new();
            var visited = new bool[Nodes.Count];

            static void Unblock(int index, List<int> blocked, List<(int, int)> blockMap)
            {
                blocked.Remove(index);
                for (int i = 0; i < blockMap.Count; i++)
                {
                    if (blockMap[i].Item1 == index)
                        Unblock(blockMap[i].Item2, blocked, blockMap);
                    blockMap.RemoveAt(i);
                    i--;
                }
            }

            bool Inner(int startIndex, int index)
            {
                bool foundCycle = false;
                stack.Add(index);
                blocked.Add(index);

                foreach (int n in Children(index))
                {
                    if (visited[n] || !component.Contains(n))
                        continue;

                    if (n == startIndex)
                    {
                        result.Add(stack.ToList());
                        foundCycle = true;
                    }
                    else if (!blocked.Contains(n) && Inner(startIndex, n))
                        foundCycle = true;
                }

                if (foundCycle)
                    Unblock(index, blocked, blockMap);
                else
                    foreach (int n in Children(index))
                        if (!visited[n] && component.Contains(n))
                            blockMap.Add((index, n));

                stack.Remove(index);
                return foundCycle;
            }

            foreach (var i in component)
            {
                Inner(i, i);
                visited[i] = true;
            }

            return result;
        }

        public List<List<int>> GetAllSimpleLoops(List<int> component)
        {
            var result = new List<List<int>>();
            var backEdges = GetBackEdges().ToHashSet();
            var headers = backEdges.Select(e => e.Item2).Distinct().ToList();

            var cycles = GetAllSimpleCyclePaths(component);
            foreach (int header in headers)
            {
                /*Func<string> vis = () => // For VS Code debug visualisation
                {
                    var d = ToVis();
                    foreach (var n in d.Nodes)
                        if (component.Contains(int.Parse(n.Id, CultureInfo.InvariantCulture)))
                            n.Color = "#4040b0";
                    return d.AddPointer("header", header).ToString();
                };*/

                if (!component.Contains(header))
                    continue;

                var simplePaths = new List<List<int>>();
                foreach (List<int> path in cycles)
                {
                    if (!path.Contains(header))
                        continue;

                    bool skip = headers.Any(x => x != header && path.Contains(x));
                    if (skip)
                        continue;
                    simplePaths.Add(path);
                }

                if (simplePaths.Count == 0)
                    continue;

                var loopParts = new List<int> { header };
                foreach (List<int> path in simplePaths)
                    loopParts = loopParts.Union(path).ToList();

                result.Add(loopParts);
            }

            return result;
        }

        public CfgLoop GetLoopInformation(List<int> nodes)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (nodes.Count == 0) throw new ArgumentException("Empty loop provided to GetLoopInformation", nameof(nodes));

            var body = new List<LoopPart>();
            var header = new LoopPart(nodes[0], true);
            var exits = new HashSet<int>();

            // Determine if header can break out of the loop
            foreach (int child in Children(nodes[0]))
            {
                if (nodes.Contains(child))
                    continue;
                CfgEdge edgeLabel = GetEdgeLabel(nodes[0], child);
                header = new LoopPart(header.Index, true, Break: true, Negated: edgeLabel == CfgEdge.True);
                exits.Add(child);
            }

            for (int i = 1; i < nodes.Count; i++)
            {
                bool isContinue = false;
                bool isBreak = false;
                bool isTail = true;
                bool negated = false;

                foreach (int child in Children(nodes[i]))
                {
                    // Func<string> vis = () => ToVis().AddPointer("i", nodes[i]).AddPointer("child", child).ToString(); // For VS Code debug visualisation

                    if (child == header.Index) // Jump to header = possible continue
                        isContinue = true;
                    else if (nodes.Contains(child))
                        isTail = false;
                    else
                    {
                        negated = GetEdgeLabel(nodes[i], child) == CfgEdge.False;
                        isBreak = true;
                        exits.Add(child);
                    }
                }

                bool hasOutsideEntry = Enumerable.Any(Parents(nodes[i]), x => !nodes.Contains(x));
                body.Add(new LoopPart(nodes[i], false, isTail, isBreak, isContinue, hasOutsideEntry, negated));
            }

            bool isMultiExit = exits.Count > 1;
            int? mainExit;
            if (isMultiExit)
            {
                // If the main loop exit post-dominates all the others then we should be able to structure them
                int? headerExit = Children(header.Index).Intersect(exits).Select(x => (int?)x).SingleOrDefault();
                var tailExits = body
                    .Where(x => x.Tail)
                    .SelectMany(x => Children(x.Index))
                    .Intersect(exits)
                    .ToList();

                mainExit = headerExit ?? (tailExits.Count == 1 ? tailExits[0] : null);
                if (mainExit.HasValue)
                {
                    var postDom = GetPostDominatorTree();
                    if (exits.All(x => x == mainExit.Value || postDom.Dominates(mainExit.Value, x)))
                        isMultiExit = false;
                }
            }
            else mainExit = exits.SingleOrDefault();

            return new CfgLoop(header, body, isMultiExit, mainExit);
        }

        public ImmutableArray<CfgLoop> GetLoops()
        {
            if (_cachedLoops != null)
                return _cachedLoops;

            var components = GetStronglyConnectedComponents();
            _cachedLoops =
                (from component in components.Where(x => x.Count > 1)
                 from loop in GetAllSimpleLoops(component)
                 select GetLoopInformation(loop)).ToImmutableArray();

            return _cachedLoops;
        }

        public List<List<int>> GetAllReachingPaths(int from, int to)
        {
            var result = new List<List<int>>();
            GetReachingPath(from, to, new List<int>(), result);
            return result;
        }

        public void GetRegionParts(HashSet<int> region, int entry, int exit)
        {
            if (region.Contains(entry)) return;

            region.Add(entry);
            foreach (var child in Children(entry).Where(child => child != exit))
                GetRegionParts(region, child, exit);
        }

        public List<(HashSet<int> nodes, int entry, int exit)> GetAllSeseRegions()
        {
            var domTree = GetDominatorTree();
            var postDomTree = GetPostDominatorTree();
            List<(HashSet<int>, int, int)> regions = new();

            foreach (var i in GetDfsPostOrder())
            {
                for (int j = 0; j < Nodes.Count; j++)
                {
                    if (Nodes[j] == null) continue;
                    if (i == j) continue;
                    if (!domTree.Dominates(i, j)) continue;
                    if (!postDomTree.Dominates(j, i)) continue;
                    if (Children(j).Length > 1) continue;
                    if (Parents(i).Length > 1) continue;

                    HashSet<int> region = new();
                    GetRegionParts(region, i, j);
                    region.Add(j);
                    regions.Add((region, i, j));
                }
            }
            return regions;
        }

        public List<int> GetTopogicalOrder()
        {
            var result = new List<int>();
            var visited = new bool[Nodes.Count];

            bool found = true;
            while (found)
            {
                found = false;
                for (int i = 0; i < Nodes.Count; i++)
                {
                    /*Func<string> vis = () => // For VS Code debug visualisation
                    {
                        var d = ToVis();
                        foreach (var n in d.Nodes)
                            if (visited[int.Parse(n.Id, CultureInfo.InvariantCulture)])
                                n.Color = "#a0a0a0";
                        for (int ri = 0; ri < result.Count; ri++)
                            d.AddPointer($"r{ri}", result[ri]);
                        return d.AddPointer("i", i).ToString();
                    }; */

                    if (visited[i])
                        continue; // Don't visit nodes twice


                    if (Parents(i).Any(x => !visited[x]))
                        continue; // If we haven't visited all of the parents we can't evaluate this node yet.

                    visited[i] = true;
                    result.Add(i);
                    found = true;
                    break;
                }
            }

            if (result.Count != ActiveNodeCount) // Cycle detected
                result.Clear();

            return result;
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

        public void GetReachingPath(int start, int target, List<int> path, List<List<int>> stack)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            path.Add(start);
            var children = Children(start);
            foreach (int child in children)
            {
                var npath = path.ToList();

                if (child == target)
                {
                    npath.Add(target);
                    stack.Add(npath);
                    continue;
                }

                if (path.Contains(child))
                    continue;

                GetReachingPath(child, target, npath, stack);
            }
        }

        public IEnumerable<int> GetBranchNodes()
            => _edgesByStart
               .Where(x => x.Value.Length > 1)
               .Select(x => x.Key);

        public IEnumerable<int> GetNonBranchNodes()
            => _edgesByStart
                .Where(x => x.Value.Length <= 1)
                .Select(x => x.Key);

        public (int? trueChild, int? falseChild) GetBinaryChildren(int index)
        {
            var children = Children(index);
            if (children.Length > 2)
                throw new ControlFlowGraphException($"Node {index} has {children.Length} children! Max allowed is 2 for branch events, 1 for regular events.", this);

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

        public int[] GetShortestPaths()
        {
            var result = new int[Nodes.Count];
            Array.Fill(result, int.MaxValue);
            result[EntryIndex] = 0;

            foreach (var i in GetTopogicalOrder())
                foreach (var child in Children(i))
                    if (result[child] > result[i] + 1)
                        result[child] = result[i] + 1;

            return result;
        }

        public int[] GetLongestPaths()
        {
            var result = new int[Nodes.Count];
            Array.Fill(result, int.MinValue);
            result[EntryIndex] = 0;

            foreach (var i in GetTopogicalOrder())
                foreach (var child in Children(i))
                    if (result[child] < result[i] + 1)
                        result[child] = result[i] + 1;

            return result;
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
        public static IEnumerable<ICfgNode> BuildTestNodes(int count)
        {
            if (count < 2)
                throw new InvalidOperationException("All control flow graphs require an entry and exit node");

            yield return Emit.Empty();
            for (int i = 1; i < count - 1; i++)
                yield return Emit.Statement(Emit.Const(i));
            yield return Emit.Empty();
        }

        public static ControlFlowGraph FromString(string s)
        {
            // Syntax: [NodeCount, Entry, Exit, Edges]
            // Edges: 0+1 0-2 2+3 etc, + for true, - for false
            // [5,0,4,0+1 1+2 2+3 3+4]
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
                int plusIndex = edgePart.IndexOf('+');
                int minusIndex = edgePart.IndexOf('-');
                if (plusIndex <= 0 && minusIndex <= 0)
                    throw new FormatException($"Bad edge \"{edgePart}\", expected two numbers separated by a + or -");

                var index = Math.Max(plusIndex, minusIndex);
                CfgEdge label = plusIndex > 0 ? CfgEdge.True : CfgEdge.False;
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

        public override string ToString()
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
                if (label == CfgEdge.DisjointGraphFixup) // Disjoint fixups are implied
                    continue;

                if (!first)
                    sb.Append(' ');
                sb.Append(start);
                sb.Append(label == CfgEdge.True ? '+' : '-');
                sb.Append(end);
                first = false;
            }

            sb.Append(']');
            return sb.ToString();
        }
    }
}
#pragma warning restore 8321

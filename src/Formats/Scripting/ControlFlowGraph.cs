using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UAlbion.Formats.Scripting
{
    public class ControlFlowGraph
    {
        const double DefragThreshold = 0.5; // If the majority are deleted then defrag
        static readonly ImmutableDictionary<int, ImmutableArray<int>> EmptyEdges = ImmutableDictionary<int, ImmutableArray<int>>.Empty;
        public ICfgNode Head => Nodes.Count == 0 ? null : Nodes[_headIndex];
        int _headIndex { get; }
        public ImmutableList<ICfgNode> Nodes { get; }
        readonly ImmutableDictionary<int, ImmutableArray<int>> _edgesByStart;
        readonly ImmutableDictionary<int, ImmutableArray<int>> _edgesByEnd;
        readonly ImmutableHashSet<(int start, int end)> _falseEdges;
        readonly ImmutableStack<int> _deletedNodes;
        readonly int _deletedNodeCount;

        // Memoised results
        ControlFlowGraph _cachedReverse;
        TreeNode<int> _cachedDominatorTree;
        ImmutableArray<int> _cachedOrder;
        ImmutableArray<int> _cachedPostOrder;
        ImmutableArray<(int start, int end)> _cachedBackEdges;
        ImmutableArray<CfgLoop> _cachedLoops;

        public IEnumerable<(int start, int end)> Edges =>
            from kvp in _edgesByStart
            from end in kvp.Value
            select (kvp.Key, end);

        public ControlFlowGraph()
        {
            _headIndex = 0;
            Nodes = ImmutableList<ICfgNode>.Empty;
            _edgesByStart = EmptyEdges;
            _edgesByEnd = EmptyEdges;
        }

        ControlFlowGraph(
            int headIndex,
            ImmutableList<ICfgNode> nodes,
            ImmutableDictionary<int, ImmutableArray<int>> edgesByStart,
            ImmutableDictionary<int, ImmutableArray<int>> edgesByEnd,
            ImmutableHashSet<(int, int)> falseEdges,
            ImmutableStack<int> deletedNodes,
            int deletedNodeCount)
        {
            Nodes = nodes;
            _edgesByStart = edgesByStart;
            _edgesByEnd = edgesByEnd;
            _falseEdges = falseEdges;
            _deletedNodes = deletedNodes;
            _deletedNodeCount = deletedNodeCount;

#if DEBUG
            if (Nodes == null) throw new ArgumentNullException(nameof(nodes)); 
            if (_edgesByStart == null) throw new ArgumentNullException(nameof(edgesByStart)); 
            if (_edgesByEnd == null) throw new ArgumentNullException(nameof(edgesByEnd)); 
            if (_falseEdges == null) throw new ArgumentNullException(nameof(falseEdges)); 
            if (_deletedNodes == null) throw new ArgumentNullException(nameof(deletedNodes));

            if(headIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(headIndex), $"Invalid head index given ({headIndex})");
            if (headIndex > nodes.Count)
                throw new ArgumentOutOfRangeException(nameof(headIndex), $"Head index {headIndex} given, but there are only {nodes.Count} nodes");
            if (nodes[headIndex] == null)
                throw new ArgumentOutOfRangeException(nameof(headIndex), $"Head index {headIndex} given, but it has been deleted");
            if (_edgesByEnd.TryGetValue(headIndex, out _))
                throw new ArgumentOutOfRangeException(nameof(headIndex), $"Head index {headIndex} given, but it does not have 0 indegree");
            if (GetEntryNode() != headIndex)
                throw new ArgumentOutOfRangeException(nameof(headIndex), $"Head index {headIndex} given, but it is not the unique entry node");
#endif

            _headIndex = headIndex;
        }

        ControlFlowGraph(
            int headIndex,
            ImmutableList<ICfgNode> nodes,
            ImmutableDictionary<int, ImmutableArray<int>> edgesByStart,
            ImmutableDictionary<int, ImmutableArray<int>> edgesByEnd,
            ImmutableHashSet<(int, int)> falseEdges,
            ImmutableStack<int> deletedNodes,
            int deletedNodeCount,
            ControlFlowGraph reversed)
            : this(headIndex, nodes, edgesByStart, edgesByEnd, falseEdges, deletedNodes, deletedNodeCount)
        {
            _cachedReverse = reversed;
        }

        public ControlFlowGraph(int headIndex, IEnumerable<ICfgNode> nodes, IEnumerable<(int start, int end, bool label)> edges)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (edges == null) throw new ArgumentNullException(nameof(edges));

            Nodes = ImmutableList<ICfgNode>.Empty.AddRange(nodes);
            var starts = EmptyEdges.ToBuilder();
            var ends = EmptyEdges.ToBuilder();
            var falseEdges = ImmutableHashSet<(int, int)>.Empty.ToBuilder();

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
                    throw new ArgumentException($"Tried to add the edge ({edge.start}, {edge.end}) twice", nameof(edges));

                starts[edge.start] = endsForStart.Add(edge.end);
                ends[edge.end] = startsForEnd.Add(edge.start);
                if (!edge.label)
                    falseEdges.Add((edge.start, edge.end));
            }

            _edgesByStart = starts.ToImmutable();
            _edgesByEnd = ends.ToImmutable();
            _falseEdges = falseEdges.ToImmutable();
            _deletedNodes = ImmutableStack<int>.Empty;

            if (_edgesByEnd.TryGetValue(headIndex, out _))
                throw new ArgumentOutOfRangeException(nameof(headIndex), $"Head index {headIndex} given, but it does not have 0 indegree");
#if DEBUG
            if (GetEntryNode() != headIndex)
                throw new ArgumentOutOfRangeException(nameof(headIndex), $"Head index {headIndex} given, but it is not the unique entry node");
#endif
            _headIndex = headIndex;
        }

        public ImmutableArray<int> Children(int i) => _edgesByStart.TryGetValue(i, out var nodes) ? nodes : ImmutableArray<int>.Empty;
        public ImmutableArray<int> Parents(int i) => _edgesByEnd.TryGetValue(i, out var nodes) ? nodes : ImmutableArray<int>.Empty;
        public bool GetEdgeLabel(int start, int end) => !_falseEdges.Contains((start, end));
        public ControlFlowGraph Reverse() =>
            _cachedReverse ??= 
                new ControlFlowGraph(
                GetExitNode(),
                Nodes,
                _edgesByEnd,
                _edgesByStart,
                _falseEdges.Select(x => (x.end, x.start)).ToImmutableHashSet(),
                _deletedNodes,
                _deletedNodeCount,
                this);

        public ControlFlowGraph AddNode(ICfgNode node, out int index)
        {
            if (_deletedNodes.IsEmpty)
            {
                index = Nodes.Count;
                return new ControlFlowGraph(_headIndex, Nodes.Add(node), _edgesByStart, _edgesByEnd, _falseEdges, _deletedNodes, 0);
            }

            var deletedNodes = _deletedNodes.Pop(out index);
            Debug.Assert(Nodes[index] == null);
            var nodes = Nodes.SetItem(index, node);
            return new ControlFlowGraph(_headIndex, nodes, _edgesByStart, _edgesByEnd, _falseEdges, deletedNodes, _deletedNodeCount - 1);
        }

        public ControlFlowGraph AddEdge(int start, int end, bool label)
        {
            var edgesByStart = _edgesByStart.TryGetValue(start, out var byStart) 
                ? _edgesByStart.SetItem(start, byStart.Add(end)) 
                : _edgesByStart.Add(start, ImmutableArray<int>.Empty.Add(end));

            var edgesByEnd = _edgesByEnd.TryGetValue(end, out var byEnd) 
                ? _edgesByEnd.SetItem(end, byEnd.Add(start)) 
                : _edgesByEnd.Add(end, ImmutableArray<int>.Empty.Add(start));

            var falseEdges = label ? _falseEdges : _falseEdges.Add((start, end));

            return new ControlFlowGraph(
                _headIndex,
                Nodes,
                edgesByStart,
                edgesByEnd,
                falseEdges,
                _deletedNodes,
                _deletedNodeCount);
        }

        public ControlFlowGraph ReplaceNode(int index, ICfgNode newNode) =>
            new(_headIndex,
                Nodes.SetItem(index, newNode),
                _edgesByStart, _edgesByEnd, _falseEdges, 
                _deletedNodes, _deletedNodeCount);

        public ControlFlowGraph RemoveNode(int index) => RemoveNodes(new[] { index });
        public ControlFlowGraph RemoveNodes(IEnumerable<int> indices)
        {
            if (indices == null) throw new ArgumentNullException(nameof(indices));
            var nodes = Nodes.ToBuilder();
            var byStart = _edgesByStart.ToBuilder();
            var byEnd = _edgesByEnd.ToBuilder();
            var falseEdges = _falseEdges.ToBuilder();
            var deletedNodes = _deletedNodes;
            int deletedCount = _deletedNodeCount;

            foreach (var i in indices.OrderByDescending(x => x))
            {
                if (_headIndex == i) throw new InvalidOperationException($"Tried to remove the head node ({i})");
                if (nodes[i] == null) throw new InvalidOperationException($"Tried to remove a non-existent node ({i})");

                nodes[i] = null;
                deletedNodes = deletedNodes.Push(i);
                deletedCount++;
                BuilderRemoveEdges(i, byStart, byEnd, falseEdges);
            }

            return new ControlFlowGraph(
                _headIndex,
                nodes.ToImmutable(),
                byStart.ToImmutable(),
                byEnd.ToImmutable(),
                falseEdges.ToImmutable(),
                deletedNodes,
                deletedCount);
        }

        static void BuilderAddEdge(int start, int end, bool label,
            ImmutableDictionary<int, ImmutableArray<int>>.Builder byStart,
            ImmutableDictionary<int, ImmutableArray<int>>.Builder byEnd,
            ImmutableHashSet<(int start, int end)>.Builder falseEdges)
        {
            byStart[start] = byStart.TryGetValue(start, out var starts)
                ? starts.Add(end)
                : ImmutableArray<int>.Empty.Add(end);

            byEnd[end] = byEnd.TryGetValue(end, out var ends) 
                ? ends.Add(start) 
                : ImmutableArray<int>.Empty.Add(start);

            if (!label)
                falseEdges.Add((start, end));
        }

        static void BuilderRemoveEdges(int i, 
            ImmutableDictionary<int, ImmutableArray<int>>.Builder byStart,
            ImmutableDictionary<int, ImmutableArray<int>>.Builder byEnd,
            ImmutableHashSet<(int start, int end)>.Builder falseEdges)
        {
            void RemoveHelper(int start, int end)
            {
                var newEnds = byStart[start].Remove(end);
                if (newEnds.IsEmpty) byStart.Remove(start);
                else byStart[start] = newEnds;

                var newStarts = byEnd[end].Remove(start);
                if (newStarts.IsEmpty) byEnd.Remove(end);
                else byEnd[end] = newStarts;

                falseEdges.Remove((start, end));
            }

            if (byStart.TryGetValue(i, out var ends))
                foreach (var end in ends)
                    RemoveHelper(i, end);

            if (byEnd.TryGetValue(i, out var starts))
                foreach (var start in starts)
                    RemoveHelper(start, i);
        }

        public ControlFlowGraph Defragment()
        {
            if ((double)_deletedNodeCount / Nodes.Count < DefragThreshold)
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

            return new ControlFlowGraph(mapping[_headIndex], Nodes.Where(x => x != null), edges);
        }

        public ControlFlowGraph RemoveEdge(int start, int end)
        {
            var byStart = _edgesByStart;
            var byEnd = _edgesByEnd;
            var falseEdges = _falseEdges.Remove((start, end));

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

            return new ControlFlowGraph(_headIndex, Nodes, byStart, byEnd, falseEdges, _deletedNodes, _deletedNodeCount);
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

        int GetEntryNode()
        {
            // Should be the only node with indegree 0
            int result = -1;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null || !Parents(i).IsEmpty)
                    continue;
                if (result != -1)
                    throw new InvalidOperationException("Multiple entry nodes were found in control flow graph!");
                result = i;
            }

            return result;
        }

        int GetExitNode()
        {
            // Should be the only node with outdegree 0
            int result = -1;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null || !Children(i).IsEmpty)
                    continue;
                if (result != -1)
                    throw new InvalidOperationException("Multiple exit nodes were found in control flow graph!");
                result = i;
            }

            return result;
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
            if (!postOrder)
                results.Add(index);

            foreach (int i in Children(index))
            {
                if (!visited[i])
                    DepthFirstSearch(i, visited, stack, results, backEdges, postOrder);
                else if (stack.Contains(i))
                    backEdges.Add((index, i));
            }

            stack.Remove(index);
            if (postOrder)
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

            DepthFirstSearch(_headIndex, visited, stack, results, backEdges, postOrder);
#if DEBUG
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null || visited[i]) continue;
                stack.Clear();
                DepthFirstSearch(i, visited, stack, results, backEdges, postOrder);
                throw new InvalidOperationException("Disconnected graph found during depth-first sort!");
            }
#endif

            return (results.ToImmutableArray(), backEdges.ToImmutableArray());
        }

        public TreeNode<int> GetPostDominatorTree() => Reverse().GetDominatorTree();
        public TreeNode<int> GetDominatorTree()
        {
            // TODO: Use more efficient Lengauer-Tarjan algorithm if required
            if (_cachedDominatorTree != null)
                return _cachedDominatorTree;

            if (Parents(_headIndex).Any())
                throw new InvalidOperationException("Tried to obtain dominator tree of graph, but the head was not parentless.");

            var dominators = new List<int>[Nodes.Count];
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null)
                    continue;

                dominators[i] = new List<int>(Nodes.Count);
                if (i == _headIndex)
                    dominators[i].Add(i);
                else
                    for (int j = 0; j < Nodes.Count; j++)
                        dominators[i].Add(j); // Start off with everything dominating everything then remove the ones that aren't
            }

            var postOrder = GetDfsPostOrder();
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach(var index in postOrder)
                {
                    if (index == _headIndex)
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

            TreeNode<int> tree = null;
            foreach (List<int> list in dominators)
            {
                if (list == null)
                    continue;

                // The dominator lists could be out of order, need to make sure.
                list.Sort((x, y) => x == y ? 0 : dominators[y].Contains(x) ? -1 : 1);
                tree = TreeNode<int>.AddPath(tree, list, (x, y) => x == y);
            }

            _cachedDominatorTree = tree;
            return tree;
        }

        static bool ListEquals(List<int> x, List<int> y) => x.Count == y.Count && !x.Except(y).Any();

        public void ExportToDot(StringBuilder sb)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            sb.AppendLine("digraph G {");
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null) continue;
                sb.Append("    Node"); sb.Append(i);
                sb.Append(" [shape=box, fontname = \"Consolas\", fontsize=8, fillcolor=azure2, style=filled, label=\"Node "); sb.Append(i);
                sb.Append("\\l"); sb.Append(Nodes[i]);
                sb.AppendLine("\"];");
            }

            foreach (var kvp in _edgesByStart)
            {
                var start = kvp.Key;
                foreach (var end in kvp.Value)
                {
                    sb.Append("    Node"); sb.Append(start);
                    sb.Append(" -> Node"); sb.Append(end);
                    // sb.Append("[label=\""); sb.Append(edgeLabel); sb.Append("\"");
                    // sb.Append((isBackEdge ? "color=\"1 1 .7\"" : ""));
                    sb.AppendLine(" [];");
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

            return new ControlFlowGraph(mapping[_headIndex], nodes, edges);
        }

        public List<List<int>> GetAllStronglyConnectedComponents()
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
            var headers = new List<int>();
            var backEdges = GetBackEdges().ToHashSet();

            foreach ((int start, int end) e in Edges)
                if (backEdges.Contains(e) && !headers.Contains(e.end))
                    headers.Add(e.end);

            var cycles = GetAllSimpleCyclePaths(component);
            foreach (int header in headers)
            {
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
                header = new LoopPart(header.Index, true, Break: true);
                exits.Add(child);
            }

            for (int i = 1; i < nodes.Count; i++)
            {
                bool isContinue = false;
                bool isBreak = false;
                bool isTail = true;

                foreach (int child in Children(nodes[i]))
                {
                    if (child == header.Index)
                        isContinue = true;
                    else if (nodes.Contains(child))
                        isTail = false;
                    else
                    {
                        isBreak = true;
                        exits.Add(child);
                    }
                }

                bool hasOutsideEntry = Enumerable.Any(Parents(nodes[i]), x => !nodes.Contains(x));
                body.Add(new LoopPart(nodes[i], false, isTail, isBreak, isContinue, hasOutsideEntry));
            }

            return new CfgLoop(header, body, exits.Count > 1);
        }

        public ImmutableArray<CfgLoop> GetLoops()
        {
            if (_cachedLoops != null)
                return _cachedLoops;

            var components = GetAllStronglyConnectedComponents();
            var loops = new List<CfgLoop>();
            foreach (var component in components)
            {
                if (component.Count <= 1) continue;
                foreach (var loop in GetAllSimpleLoops(component))
                    loops.Add(GetLoopInformation(loop));
            }

            _cachedLoops = loops.ToImmutableArray();
            return _cachedLoops;
        }

        public List<List<int>> GetAllReachingPaths(int idx)
        {
            var result = new List<List<int>>();
            GetReachingPath(idx, 0, new List<int>(), result);
            return result;
        }

        public List<List<int>> GetAllSeseRegions()
        {
            TreeNode<int> domTree = GetDominatorTree();
            TreeNode<int> postDomTree = GetPostDominatorTree();
            List<List<int>> result = new List<List<int>>();
            static bool eq(int x, int y) => x == y;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] == null) continue;
                for (int j = 0; j < Nodes.Count; j++)
                {
                    if (Nodes[j] == null) continue;
                    if (i == j) continue;
                    if (!domTree.Dominates(i, j, eq)) continue;
                    if (!postDomTree.Dominates(j, i, eq)) continue;
                    if (Children(j).Length >= 2) continue;
                    if (Parents(i).Length >= 2) continue;

                    List<int> region = new List<int>();
                    GetRegionParts(i, region, j);
                    region.Add(j);
                    result.Add(region);
                }
            }
            return result;
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
                    if (visited[i]) 
                        continue;

                    if (Edges.Any(e => e.end == i && !visited[e.start])) 
                        continue;

                    visited[i] = true;
                    result.Add(i);
                    found = true;
                    break;
                }
            }

            return result;
        }

        public ControlFlowGraph GetCutOut(List<int> selectedNodes)
        {
            if (selectedNodes == null) throw new ArgumentNullException(nameof(selectedNodes));
            List<ICfgNode> resultNodes = new();
            List<(int, int, bool)> resultEdges = new();
            var mapping = new int[Nodes.Count];

            foreach (int index in selectedNodes)
            {
                mapping[index] = resultNodes.Count;
                resultNodes.Add(Nodes[index]);
            }

            foreach (var edge in Edges)
                if (selectedNodes.Contains(edge.start) && selectedNodes.Contains(edge.end))
                    resultEdges.Add((mapping[edge.start], mapping[edge.end], GetEdgeLabel(edge.start, edge.end)));

            // TODO
            return new ControlFlowGraph(-1, resultNodes, resultEdges);
        }

        public void GetReachingPath(int target, int current, List<int> path, List<List<int>> stack)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (stack == null) throw new ArgumentNullException(nameof(stack));

            path.Add(current);
            var children = Children(current);
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

                GetReachingPath(target, child, npath, stack);
            }
        }

        public void GetRegionParts(int current, List<int> stack, int end)
        {
            if (stack == null) throw new ArgumentNullException(nameof(stack));
            if (stack.Contains(current)) return;

            stack.Add(current);
            foreach (var child in Children(current).Where(child => child != end))
                GetRegionParts(child, stack, end);
        }

        public IEnumerable<int> GetBranchNodes()
            => _edgesByStart
               .Where(x => x.Value.Length > 1)
               .Select(x => x.Key);

        public IEnumerable<int> GetNonBranchNodes() 
            => _edgesByStart
                .Where(x => x.Value.Length <= 1)
                .Select(x => x.Key);
    }
}

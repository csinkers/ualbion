using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Scripting.Ast;

namespace UAlbion.Scripting
{
    public class EventLayout
    {
        readonly IList<ControlFlowGraph> _graphs;
        readonly Dictionary<ushort, (int, int)> _indexToNode = new();
        readonly Dictionary<(int, int), ushort> _nodeToIndex = new();

        EventLayout(IList<ControlFlowGraph> graphs)
        {
            _graphs = graphs ?? throw new ArgumentNullException(nameof(graphs));
        }

        public List<EventNode> Events { get; } = new();
        public List<ushort> Chains { get; } = new();
        public List<ushort> ExtraEntryPoints { get; } = new();

        public static EventLayout Build(IList<ControlFlowGraph> graphs)
        {
            var layout = new EventLayout(graphs);
            layout.AssertGraphsFullyReduced();
            var labels = layout.ExtractLabels();

            // Layout fixed events ("EventXX" labels)
            foreach (var (label, gi, ni) in labels)
            {
                var ei = GetLabelNumber(ScriptConstants.FixedEventPrefix, label);
                if (!ei.HasValue)
                    continue;

                layout.AddEntryPoint(ei.Value, gi, ni);
            }

            // Loop through chains ("ChainXX" labels) and layout in true-first order
            ushort eventIndex = 0;
            var chains = GetChains(labels);
            for (int chainIndex = 0; chainIndex < chains.Count; chainIndex++)
            {
                if (chains[chainIndex] == null)
                    continue;

                var (gi, ni) = chains[chainIndex].Value;
                layout.SetChain(chainIndex, gi, ni, ref eventIndex);
            }

            // Layout any unreferenced events
            for (var gi = 0; gi < graphs.Count; gi++)
            {
                var graph = graphs[gi];
                for (var ni = 0; ni < graph.Nodes.Count; ni++)
                {
                    var node = graph.Nodes[ni];
                    if (node == null || layout.IsNodeHandled(gi, ni))
                        continue;

                    layout.Arrange(gi, ni, ref eventIndex);
                }
            }

            layout.LinkNodes();
            layout.ExtraEntryPoints.Sort();
            return layout;
        }


        bool IsEventFree(ushort index) => !_indexToNode.ContainsKey(index);
        bool IsNodeHandled(int graphIndex, int nodeIndex) => _nodeToIndex.ContainsKey((graphIndex, nodeIndex));

        void Set(ushort index, int graphIndex, int nodeIndex)
        {
            if (!IsEventFree(index))
                throw new InvalidOperationException($"Tried to overwrite event {index} with ({graphIndex},{nodeIndex}) but it already contains {_indexToNode[index]}");

            if (IsNodeHandled(graphIndex, nodeIndex))
                return;

            var node = _graphs[graphIndex].Nodes[nodeIndex];
            if (node is EmptyNode)
                return;

            var eventNode = (SingleEvent)node;
            _indexToNode[index] = (graphIndex, nodeIndex);
            _nodeToIndex[(graphIndex, nodeIndex)] = index;

            while (Events.Count <= index)
                Events.Add(null);

            Events[index] = 
                eventNode.Event is IBranchingEvent branch
                ? new BranchNode(index, branch)
                : new EventNode(index, eventNode.Event);
        }

        void AddEntryPoint(ushort eventId, int graphIndex, int nodeIndex)
        {
            Set(eventId, graphIndex, nodeIndex);
            ExtraEntryPoints.Add(eventId);
        }

        void SetChain(int chainIndex, int graphIndex, int nodeIndex, ref ushort eventIndex)
        {
            while (Chains.Count <= chainIndex)
                Chains.Add(0xffff);

            Chains[chainIndex] = Arrange(graphIndex, nodeIndex, ref eventIndex);
        }

        List<(string, int, int)> ExtractLabels()
        {
            var labels = new List<(string, int, int)>();
            for (var graphIndex = 0; graphIndex < _graphs.Count; graphIndex++)
            {
                var graph = _graphs[graphIndex];
                for (var nodeIndex = 0; nodeIndex < graph.Nodes.Count;)
                {
                    var node = graph.Nodes[nodeIndex] as Label;
                    if (node == null)
                    {
                        nodeIndex++;
                        continue;
                    }

                    int target = nodeIndex;
                    while (graph.Nodes[target] is Label)
                    {
                        var children = graph.Children(target);
                        if (children.Length != 1)
                            throw new ControlFlowGraphException($"Label {graph.Nodes[target]} ({target}) does not have a single child", graph);

                        target = children[0];
                    }

                    labels.Add((node.Name, graphIndex, target));

                    var parents = graph.Parents(nodeIndex);
                    foreach (var parent in parents)
                        graph = graph.AddEdge(parent, graph.Children(nodeIndex)[0], graph.GetEdgeLabel(parent, nodeIndex));
                    graph = graph.RemoveNode(nodeIndex);
                }

                _graphs[graphIndex] = graph;
            }

            return labels;
        }

        void AssertGraphsFullyReduced()
        {
            foreach (var graph in _graphs)
            {
                for (var index = 0; index < graph.Nodes.Count; index++)
                {
                    var node = graph.Nodes[index];
                    if (node is null or SingleEvent or Label or EmptyNode)
                        continue;

                    throw new ControlFlowGraphException(
                        $"Tried to lay out a graph containing a non-event node ({index}): {node} ({node.GetType().Name})",
                        graph);
                }
            }
        }

        void LinkNodes()
        {
            for (ushort ei = 0; ei < Events.Count; ei++)
            {
                var e = Events[ei];
                if (e == null)
                    continue;

                var (gi, ni) = _indexToNode[ei];
                var graph = _graphs[gi];
                var exitNode = graph.GetExitNode();
                var (trueChild, falseChild) = graph.GetBinaryChildren(ni);

                if (trueChild == null)
                    throw new ControlFlowGraphException($"Node {ni} had no true child", graph);

                if (trueChild.Value != exitNode)
                    e.Next = Events[_nodeToIndex[(gi, trueChild.Value)]];

                if (e is BranchNode branch)
                {
                    var target = falseChild ?? trueChild.Value;
                    if (target != exitNode)
                        branch.NextIfFalse = Events[_nodeToIndex[(gi, target)]];
                }
            }
        }

        ushort Arrange(int graphIndex, int initialNode, ref ushort eventIndex)
        {
            ushort? initialEventIndex = null;
            var stack = new Stack<int>();
            stack.Push(initialNode);

            while (stack.TryPop(out var nodeIndex))
            {
                if (IsNodeHandled(graphIndex, nodeIndex))
                    continue;

                while (!IsEventFree(eventIndex))
                    eventIndex++;

                initialEventIndex ??= eventIndex;
                Set(eventIndex, graphIndex, nodeIndex);
                var graph = _graphs[graphIndex];
                var (trueChild, falseChild) = graph.GetBinaryChildren(nodeIndex);
                if (trueChild.HasValue) stack.Push(trueChild.Value);
                if (falseChild.HasValue) stack.Push(falseChild.Value);
            }

            return initialEventIndex ?? 0xffff;
        }

        static List<(int, int)?> GetChains(List<(string, int, int)> labels)
        {
            var chains = new List<(int, int)?>();
            foreach (var (label, gi, ni) in labels)
            {
                var chainNumber = GetLabelNumber(ScriptConstants.ChainPrefix, label);
                if (!chainNumber.HasValue)
                    continue;

                while (chains.Count <= chainNumber)
                    chains.Add(null);

                chains[(int)chainNumber] = (gi, ni);
            }

            return chains;
        }

        static ushort? GetLabelNumber(string prefix, string label)
        {
            if (!label.StartsWith(prefix))
                return null;

            var tail = label[prefix.Length..];
            if (!ushort.TryParse(tail, out var eventIndex))
                return null;

            return eventIndex;
        }
    }
}
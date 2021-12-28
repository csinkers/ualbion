using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Scripting;

public static class GraphExtensions
{
    public static int InDegree(this IGraph graph, int node) => graph.Parents(node).Count;
    public static int OutDegree(this IGraph graph, int node) => graph.Children(node).Count;

    public static int GetEntryNode(this IGraph graph)
    {
        // Should be the only node with indegree 0
        int result = -1;
        for (int i = 0; i < graph.NodeCount; i++)
        {
            if (!graph.IsNodeActive(i) || graph.Parents(i).Count != 0)
                continue;
            if (result != -1)
                throw new ControlFlowGraphException("Multiple entry nodes were found in graph!", graph);
            result = i;
            // return result;
        }

        return result;
    }

    public static int GetExitNode(this IGraph graph)
    {
        // Should be the only node with outdegree 0
        int result = -1;
        for (int i = 0; i < graph.NodeCount; i++)
        {
            if (!graph.IsNodeActive(i) || graph.Children(i).Count != 0)
                continue;
            if (result != -1)
                throw new ControlFlowGraphException("Multiple exit nodes were found in graph!", graph);
            result = i;
            // return result;
        }

        return result;
    }

    public static IEnumerable<int> GetExitNodes(this IGraph graph)
    {
        for (int i = 0; i < graph.NodeCount; i++)
        {
            if (!graph.IsNodeActive(i) || graph.Children(i).Count != 0)
                continue;
            yield return i;
        }
    }

    public static void DepthFirstSearch(
        this IGraph graph,
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

        foreach (int i in graph.Children(index))
        {
            if (!visited[i])
                graph.DepthFirstSearch(i, visited, stack, results, backEdges, postOrder);
            else if (stack.Contains(i) && backEdges != null)
                backEdges.Add((index, i));
        }

        stack.RemoveAt(stack.Count - 1);
        if (postOrder && results != null)
            results.Add(index);
    }

    public static List<int> GetDfsOrder(this IGraph graph, int start, bool postOrder)
    {
        var results = new List<int>();
        var visited = new bool[graph.NodeCount];
        var stack = new List<int>();
        graph.DepthFirstSearch(start, visited, stack, results, null, postOrder);
        return results;
    }

    public static List<int> GetTopogicalOrder(this IGraph graph)
    {
        var result = new List<int>();
        var visited = new bool[graph.NodeCount];

        bool found = true;
        while (found)
        {
            found = false;
            for (int i = 0; i < graph.NodeCount; i++)
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

                if (!graph.IsNodeActive(i) || visited[i])
                    continue;

                if (graph.Parents(i).Any(x => !visited[x]))
                    continue; // If we haven't visited all of the parents we can't evaluate this node yet.

                visited[i] = true;
                result.Add(i);
                found = true;
                break;
            }
        }

        if (result.Count != graph.ActiveNodeCount) // Cycle detected
            result.Clear();

        return result;
    }

    public static int[] GetShortestPaths(this IGraph graph, int entry)
    {
        var result = new int[graph.NodeCount];
        Array.Fill(result, int.MaxValue);
        result[entry] = 0;

        foreach (var i in graph.GetTopogicalOrder())
        foreach (var child in graph.Children(i))
            if (result[child] > result[i] + 1)
                result[child] = result[i] + 1;

        return result;
    }

    public static int[] GetLongestPaths(this IGraph graph, int entry)
    {
        var result = new int[graph.NodeCount];
        Array.Fill(result, int.MinValue);
        result[entry] = 0;

        foreach (var i in graph.GetTopogicalOrder())
        foreach (var child in graph.Children(i))
            if (result[child] < result[i] + 1)
                result[child] = result[i] + 1;

        return result;
    }

    public static List<List<int>> GetAllReachingPaths(this IGraph graph, int from, int to)
    {
        var result = new List<List<int>>();
        GetReachingPath(graph, from, to, new List<int>(), result);
        return result;
    }

    static void GetReachingPath(IGraph graph, int start, int target, List<int> path, List<List<int>> stack)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));
        if (stack == null) throw new ArgumentNullException(nameof(stack));

        path.Add(start);
        var children = graph.Children(start);
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

            GetReachingPath(graph, child, target, npath, stack);
        }
    }

    public static DominatorTree GetDominatorTree(this IGraph graph, int entry)
    {
        // TODO: Use more efficient Lengauer-Tarjan algorithm if required
        if (graph.Parents(entry).Any())
            throw new ControlFlowGraphException("Tried to obtain dominator tree of graph, but the entry was not parentless.", graph);

        var dominators = new List<int>[graph.NodeCount];
        for (int i = 0; i < graph.NodeCount; i++)
        {
            if (!graph.IsNodeActive(i))
                continue;

            dominators[i] = new List<int>(graph.NodeCount);
            if (i == entry)
                dominators[i].Add(i);
            else
                for (int j = 0; j < graph.NodeCount; j++)
                    if (graph.IsNodeActive(j))
                        dominators[i].Add(j); // Start off with everything dominating everything then remove the ones that aren't
        }

        var postOrder = graph.GetDfsPostOrder();
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var index in postOrder)
            {
                if (index == entry)
                    continue;

                List<int> currentDominators;
                var parents = graph.Parents(index);
                if (parents.Count > 0)
                {
                    currentDominators = dominators[parents[0]].ToList(); // make a copy to avoid modifying the parent
                    for (var i = 1; i < parents.Count; i++)
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
            if (list[0] != entry)
                throw new ControlFlowGraphException("Disjoint graph detected while computing dominator tree", graph);

            tree = tree.AddPathList(list);
        }

        return tree;
    }

    public static (int[], int componentCount) GetComponentMapping(this IGraph graph)
    {
        int componentIndex = 0;
        var mapping = new int[graph.NodeCount];
        var stack = new Stack<int>();
        Array.Fill(mapping, -1);

        for (int i = 0; i < graph.NodeCount; i++)
        {
            if (!graph.IsNodeActive(i) || mapping[i] != -1)
                continue;

            stack.Push(i);
            while (stack.TryPop(out var current))
            {
                if (mapping[current] != -1)
                    continue;

                mapping[current] = componentIndex;
                foreach (int child in graph.Children(current))
                    stack.Push(child);
                foreach (int child in graph.Parents(current))
                    stack.Push(child);
            }

            componentIndex++;
        }

        return (mapping, componentIndex);
    }

    public static (bool[] reachability, int reachableCount) GetReachability(this IGraph graph, int start)
    {
        int reachableCount = 0;
        var visited = new bool[graph.NodeCount];
        var stack = new Stack<int>();

        stack.Push(start);
        while (stack.TryPop(out var current))
        {
            if (visited[current])
                continue;

            visited[current] = true;
            reachableCount++;
            foreach (int child in graph.Children(current))
                stack.Push(child);
        }

        return (visited, reachableCount);
    }

    public static List<List<int>> GetStronglyConnectedComponents(this IGraph graph)
    {
        List<List<int>> result = new();
        List<int> finished = new();
        var visited = new bool[graph.NodeCount];

        void Inner(int node)
        {
            visited[node] = true;
            var children = graph.Children(node);
            foreach (int child in children)
                if (!visited[child])
                    Inner(child);
            finished.Add(node);
        }

        Array.Clear(visited, 0, visited.Length);
        for (int i = 0; i < graph.NodeCount; i++)
            if (!visited[i])
                Inner(i);

        var reversedEdges = graph.Reverse();
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

    public static List<List<int>> GetAllSimpleCyclePaths(this IGraph graph, IList<int> component)
    {
        if (component == null) throw new ArgumentNullException(nameof(component));
        List<int> stack = new();
        List<int> blocked = new();
        List<List<int>> result = new();
        List<(int, int)> blockMap = new();
        var visited = new bool[graph.NodeCount];

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

            foreach (int n in graph.Children(index))
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
                foreach (int n in graph.Children(index))
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

    public static List<List<int>> GetAllSimpleLoops(this IGraph graph, List<int> component)
    {
        var result = new List<List<int>>();
        var backEdges = graph.GetBackEdges().ToHashSet();
        var headers = backEdges.Select(e => e.Item2).Distinct().ToList();

        var cycles = graph.GetAllSimpleCyclePaths(component);
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

    public static HashSet<int> GetRegionParts(this IGraph graph, int entry, int exit)
    {
        var result = new HashSet<int>();
        GetRegionParts(graph, result, entry, exit);
        return result;
    }

    static void GetRegionParts(IGraph graph, HashSet<int> region, int entry, int exit)
    {
        if (!region.Add(entry))
            return;

        foreach (var child in graph.Children(entry).Where(child => child != exit))
            GetRegionParts(graph, region, child, exit);
    }

    public static List<(HashSet<int> nodes, int entry, int exit)> GetAllSeseRegions(this IGraph graph)
    {
        var domTree = graph.GetDominatorTree();
        var postDomTree = graph.GetPostDominatorTree();
        List<(HashSet<int>, int, int)> regions = new();

        foreach (var i in graph.GetDfsPostOrder())
        {
            for (int j = 0; j < graph.NodeCount; j++)
            {
                if (!graph.IsNodeActive(j)) continue;
                if (i == j) continue;
                if (!domTree.Dominates(i, j)) continue;
                if (!postDomTree.Dominates(j, i)) continue;
                if (graph.Children(j).Count > 1) continue;
                if (graph.Parents(i).Count > 1) continue;

                var region = graph.GetRegionParts(i, j);
                region.Add(j);
                regions.Add((region, i, j));
            }
        }
        return regions;
    }

    static bool ListEquals(List<int> x, List<int> y) => x.Count == y.Count && !x.Except(y).Any();
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UAlbion.Scripting.Ast;
using Xunit;

namespace UAlbion.Scripting.Tests
{
    public static class TestUtil
    {
        static readonly SemaphoreSlim ProcessSemaphore = new(Environment.ProcessorCount, Environment.ProcessorCount);
        public static string FindBasePath()
        {
            var exeLocation = Assembly.GetExecutingAssembly().Location;
            var curDir = new DirectoryInfo(Path.GetDirectoryName(exeLocation) ?? throw new InvalidOperationException());

            while (curDir != null && !File.Exists(Path.Combine(curDir.FullName, "data", "config.json")))
                curDir = curDir.Parent;

            var baseDir = curDir?.FullName;
            return baseDir;
        }

        public static void VerifyAstVsScript(string expected, ICfgNode ast)
        {
            var visitor = new FormatScriptVisitor { PrettyPrint = false, WrapStatements = false };
            ast.Accept(visitor);
            Assert.Equal(expected, visitor.Code);
        }

        public static void VerifyCfgVsScript(
            ControlFlowGraph graph,
            List<(string, IGraph)> steps,
            string expected,
            string resultsDir,
            [CallerMemberName] string method = null)
        {
            if (!CompareCfgVsScript(graph, expected, out var error))
            {
                DumpSteps(steps, resultsDir, method);
                throw new InvalidOperationException(error);
            }
        }

        public static bool CompareCfgVsScript(ControlFlowGraph graph, string expected, out string message)
        {
            if (graph.ActiveNodeCount > 1)
            {
                message = "Result is not fully reduced";
                return false;
            }

            var nodes = graph.GetDfsOrder().Select(x => graph.Nodes[x]);
            return CompareNodesVsScript(nodes, expected, out message);
        }

        static string FormatScript(IEnumerable<ICfgNode> nodes, bool pretty)
        {
            var visitor = new FormatScriptVisitor { PrettyPrint = pretty, WrapStatements = false };
            foreach (var node in nodes)
                node.Accept(visitor);

            return visitor.Code;
        }

        public static bool CompareNodesVsScript(IEnumerable<ICfgNode> nodes, string expected, out string message)
        {
            var compact = FormatScript(nodes, false);
            var pretty = FormatScript(nodes, true);

            message = null;
            var nl = Environment.NewLine;
            if (compact != expected && pretty != expected)
            {
                message = $"Test Failed{nl}Expected:{nl}{expected}{nl}Actual (compact):{nl}{compact}{nl}Actual (pretty printed):{nl}{pretty}{nl}";
                return false;
            }

            return true;
        }

        public static bool CompareLayout(EventLayout actual, EventLayout expected, out string message)
        {
            if (!Compare(expected.Events, actual.Events, out var error))
            {
                message = "Event mismatch: " + error;
                return false;
            }

            if (!Compare(expected.Chains, actual.Chains, out error))
            {
                message = "Chain mismatch: " + error;
                return false;
            }

            if (!Compare(expected.ExtraEntryPoints, actual.ExtraEntryPoints, out error))
            {
                message = "Entry point mismatch " + error;
                return false;
            }

            message = null;
            return true;
        }

        static bool Compare<T>(IList<T> left, IList<T> right, out string error)
        {
            if (left.Count != right.Count)
            {
                error = $"Expected {left.Count}, but there were {right.Count}";
                return false;
            }

            var sb = new StringBuilder();
            for (int i = 0; i < left.Count; i++)
            {
                if (!Equals(left[i], right[i]))
                    sb.AppendLine($"    {i}: expected \"{left[i]}\", received \"{right[i]}\"");
            }

            error = sb.ToString();
            return sb.Length == 0;
        }

        public static bool CompareCfg(ControlFlowGraph cfg, ControlFlowGraph expectedCfg, out string message)
        {
            var actual = Arrange(cfg);
            var expected = Arrange(expectedCfg);

            if (!expected.nodes.SequenceEqual(actual.nodes))
            {
                message = "Node mismatch";
                return false;
            }

            if (!expected.edges.SequenceEqual(actual.edges))
            {
                message = "Edge mismatch";
                return false;
            }

            message = null;
            return true;
        }

        public static (List<ICfgNode> nodes, List<(int, int, CfgEdge)> edges) Arrange(ControlFlowGraph graph)
        {
            var nodes = new List<ICfgNode>();
            var mapping = new int[graph.Nodes.Count];
            Array.Fill(mapping, -1);

            foreach(var entry in graph.GetDfsOrder())
            {
                var stack = new Stack<int>();
                stack.Push(entry);
                while (stack.TryPop(out var nodeIndex))
                {
                    if (mapping[nodeIndex] != -1)
                        continue;

                    mapping[nodeIndex] = nodes.Count;
                    nodes.Add(graph.Nodes[nodeIndex]);

                    var (trueChild, falseChild) = graph.GetBinaryChildren(nodeIndex);
                    if (trueChild.HasValue) stack.Push(trueChild.Value);
                    if (falseChild.HasValue) stack.Push(falseChild.Value);
                }
            }

            var edges = graph.LabelledEdges.Select(x => ( mapping[x.start], mapping[x.end], x.label )).ToList();
            return (nodes, edges);
        }

        public static void DumpSteps(List<(string, IGraph)> steps, string resultsDir, [CallerMemberName] string method = null)
        {
            if (steps == null)
                return;

            if (!Directory.Exists(resultsDir))
                Directory.CreateDirectory(resultsDir);

            var tasks = new List<Task>();
            for (int i = 0; i < steps.Count; i++)
            {
                var (description, graph) = steps[i];
                description = SanitizeForPath(description);
                var path = Path.Combine(resultsDir, $"{method}_{i}_{description}.gv");
                File.WriteAllText(path, graph.ExportToDot());

                tasks.Add(Task.Run(() => FormatGraph(path)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        static string SanitizeForPath(string description)
        {
            var sb = new StringBuilder();
            foreach (var c in description)
            {
                if (c is ' ' or '_' or '-')
                    sb.Append(c);
                else if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else if (c is '(' or ')' or '[' or ']')
                    sb.Append(c);
            }
            return sb.ToString();
        }

        static async Task FormatGraph(string path)
        {
            await ProcessSemaphore.WaitAsync();
            try
            {
                var graphVizDot = @"C:\Program Files\Graphviz\bin\dot.exe";
                if (!File.Exists(graphVizDot))
                    return;

                var pngPath = Path.ChangeExtension(path, "png");
                var args = $"\"{path}\" -T png -o \"{pngPath}\"";
                using var process = Process.Start(graphVizDot, args);
                if (process == null)
                    return;

                await process.WaitForExitAsync();
            }
            finally { ProcessSemaphore.Release(); }
        }
    }
}
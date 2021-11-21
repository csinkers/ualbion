using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        static readonly SemaphoreSlim _processSemaphore = new(Environment.ProcessorCount, Environment.ProcessorCount);
        public static string FindBasePath()
        {
            var exeLocation = Assembly.GetExecutingAssembly().Location;
            var curDir = new DirectoryInfo(Path.GetDirectoryName(exeLocation) ?? throw new InvalidOperationException());

            while (curDir != null && !File.Exists(Path.Combine(curDir.FullName, "data", "config.json")))
                curDir = curDir.Parent;

            var baseDir = curDir?.FullName;
            return baseDir;
        }

        public static void VerifyPseudocode(string expected, ICfgNode ast, [CallerMemberName] string method = null)
        {
            var visitor = new FormatScriptVisitor { PrettyPrint = false };
            ast.Accept(visitor);
            Assert.Equal(expected, visitor.Code);
        }

        public static void Verify(
            ControlFlowGraph graph,
            List<(string, ControlFlowGraph)> steps,
            string expected,
            string resultsDir,
            [CallerMemberName] string method = null)
        {
            var visitor = new FormatScriptVisitor { PrettyPrint = false };
            foreach (var node in graph.GetDfsOrder())
                graph.Nodes[node].Accept(visitor);
            var pseudo = visitor.Code;
            DumpSteps(steps, resultsDir, method);

            Assert.Equal(expected, pseudo);
        }

        public static void DumpSteps(List<(string, ControlFlowGraph)> steps, string resultsDir, string method)
        {
            if (steps == null)
                return;

            if (!Directory.Exists(resultsDir))
                Directory.CreateDirectory(resultsDir);

            var tasks = new List<Task>();
            for (int i = 0; i < steps.Count; i++)
            {
                var (description, graph) = steps[i];
                var path = Path.Combine(resultsDir, $"{method}_{i}_{description}.gv");
                var sb = new StringBuilder();
                graph.ExportToDot(sb);
                File.WriteAllText(path, sb.ToString());

                tasks.Add(Task.Run(() => FormatGraph(path)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        static async Task FormatGraph(string path)
        {
            await _processSemaphore.WaitAsync();
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
            finally { _processSemaphore.Release(); }
        }
    }
}
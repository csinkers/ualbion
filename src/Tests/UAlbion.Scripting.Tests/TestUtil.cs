using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UAlbion.Scripting.Ast;
using Xunit;

namespace UAlbion.Scripting.Tests
{
    static class TestUtil
    {
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
            var visitor = new EmitPseudocodeVisitor { PrettyPrint = false };
            ast.Accept(visitor);
            Assert.Equal(expected, visitor.Code);
        }

        public static void Verify(ControlFlowGraph graph, List<(string, ControlFlowGraph)> steps, string expected, [CallerMemberName] string method = null)
        {
            var visitor = new EmitPseudocodeVisitor { PrettyPrint = false };
            foreach (var node in graph.GetDfsOrder())
                graph.Nodes[node].Accept(visitor);
            var pseudo = visitor.Code;
            DumpSteps(steps, method);

            Assert.Equal(expected, pseudo);
        }

        public static void DumpSteps(List<(string, ControlFlowGraph)> steps, string method)
        {
            if (steps == null)
                return;

            var baseDir = FindBasePath();
            var resultsDir = Path.Combine(baseDir, "re", "DecompilerTests");
            if (!Directory.Exists(resultsDir))
                Directory.CreateDirectory(resultsDir);

            for (int i = 0; i < steps.Count; i++)
            {
                var (description, graph) = steps[i];
                var path = Path.Combine(resultsDir, $"{method}_{i}_{description}.gv");
                var sb = new StringBuilder();
                graph.ExportToDot(sb);
                File.WriteAllText(path, sb.ToString());

                var graphVizDot = @"C:\Program Files\Graphviz\bin\dot.exe";
                if (!File.Exists(graphVizDot))
                    continue;

                var pngPath = Path.ChangeExtension(path, "png");
                var args = $"\"{path}\" -T png -o \"{pngPath}\"";
                using var process = Process.Start(graphVizDot, args);
                process?.WaitForExit();
            }
        }
    }
}
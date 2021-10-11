using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Scripting;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class DecompilerTests
    {
        static void Verify(ICfgNode tree, List<(string, ControlFlowGraph)> steps, string expected, [CallerMemberName] string method = null)
        {
            var sb = new StringBuilder();
            tree.ToPseudocode(sb, true, false);
            var pseudo = sb.ToString();
            //if (pseudo != expected)
            DumpSteps(steps, method);

            expected = FormatUtil.StripWhitespaceForScript(expected);
            pseudo = FormatUtil.StripWhitespaceForScript(pseudo);
            Assert.Equal(expected, pseudo);
        }

        static void Verify(ControlFlowGraph graph, List<(string, ControlFlowGraph)> steps, string expected, [CallerMemberName] string method = null)
        {
            var sb = new StringBuilder();
            foreach (var node in graph.GetDfsOrder())
                graph.Nodes[node].ToPseudocode(sb, true, false);
            var pseudo = sb.ToString();
            //if (pseudo != expected)
            DumpSteps(steps, method);

            expected = FormatUtil.StripWhitespaceForScript(expected);
            pseudo = FormatUtil.StripWhitespaceForScript(pseudo);
            Assert.Equal(expected, pseudo);
        }

        static void DumpSteps(List<(string, ControlFlowGraph)> steps, string method)
        {
            if (steps == null)
                return;

            var disk = new MockFileSystem(true);
            var baseDir = ConfigUtil.FindBasePath(disk);
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

        static void TestSimplify(ControlFlowGraph graph, string expected, [CallerMemberName] string method = null)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var steps = new List<(string, ControlFlowGraph)> { ("Initial", graph) };
            try
            {
                var result = Decompiler.SimplifyGraph(graph,
                    (description, x) =>
                    {
                        if (steps.Count == 0 || steps[^1].Item2 != x)
                            steps.Add((description, x));
                        return x;
                    });

                Verify(result, steps, expected, method);
            }
            catch (ControlFlowGraphException e)
            {
                steps.Add((e.Message, e.Graph));
                DumpSteps(steps, method);
                throw;
            }
            catch
            {
                DumpSteps(steps, method);
                throw;
            }
        }

        static void TestDecompile(string script, string expected, [CallerMemberName] string method = null)
        {
            AssetMapping.GlobalIsThreadLocal = true;
            AssetMapping.Global.RegisterAssetType(typeof(Base.Item), AssetType.Item);
            AssetMapping.Global.RegisterAssetType(typeof(Base.Map), AssetType.Map);
            AssetMapping.Global.RegisterAssetType(typeof(Base.MapText), AssetType.MapText);
            AssetMapping.Global.RegisterAssetType(typeof(Base.Door), AssetType.Door);
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Formats.ScriptEvents.PartyMoveEvent)));

            var events = EventNode.ParseScript(script);
            var steps = new List<(string, ControlFlowGraph)>();
            try
            {
                var result = Decompiler.Decompile(events.Cast<IEventNode>().ToList(), steps);
                Verify(result, steps, expected, method);
            }
            catch (ControlFlowGraphException e)
            {
                steps.Add((e.Message, e.Graph));
                DumpSteps(steps, method);
                throw;
            }
            catch
            {
                DumpSteps(steps, method);
                throw;
            }
        }

        [Fact]
        public void ReduceSequenceTest()
        {
            var result = 
                Decompiler.ReduceSequences(
                    Decompiler.ReduceSequences(TestGraphs.Sequence));
            Verify(result, null, "0; 1; 2; ");
        }

        [Fact]
        public void ReduceIfThenTest()
        {
            var result = Decompiler.ReduceIfThen(TestGraphs.IfThen);
            Verify(result, null, "if (0) { 1; } 2; ");
        }

        [Fact]
        public void ReduceIfThenElseTest()
        {
            var result = Decompiler.ReduceIfThenElse(TestGraphs.IfThenElse);
            Verify(result, null, "if (0) { 1; } else { 2; } 3; ");
        }

        [Fact]
        public void ReduceSimpleWhileTest()
        {
            var result = Decompiler.ReduceSimpleWhile(TestGraphs.SimpleWhileLoop);
            Verify(result, null, "0; while (1) { } 2; ");
        }

        [Fact]
        public void ReduceWhileTest()
        {
            var result = Decompiler.ReduceSimpleLoops(TestGraphs.WhileLoop);
            Verify(result, null, "0; while (1) { 2; } 3; ");
        }

        [Fact]
        public void ReduceDoWhileTest()
        {
            var result = Decompiler.ReduceSimpleLoops(TestGraphs.DoWhileLoop);
            Verify(result, null, "0; do { 1; } while (2); 3; ");
        }

        /*
        [Fact]
        public void ReduceSeseTest()
        {
            var result = Decompiler.ReduceSeseRegions(TestGraphs.ZeroKSese);
            Verify(result, null, "todo");
        }
        */

        [Fact] public void SequenceTest() => TestSimplify(TestGraphs.Sequence, "0; 1; 2; "); 
        [Fact] public void IfThenTest() => TestSimplify(TestGraphs.IfThen, "if (0) { 1; } 2; "); 
        [Fact] public void IfThenElseTest() => TestSimplify(TestGraphs.IfThenElse, "if (0) { 1; } else { 2; } 3; "); 
        [Fact] public void SimpleWhileTest() => TestSimplify(TestGraphs.SimpleWhileLoop, "0; while (1) { } 2; "); 
        [Fact] public void WhileTest() => TestSimplify(TestGraphs.WhileLoop, "0; while (1) { 2; } 3; "); 
        [Fact] public void DoWhileTest() => TestSimplify(TestGraphs.DoWhileLoop, "0; do { 1; } while (2); 3; "); 
        // [Fact] public void ZeroKSeseTest() => TestSimplify(TestGraphs.ZeroKSese, "0; do { 2; } while (1); 3; "); 
        // [Fact] public void NoMoreGotos3Test() => TestSimplify(TestGraphs.NoMoreGotos3, "something"); 
        // [Fact] public void NoMoreGotos3Region1Test() => TestSimplify(TestGraphs.NoMoreGotos3Region1, TestGraphs.NoMoreGotos3Region1Code); 
        // [Fact] public void NoMoreGotos3Region2Test() => TestSimplify(TestGraphs.NoMoreGotos3Region2, TestGraphs.NoMoreGotos3Region2Code); 
        // [Fact] public void NoMoreGotos3Region3Test() => TestSimplify(TestGraphs.NoMoreGotos3Region3, TestGraphs.NoMoreGotos3Region3Code); 
        // [Fact] public void LoopBranchTest() => TestSimplify(TestGraphs.LoopBranch, TestGraphs.LoopBranchCode);
        [Fact] public void BreakBranchTest() => TestSimplify(TestGraphs.BreakBranch, TestGraphs.BreakBranchCode);
        [Fact] public void BreakBranch2Test() => TestSimplify(TestGraphs.BreakBranch2, TestGraphs.BreakBranch2Code);

        [Fact]
        public void DecompileTest()
        {
            const string script = 
@"!0?1:2: query_verb IsTrue 0 Examine
 1=>!: map_text MapText.Jirinaar 37 NoPortrait None // ""The door to the house of the Hunter Clan. It is secured with a lock.""
!2?3:!: inv:door Door.HunterClanFrontDoor MapText.Jirinaar Item.HunterClanKey 100 32 33
!3?4:!: query_previous_action_result IsTrue 0 0
 4=>5: modify_unk2 0 0 0 0 101 0
 5=>!: teleport Map.HunterClan 69 67 Unchanged 255 0";

            string expected = FormatUtil.StripWhitespaceForScript(
@"if (query_verb IsTrue 0 Examine) {
    map_text MapText.Jirinaar 37 NoPortrait None;
}
else {
    if (inv:door Door.HunterClanFrontDoor MapText.Jirinaar Item.HunterClanKey 100 32 33) {
        if (query_previous_action_result IsTrue 0 0) {
            modify_unk2 0 0 0 0 101 0;
            teleport Map.HunterClan 69 67 Unchanged 255 0;
        }
    }
}");
            TestDecompile(script, expected);
        }
    }
}
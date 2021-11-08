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
using UAlbion.Scripting;
using UAlbion.Scripting.Ast;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Formats.Tests
{
    public class AlbionDecompilerTests
    {
        static void Verify(ICfgNode tree, List<(string, ControlFlowGraph)> steps, string expected, [CallerMemberName] string method = null)
        {
            var visitor = new EmitPseudocodeVisitor();
            tree.Accept(visitor);
            DumpSteps(steps, method);

            Assert.Equal(expected, visitor.Code);
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

        static void TestDecompile(string script, string expected, [CallerMemberName] string method = null)
        {
            AssetMapping.GlobalIsThreadLocal = true;
            AssetMapping.Global.RegisterAssetType(typeof(Base.Item), AssetType.Item);
            AssetMapping.Global.RegisterAssetType(typeof(Base.Map), AssetType.Map);
            AssetMapping.Global.RegisterAssetType(typeof(Base.MapText), AssetType.MapText);
            AssetMapping.Global.RegisterAssetType(typeof(Base.Door), AssetType.Door);
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(ScriptEvents.PartyMoveEvent)));

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
        public void DecompileTest()
        {
            const string script = 
@"!0?1:2: query_verb Examine
 1=>!: map_text MapText.Jirinaar 37 NoPortrait None ; ""The door to the house of the Hunter Clan. It is secured with a lock.""
!2?3:!: open_door Door.HunterClan MapText.Jirinaar Item.HunterClanKey 100 32 33
!3?4:!: result
 4=>5: modify_unk2 0 0 0 0 101 0
 5=>!: teleport Map.HunterClan 69 67 Unchanged 255 0";

            string expected = 
@"if (query_verb Examine) {
    map_text MapText.Jirinaar 37 NoPortrait
} else {
    if (open_door Door.HunterClan MapText.Jirinaar Item.HunterClanKey 100 32 33) {
        if (result) {
            modify_unk2 0 0 0 0 101 0
            teleport Map.HunterClan 69 67 Unchanged 255 0
        }
    }
}";
            TestDecompile(script, expected);
        }
    }
}

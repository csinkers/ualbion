using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Formats.MapEvents;
using UAlbion.Game;
using UAlbion.Game.Settings;
using UAlbion.Scripting;
using UAlbion.Scripting.Tests;
using UAlbion.TestCommon;
using Xunit;

namespace UAlbion.Base.Tests
{
    public class FullDecompilationTests : IDisposable
    {
        static readonly string ResultsDir = Path.Combine(TestUtil.FindBasePath(), "re", "FullDecomp");
        static int s_testNum;

        static readonly IJsonUtil JsonUtil = new FormatJsonUtil();
        static readonly CoreConfig CoreConfig;
        static readonly GeneralConfig GeneralConfig;
        static readonly GameConfig GameConfig;
        static readonly GeneralSettings Settings;
        readonly int _testNum;

        static FullDecompilationTests()
        {
            var disk = new MockFileSystem(true);
            var baseDir = ConfigUtil.FindBasePath(disk);
            GeneralConfig = AssetSystem.LoadGeneralConfig(baseDir, disk, JsonUtil);
            CoreConfig = new CoreConfig();
            GameConfig = AssetSystem.LoadGameConfig(baseDir, disk, JsonUtil);
            Settings = new GeneralSettings
            {
                ActiveMods = { "Base" },
                Language = Language.English
            };
        }

        public FullDecompilationTests()
        {
            Event.AddEventsFromAssembly(typeof(ActionEvent).Assembly);
            AssetMapping.GlobalIsThreadLocal = true;
            AssetMapping.Global.Clear();
            _testNum = Interlocked.Increment(ref s_testNum);
            PerfTracker.StartupEvent($"Start decompilation test {_testNum}");
        }
        public void Dispose()
        {
            PerfTracker.StartupEvent($"Finish decompilation test {_testNum}");
        }

        [Fact] public void Set1() => TestEventSet(new EventSetId(AssetType.EventSet, 1));
        [Fact] public void Map110() => TestMap(new MapId(AssetType.Map, 110));

        void TestMap(MapId id)
        {
            var map = Load(x => x.LoadMap(id));
            var npcRefs = map.Npcs.Where(x => x.Node != null).Select(x => x.Node.Id).ToHashSet();
            var zoneRefs = map.Zones.Where(x => x.Node != null).Select(x => x.Node.Id).ToHashSet();
            var refs = npcRefs.Union(zoneRefs).Except(map.Chains);

            TestInner(
                map.Events,
                map.Chains,
                refs);
        }

        void TestEventSet(EventSetId id)
        {
            var set = Load(x => x.LoadEventSet(id));
            TestInner(set.Events, set.Chains, Array.Empty<ushort>());
        }

        void TestInner<T>(
            IList<T> events,
            IEnumerable<ushort> chains,
            IEnumerable<ushort> entryPoints,
            [CallerMemberName] string testName = null) where T : IEventNode
        {
            var graphs = Decompiler.BuildEventRegions(events, chains, entryPoints);
            foreach (var graph in graphs)
            {
                var decompiled = Decompile(graph, testName);
                var visitor = new EmitPseudocodeVisitor();
                decompiled.Head.Accept(visitor);
                var script = visitor.Code;

                var (roundTripEvents, roundTripChains) = ScriptCompiler.Compile(script);
                var (expectedEvents, expectedChains) = ScriptCompiler.LayoutGraph(graph);
                if (!expectedEvents.SequenceEqual(roundTripEvents))
                    throw new InvalidOperationException();

                if (!expectedChains.SequenceEqual(roundTripChains))
                    throw new InvalidOperationException();
            }
        }

        static ControlFlowGraph Decompile(ControlFlowGraph graph, string testName)
        {
            var steps = new List<(string, ControlFlowGraph)>();

            ControlFlowGraph Record(string d, ControlFlowGraph g)
            {
                steps.Add((d, g));
                return g;
            }

            try
            {
                return Decompiler.SimplifyGraph(graph, Record);
            }
            catch(ControlFlowGraphException ex)
            {
                steps.Add((ex.Message, ex.Graph));
                TestUtil.DumpSteps(steps, ResultsDir, testName);
                throw;
            }
        }

        static T Load<T>(Func<IAssetManager, T> func)
        {
            var disk = new MockFileSystem(true);
            var exchange = AssetSystem.Setup(disk, JsonUtil, GeneralConfig, Settings, CoreConfig, GameConfig);

            var assets = exchange.Resolve<IAssetManager>();
            var result = func(assets);
            Assert.NotNull(result);

            return result;
        }
    }
}

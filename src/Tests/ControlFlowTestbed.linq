<Query Kind="Program">
  <Reference Relative="..\..\build\UAlbion.Formats\bin\Debug\net5.0\SerdesNet.dll">C:\Depot\bb\ualbion\build\UAlbion.Formats\bin\Debug\net5.0\SerdesNet.dll</Reference>
  <Reference Relative="..\..\build\UAlbion.Scripting\bin\Debug\net5.0\Superpower.dll">C:\Depot\bb\ualbion\build\UAlbion.Scripting\bin\Debug\net5.0\Superpower.dll</Reference>
  <Reference Relative="..\..\build\UAlbion.Scripting\bin\Debug\net5.0\UAlbion.Api.dll">C:\Depot\bb\ualbion\build\UAlbion.Scripting\bin\Debug\net5.0\UAlbion.Api.dll</Reference>
  <Reference Relative="..\..\build\UAlbion.Formats\bin\Debug\net5.0\UAlbion.Base.dll">C:\Depot\bb\ualbion\build\UAlbion.Formats\bin\Debug\net5.0\UAlbion.Base.dll</Reference>
  <Reference Relative="..\..\build\UAlbion.Formats\bin\Debug\net5.0\UAlbion.Config.dll">C:\Depot\bb\ualbion\build\UAlbion.Formats\bin\Debug\net5.0\UAlbion.Config.dll</Reference>
  <Reference Relative="..\..\build\UAlbion.Formats\bin\Debug\net5.0\UAlbion.Formats.dll">C:\Depot\bb\ualbion\build\UAlbion.Formats\bin\Debug\net5.0\UAlbion.Formats.dll</Reference>
  <Reference Relative="..\..\build\UAlbion.Scripting\bin\Debug\net5.0\UAlbion.Scripting.dll">C:\Depot\bb\ualbion\build\UAlbion.Scripting\bin\Debug\net5.0\UAlbion.Scripting.dll</Reference>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>UAlbion.Api</Namespace>
  <Namespace>UAlbion.Scripting</Namespace>
  <Namespace>UAlbion.Scripting.Ast</Namespace>
  <Namespace>UAlbion.Config</Namespace>
</Query>

void Main()
{
	/*Event.AddEventsFromAssembly(GetType().Assembly);
	Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Api.Event)));
	Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Formats.ScriptEvents.PartyMoveEvent)));
	AssetMapping.Global.RegisterAssetType(typeof(UAlbion.Base.Switch), AssetType.Switch);
	AssetMapping.Global.RegisterAssetType(typeof(UAlbion.Base.EventText), AssetType.EventText);
	AssetMapping.Global.RegisterAssetType(typeof(UAlbion.Base.Word), AssetType.Word);
	AssetMapping.Global.RegisterAssetType(typeof(UAlbion.Base.PartyMember), AssetType.Party);
	AssetMapping.Global.RegisterAssetType(typeof(UAlbion.Base.Script), AssetType.Script);
	//*/
	/*RegionTest(@"0=>1: test 0
1=>2: test 1
2=>4: test 2
3=>4: test 3
4=>5: test 4
5=>!: test 5", "0 3"); */// Multiple chains
	/*RegionTest(@"0=>1: test 0
1=>2: test 1
2=>3: test 2
3=>4: test 3
4=>5: test 4
5=>!: test 5", "0 3"); // Overlapping chains */
	DumpSteps(TestGraphs.MultiBreak, 20);
	/*const string Foo = @"";
	var events = EventNode.ParseRawEvents(Foo);
	//BuildGraphDumper(Decompiler.BuildDisconnectedGraphFromEvents(events), "foo").Dump();
	var graphs = Decompiler.BuildEventRegions(events, new ushort[] { }, null);
	foreach(var graph in graphs) BuildGraphDumper(graph, "foo").Dump(); */
}

public static ControlFlowGraph BuildDisconnectedGraphFromEvents<T>(IList<T> events) where T : IEventNode
{
	var nodes = events.Select(x => (ICfgNode)Emit.Event(x.Event)).ToList();

	// Add empty nodes for the unique entry/exit points
	var entry = nodes.Count;
	nodes.Add(Emit.Empty());
	var exit = nodes.Count;
	nodes.Add(Emit.Empty());

	var trueEdges = events.Where(x => x.Next != null).Select(x => ((int)x.Id, (int)x.Next.Id, CfgEdge.True));
	var falseEdges = events.OfType<IBranchNode>().Where(x => x.NextIfFalse != null).Select(x => ((int)x.Id, (int)x.NextIfFalse.Id, CfgEdge.False));
	var edges = trueEdges.Concat(falseEdges);
	return new ControlFlowGraph(entry, exit, nodes, edges);
}

static void RegionTest(string script, string chainsString, string additionalEntryPointsString = null)
{
	var events = EventNode.ParseRawEvents(script);
	var chains = chainsString.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(ushort.Parse);
	var additional = additionalEntryPointsString?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(ushort.Parse);
	var results = Decompiler.BuildEventRegions(events, chains, additional);
	for (int i = 0; i < results.Count; i++)
	{
		DumpSteps(results[i], 20);
		// BuildGraphDumper(results[i], $"Result{i}").Dump();
	}
}

static void DumpSteps(string graph, int count) => DumpSteps(ControlFlowGraph.FromString(graph), count);
static void DumpSteps(ControlFlowGraph graph, int count, [CallerArgumentExpression("graph")] string graphExpression = null)
{
	var steps = new List<(string, IGraph)>();
	steps.Add(($"Initialise {graphExpression}", graph));
	int i = 1;
	try
	{
		for (; i <= count; i++)
		{
			var last = graph;
			string description = "Nothing";
			RecordFunc recordFunc = (desc, x) =>
			{
				if (x == last) return x;
				last = x;
				description = desc;
				return x;
			};

			graph = Decompiler.SimplifyOnce(graph, recordFunc);
			if (description == "Nothing")
				break;

			steps.Add((description, graph));
		}
	}
	catch (ControlFlowGraphException e) { steps.Add((e.Message, e.Graph)); }

	steps.Select(x => BuildGraphDumper((ControlFlowGraph)x.Item2, x.Item1)).Dump();
}
static T NullOnError<T>(Func<T> func)
{
	try { return func(); }
	catch { return default;}
}

static string FormatInts(IEnumerable<int> ints) => string.Join(" ", ints.Select(x => x.ToString()));
static string JoinLines(IEnumerable<string> lines) => string.Join(Environment.NewLine, lines);
static object BuildTreeDumper(DominatorTree dominatorTree) => new GraphDumper(() => dominatorTree.ExportToDot(75));
static object BuildGraphDumper(ControlFlowGraph graph, string description) => new
{
	Result = new
	{
		Description = description,
		Graph = new GraphDumper(() => graph.ExportToDot(true, 120)),
		AsString = graph?.Defragment(true)?.ToString(),
		Code = graph == null ? null : GetCode(graph)
	},
	Info = new
	{
		Dominator = NullOnError(() => BuildTreeDumper(graph.GetDominatorTree())),
		PostDominator = NullOnError(() => BuildTreeDumper(graph.GetPostDominatorTree())),
		Loops = NullOnError(() => graph.GetLoops()),
		BackEdges = NullOnError(() => graph.GetBackEdges())
	},
	Extra = new
	{
		Topo = NullOnError(() => FormatInts(graph.GetTopogicalOrder())),
		PostDFS = NullOnError(() => FormatInts(graph.GetDfsPostOrder())),
		SCC = NullOnError(() => graph.GetStronglyConnectedComponents()
			  .Where(x => x.Count > 1)
			  .Select(x => new
			  {
				  Component = FormatInts(x),
				  SimpleCycles = JoinLines(graph.GetAllSimpleCyclePaths(x).Select(FormatInts)),
				  SimpleLoops = JoinLines(graph.GetAllSimpleLoops(x).Select(FormatInts)),
			  })),
		SESE = NullOnError(() => graph.GetAllSeseRegions().Select(x => new
		{
			Nodes = FormatInts(x.nodes),
			Entry = x.entry,
			Exit = x.exit,
			Reaching = x.nodes.ToDictionary(y => y, y => JoinLines(graph.GetAllReachingPaths(x.entry, y).Select(FormatInts)))
		}).Where(x => x.Entry != graph.EntryIndex || x.Exit != graph.ExitIndex)),
	}
};

static string GetCode(ControlFlowGraph graph)
{
	var visitor = new FormatScriptVisitor { WrapStatements = false, PrettyPrint = true };
	graph.Accept(visitor);
	return visitor.Code;
}

class GraphDumper
{
	Func<string> _getDot;
	public GraphDumper(Func<string> getDot) { _getDot = getDot; }
	object ToDump()
	{
		var path = Path.GetTempFileName();
		File.WriteAllText(path, _getDot());

		var pngPath = FormatGraph(path);
		File.Delete(path);

		var bytes = File.ReadAllBytes(pngPath);
		File.Delete(pngPath);

		using (var ms = new MemoryStream(bytes))
			return System.Drawing.Image.FromStream(ms);
	}

	string FormatGraph(string path)
	{
		var graphVizDot = @"C:\Program Files\Graphviz\bin\dot.exe";
		if (!File.Exists(graphVizDot))
			return null;

		var pngPath = Path.ChangeExtension(path, "png");
		var args = $"\"{path}\" -T png -o \"{pngPath}\"";
		using var process = Process.Start(graphVizDot, args);
		if (process == null)
			return null;

		process.WaitForExit();
		return pngPath;
	}
}

[Event("test")]
public class TestStatementEvent : Event
{
	[EventPart("value")] public int Value { get; }
	public TestStatementEvent(int value) => Value = value;
}

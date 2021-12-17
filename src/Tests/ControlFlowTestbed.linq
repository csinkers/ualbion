<Query Kind="Program">
  <Reference Relative="..\..\build\UAlbion.Scripting\bin\Debug\net5.0\Superpower.dll">C:\Depot\bb\ualbion\build\UAlbion.Scripting\bin\Debug\net5.0\Superpower.dll</Reference>
  <Reference Relative="..\..\build\UAlbion.Scripting\bin\Debug\net5.0\UAlbion.Api.dll">C:\Depot\bb\ualbion\build\UAlbion.Scripting\bin\Debug\net5.0\UAlbion.Api.dll</Reference>
  <Reference Relative="..\..\build\UAlbion.Scripting\bin\Debug\net5.0\UAlbion.Scripting.dll">C:\Depot\bb\ualbion\build\UAlbion.Scripting\bin\Debug\net5.0\UAlbion.Scripting.dll</Reference>
  <Namespace>UAlbion.Scripting</Namespace>
  <Namespace>UAlbion.Scripting.Ast</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
</Query>

void Main()
{
	DumpSteps(TestGraphs.MultiBreakMap201, 20);
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

	steps.Select(x => DumpGraph((ControlFlowGraph)x.Item2, x.Item1)).Dump();
}
static T NullOnError<T>(Func<T> func)
{
	try { return func(); }
	catch { return default;}
}

static string FormatInts(IEnumerable<int> ints) => string.Join(" ", ints.Select(x => x.ToString()));
static string JoinLines(IEnumerable<string> lines) => string.Join(Environment.NewLine, lines);
static object DumpTree(DominatorTree dominatorTree) => new GraphDumper(() => dominatorTree.ExportToDot(75));
static object DumpGraph(ControlFlowGraph graph, string description) => new
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
		Dominator = NullOnError(() => DumpTree(graph.GetDominatorTree())),
		PostDominator = NullOnError(() => DumpTree(graph.GetPostDominatorTree())),
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
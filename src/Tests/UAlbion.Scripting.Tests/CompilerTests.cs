namespace UAlbion.Scripting.Tests;

public class CompilerTests
{
    /*
    static readonly string ResultsDir = Path.Combine(TestUtil.FindBasePath(), "re", "CompilerTests");
    static void TestCompile(string script, ControlFlowGraph expected, [CallerMemberName] string method = null)
    {
        var steps = new List<(string, ControlFlowGraph)>();
        var resultsDir = !string.IsNullOrEmpty(method) ? Path.Combine(ResultsDir, method) : ResultsDir;
        try
        {
            var result = ScriptCompiler.Compile(script, steps);
            TestUtil.VerifyLayoutVsCfg(result, expected, steps, resultsDir);
        }
        catch (ControlFlowGraphException e)
        {
            steps.Add((e.Message, e.Graph));
            TestUtil.DumpSteps(steps, resultsDir, method);
            throw;
        }
        catch
        {
            TestUtil.DumpSteps(steps, resultsDir, method);
            throw;
        }
    }

    static void TestExpand(ControlFlowGraph graph, ControlFlowGraph expected, Func<ControlFlowGraph, ControlFlowGraph> func, [CallerMemberName] string method = null)
    {
        var resultsDir = !string.IsNullOrEmpty(method) ? Path.Combine(ResultsDir, method) : ResultsDir;
        var result = func(graph);
        TestUtil.VerifyNodesVsScript(result, expected);
    }
    */
    // TODO: Test sequence, if, if-else, do, while, loop, break, continue, goto and labels.
}
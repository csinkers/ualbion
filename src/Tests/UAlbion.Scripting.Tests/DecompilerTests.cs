using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UAlbion.Scripting.Ast;
using UAlbion.Scripting.Rules;
using Xunit;

namespace UAlbion.Scripting.Tests;

public class DecompilerTests
{
    static readonly string ResultsDir = Path.Combine(TestUtil.FindBasePath(), "re", "DecompilerTests");
    static void TestSimplify(ControlFlowGraph graph, string expected, [CallerMemberName] string method = null)
    {
        if (graph == null) throw new ArgumentNullException(nameof(graph));
        var steps = new List<(string, IGraph)> { ("Initial", graph) };
        var resultsDir = !string.IsNullOrEmpty(method) ? Path.Combine(ResultsDir, method) : ResultsDir;

        try
        {
            var result = Decompiler.SimplifyGraph(graph,
                (description, x) =>
                {
                    if (steps.Count == 0 || steps[^1].Item2 != x)
                        steps.Add((description, x));
                    return x;
                });

            if (!TestUtil.CompareNodesVsScript(new[] { result }, expected, out var error))
            {
                TestUtil.DumpSteps(steps, resultsDir, method);
                throw new InvalidOperationException(error);
            }
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

    [Fact] public void SequenceTest() => TestSimplify(TestGraphs.Sequence, "1, 2, 3"); 
    [Fact] public void IfThenTest() => TestSimplify(TestGraphs.IfThen, "if (1) { 2 }"); 
    [Fact] public void IfThenElseTest() => TestSimplify(TestGraphs.IfThenElse, "if (1) { 2 } else { 3 }"); 
    [Fact] public void DiamondSeseTest() => TestSimplify(TestGraphs.DiamondSese, TestGraphs.DiamondSeseCode);
    [Fact] public void SeseExample1Test() => TestSimplify(TestGraphs.SeseExample1, TestGraphs.SeseExample1Code);
    [Fact] public void LoopEdgeCase_Map313() => TestSimplify(TestGraphs.LoopEdgeCaseMap313, TestGraphs.LoopEdgeCaseMap313Code);
    //*
    [Fact] public void InfiniteLoopTest1() => TestSimplify(TestGraphs.InfiniteLoop1, "loop { 1 }");
    [Fact] public void InfiniteLoopTest2() => TestSimplify(TestGraphs.InfiniteLoop2, "loop { 1, 2 }");
    [Fact] public void ZeroKSeseTest() => TestSimplify(TestGraphs.ZeroKSese, @"if (1) {
    3
    goto L2
} else {
    2
    if (!(6)) {
        7
        if (8) {
            10
            if (11) {
                goto L1
            } else {
                12
                if (13) {
                    L1:
                    9
                } else {
                    5
                }
            }
        } else {
            goto L1
        }
        L2:
        4
    }
}"); 
    [Fact] public void NoMoreGotos3Region1Test() => TestSimplify(TestGraphs.NoMoreGotos3Region1, TestGraphs.NoMoreGotos3Region1Code); 
    [Fact] public void NoMoreGotos3Region2Test() => TestSimplify(TestGraphs.NoMoreGotos3Region2, TestGraphs.NoMoreGotos3Region2Code); 
    [Fact] public void NoMoreGotos3Region3Test() => TestSimplify(TestGraphs.NoMoreGotos3Region3, TestGraphs.NoMoreGotos3Region3Code); 
    [Fact] public void NoMoreGotos3Test() => TestSimplify(TestGraphs.NoMoreGotos3, TestGraphs.NoMoreGotos3Code); 

    [Fact] public void MidBreakLoopTest() => TestSimplify(TestGraphs.MidBreakLoop, TestGraphs.MidBreakLoopCode);
    [Fact] public void LoopEdgeCase_Map174() => TestSimplify(TestGraphs.LoopEdgeCaseMap174, TestGraphs.LoopEdgeCaseMap174Code);
    [Fact] public void LoopEdgeCase_Map302() => TestSimplify(TestGraphs.LoopEdgeCaseMap302, TestGraphs.LoopEdgeCaseMap302Code);
    [Fact] public void LoopEdgeCase_Map305() => TestSimplify(TestGraphs.LoopEdgeCaseMap305, TestGraphs.LoopEdgeCaseMap305Code);
    [Fact] public void LoopEdgeCase_Map305Reduced() => TestSimplify(TestGraphs.LoopEdgeCaseMap305Reduced, TestGraphs.LoopEdgeCaseMap305ReducedCode);
    [Fact] public void NestedLoopTest() => TestSimplify(TestGraphs.NestedLoop, TestGraphs.NestedLoopCode);
    [Fact] public void InfiniteLoop_Map149() => TestSimplify(TestGraphs.InfiniteLoopMap149, TestGraphs.InfiniteLoopMap149Code);
    [Fact] public void MultiBreakTest() => TestSimplify(TestGraphs.MultiBreak, TestGraphs.MultiBreakCode);
    [Fact] public void MultiBreak2Test() => TestSimplify(TestGraphs.MultiBreak2, TestGraphs.MultiBreak2Code);
    [Fact] public void MultiBreak_Map166() => TestSimplify(TestGraphs.MultiBreakMap166, TestGraphs.MultiBreakMap166Code);
    [Fact] public void MultiBreak_Map200() => TestSimplify(TestGraphs.MultiBreakMap200, TestGraphs.MultiBreakMap200Code);
    [Fact] public void MultiBreak_Map201() => TestSimplify(TestGraphs.MultiBreakMap201, TestGraphs.MultiBreakMap201Code);

    void LoopConversionTest(string expected, ICfgNode ast)
    {
        var nodes = new[] { ast };
        var cfg = new ControlFlowGraph(nodes, Array.Empty<(int, int, CfgEdge)>());
        var (result, _) = LoopConverter.Apply(cfg);
        if (!TestUtil.CompareCfgVsScript(result, expected, out var message))
            throw new InvalidOperationException(message);
    }

    [Fact]
    public void ConvertWhileTest() =>
        LoopConversionTest("while (!(1)) { }, 2",
            Emit.Seq(
                Emit.Loop(
                    Emit.If(
                        Emit.Statement(Emit.Const(1)),
                        Emit.Break())),
                Emit.Statement(Emit.Const(2))));

    [Fact]
    public void ConvertDoTest() =>
        LoopConversionTest("do { 1 } while (!(2)), 3",
            Emit.Seq(
                Emit.Loop(
                    Emit.Seq(
                        Emit.Statement(Emit.Const(1)),
                        Emit.If(
                            Emit.Statement(Emit.Const(2)),
                            Emit.Break()))
                ),
                Emit.Statement(Emit.Const(3))));

    [Fact]
    public void ConvertWhileWithTerminalBreakTest() =>
        LoopConversionTest("while (!(1)) { if (2) { break } }, 3",
            Emit.Seq(
                Emit.Loop(
                    Emit.Seq(
                        Emit.If(
                            Emit.Statement(Emit.Const(1)),
                            Emit.Break()),
                        Emit.If(
                            Emit.Statement(Emit.Const(2)),
                            Emit.Break()))),
                Emit.Statement(Emit.Const(3))));

    [Fact] public void SimpleWhileTest() => TestSimplify(TestGraphs.SimpleWhileLoop, "while (1) { }"); 
    [Fact] public void WhileTest() => TestSimplify(TestGraphs.WhileLoop, "while (1) { 2 }"); 
    [Fact] public void DoWhileTest() => TestSimplify(TestGraphs.DoWhileLoop, "do { 1 } while (2)");
    [Fact] public void BreakBranchTest() => TestSimplify(TestGraphs.BreakBranch, TestGraphs.BreakBranchCode);
    [Fact] public void BreakBranch2Test() => TestSimplify(TestGraphs.BreakBranch2, TestGraphs.BreakBranch2Code);
    [Fact] public void LoopBreaksBothEnds() => TestSimplify(TestGraphs.LoopBreaksBothEnds, TestGraphs.LoopBreaksBothEndsCode);
    [Fact] public void LoopBranchTest() => TestSimplify(TestGraphs.LoopBranch, TestGraphs.LoopBranchCode);
    [Fact] public void LoopBranchReducedTest() => TestSimplify(TestGraphs.LoopBranchReduced, TestGraphs.LoopBranchReducedCode);
}
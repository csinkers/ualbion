using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Xunit;

namespace UAlbion.Scripting.Tests
{
    public class DecompilerTests
    {
        static readonly string ResultsDir = Path.Combine(TestUtil.FindBasePath(), "re", "DecompilerTests");
        static void TestSimplify(ControlFlowGraph graph, string expected, [CallerMemberName] string method = null)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            var steps = new List<(string, ControlFlowGraph)> { ("Initial", graph) };
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

        [Fact]
        public void ReduceSequenceTest()
        {
            var result = Decompiler.ReduceSequences(TestGraphs.Sequence);
            result = Decompiler.ReduceSequences(result);
            TestUtil.VerifyCfgVsScript(result, null, "1, 2, 3", ResultsDir);
        }

        [Fact]
        public void ReduceIfThenTest()
        {
            var result = Decompiler.ReduceIfThen(TestGraphs.IfThen);
            result = Decompiler.ReduceSequences(result);
            TestUtil.VerifyCfgVsScript(result, null, "if (1) { 2 }", ResultsDir);
        }

        [Fact]
        public void ReduceIfThenElseTest()
        {
            var result = Decompiler.ReduceIfThenElse(TestGraphs.IfThenElse);
            result = Decompiler.ReduceSequences(result);
            TestUtil.VerifyCfgVsScript(result, null, "if (1) { 2 } else { 3 }", ResultsDir);
        }

        [Fact]
        public void ReduceSimpleWhileTest()
        {
            var result = Decompiler.ReduceSimpleWhile(TestGraphs.SimpleWhileLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            TestUtil.VerifyCfgVsScript(result, null, "while (1) { }", ResultsDir);
        }

        [Fact]
        public void ReduceNegativeSimpleWhileTest()
        {
            var result = Decompiler.ReduceSimpleWhile(TestGraphs.NegativeSimpleWhileLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            TestUtil.VerifyCfgVsScript(result, null, "while (!(1)) { }", ResultsDir);
        }

        [Fact]
        public void ReduceWhileTest()
        {
            var result = Decompiler.ReduceLoopParts(TestGraphs.WhileLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            TestUtil.VerifyCfgVsScript(result, null, "while (1) { 2 }", ResultsDir);
        }

        [Fact]
        public void ReduceNegativeWhileTest()
        {
            var result = Decompiler.ReduceLoopParts(TestGraphs.NegativeWhileLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            TestUtil.VerifyCfgVsScript(result, null, "while (!(1)) { 2 }", ResultsDir);
        }

        [Fact]
        public void ReduceDoWhileTest()
        {
            var result = Decompiler.ReduceLoopParts(TestGraphs.DoWhileLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            TestUtil.VerifyCfgVsScript(result, null, "do { 1 } while (2)", ResultsDir);
        }

        [Fact]
        public void ReduceNegativeDoWhileTest()
        {
            var result = Decompiler.ReduceLoopParts(TestGraphs.NegativeDoWhileLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            TestUtil.VerifyCfgVsScript(result, null, "do { 1 } while (!(2))", ResultsDir);
        }

        [Fact]
        public void ReduceEmptyInfiniteLoopTest()
        {
            var result = Decompiler.ReduceSimpleWhile(TestGraphs.InfiniteEmptyLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            result = CfgRelabeller.Relabel(result, ScriptConstants.DummyLabelPrefix);
            TestUtil.VerifyCfgVsScript(result, null, "L1:, 1, goto L1", ResultsDir);
        }

        [Fact]
        public void ReduceInfiniteLoopTest()
        {
            var result = Decompiler.ReduceLoopParts(TestGraphs.InfiniteLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            result = CfgRelabeller.Relabel(result, ScriptConstants.DummyLabelPrefix);
            TestUtil.VerifyCfgVsScript(result, null, "L1:, 1, 2, goto L1", ResultsDir);
        }
/*
        [Fact]
        public void ReduceSeseTest()
        {
            var result = Decompiler.ReduceSeseRegions(TestGraphs.ZeroKSese);
            TestUtil.Verify(result, null, "todo");
        } */

        [Fact] public void SequenceTest() => TestSimplify(TestGraphs.Sequence, "1, 2, 3"); 
        [Fact] public void IfThenTest() => TestSimplify(TestGraphs.IfThen, "if (1) { 2 }"); 
        [Fact] public void IfThenElseTest() => TestSimplify(TestGraphs.IfThenElse, "if (1) { 2 } else { 3 }"); 
        [Fact] public void SimpleWhileTest() => TestSimplify(TestGraphs.SimpleWhileLoop, "while (1) { }"); 
        [Fact] public void WhileTest() => TestSimplify(TestGraphs.WhileLoop, "while (1) { 2 }"); 
        [Fact] public void DoWhileTest() => TestSimplify(TestGraphs.DoWhileLoop, "do { 1 } while (2)");
        [Fact] public void DiamondSeseTest() => TestSimplify(TestGraphs.DiamondSese, TestGraphs.DiamondSeseCode);
        [Fact] public void SeseExample1Test() => TestSimplify(TestGraphs.SeseExample1, TestGraphs.SeseExample1Code);
        /*
        [Fact] public void MultiBreakTest() => TestSimplify(TestGraphs.MultiBreak, TestGraphs.MultiBreakCode);
        [Fact] public void MidBreakLoopTest() => TestSimplify(TestGraphs.MidBreakLoop, TestGraphs.MidBreakLoopCode);
        [Fact] public void ZeroKSeseTest() => TestSimplify(TestGraphs.ZeroKSese, "0; do { 2; } while (1); 3; "); 
        [Fact] public void NoMoreGotos3Test() => TestSimplify(TestGraphs.NoMoreGotos3, "something"); 
        [Fact] public void NoMoreGotos3Region1Test() => TestSimplify(TestGraphs.NoMoreGotos3Region1, TestGraphs.NoMoreGotos3Region1Code); 
        [Fact] public void NoMoreGotos3Region2Test() => TestSimplify(TestGraphs.NoMoreGotos3Region2, TestGraphs.NoMoreGotos3Region2Code); 
        [Fact] public void NoMoreGotos3Region3Test() => TestSimplify(TestGraphs.NoMoreGotos3Region3, TestGraphs.NoMoreGotos3Region3Code); 

        [Fact] public void LoopBranchTest() => TestSimplify(TestGraphs.LoopBranch, TestGraphs.LoopBranchCode);
        [Fact] public void BreakBranchTest() => TestSimplify(TestGraphs.BreakBranch, TestGraphs.BreakBranchCode);
        [Fact] public void BreakBranch2Test() => TestSimplify(TestGraphs.BreakBranch2, TestGraphs.BreakBranch2Code);
        [Fact] public void NestedLoopTest() => TestSimplify(TestGraphs.ContinueBranch, TestGraphs.ContinueBranchCode);
        //*/
    }
}
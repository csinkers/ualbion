using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;

namespace UAlbion.Scripting.Tests
{
    public class DecompilerTests
    {
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

                TestUtil.Verify(result, steps, expected, method);
            }
            catch (ControlFlowGraphException e)
            {
                steps.Add((e.Message, e.Graph));
                TestUtil.DumpSteps(steps, method);
                throw;
            }
            catch
            {
                TestUtil.DumpSteps(steps, method);
                throw;
            }
        }

        [Fact]
        public void ReduceSequenceTest()
        {
            var result = 
                Decompiler.ReduceSequences(
                    Decompiler.ReduceSequences(TestGraphs.Sequence));
            TestUtil.Verify(result, null, "0, 1, 2");
        }

        [Fact]
        public void ReduceIfThenTest()
        {
            var result = Decompiler.ReduceIfThen(TestGraphs.IfThen);
            result = Decompiler.ReduceSequences(result);
            TestUtil.Verify(result, null, "if (0) { 1 }, 2");
        }

        [Fact]
        public void ReduceIfThenElseTest()
        {
            var result = Decompiler.ReduceIfThenElse(TestGraphs.IfThenElse);
            result = Decompiler.ReduceSequences(result);
            TestUtil.Verify(result, null, "if (0) { 1 } else { 2 }, 3");
        }

        [Fact]
        public void ReduceSimpleWhileTest()
        {
            var result = Decompiler.ReduceSimpleWhile(TestGraphs.SimpleWhileLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            TestUtil.Verify(result, null, "0, while (1) { }, 2");
        }

        [Fact]
        public void ReduceWhileTest()
        {
            var result = Decompiler.ReduceSimpleLoops(TestGraphs.WhileLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            TestUtil.Verify(result, null, "0, while (1) { 2 }, 3");
        }

        [Fact]
        public void ReduceDoWhileTest()
        {
            var result = Decompiler.ReduceSimpleLoops(TestGraphs.DoWhileLoop);
            result = Decompiler.ReduceSequences(result);
            result = Decompiler.ReduceSequences(result);
            TestUtil.Verify(result, null, "0, do { 1 } while (2), 3");
        }
/*
        [Fact]
        public void ReduceSeseTest()
        {
            var result = Decompiler.ReduceSeseRegions(TestGraphs.ZeroKSese);
            TestUtil.Verify(result, null, "todo");
        } */

        [Fact] public void SequenceTest() => TestSimplify(TestGraphs.Sequence, "0, 1, 2"); 
        [Fact] public void IfThenTest() => TestSimplify(TestGraphs.IfThen, "if (0) { 1 }, 2"); 
        [Fact] public void IfThenElseTest() => TestSimplify(TestGraphs.IfThenElse, "if (0) { 1 } else { 2 }, 3"); 
        [Fact] public void SimpleWhileTest() => TestSimplify(TestGraphs.SimpleWhileLoop, "0, while (1) { }, 2"); 
        [Fact] public void WhileTest() => TestSimplify(TestGraphs.WhileLoop, "0, while (1) { 2 }, 3"); 
        [Fact] public void DoWhileTest() => TestSimplify(TestGraphs.DoWhileLoop, "0, do { 1 } while (2), 3"); 
        /*
        [Fact] public void ZeroKSeseTest() => TestSimplify(TestGraphs.ZeroKSese, "0; do { 2; } while (1); 3; "); 
        [Fact] public void NoMoreGotos3Test() => TestSimplify(TestGraphs.NoMoreGotos3, "something"); 
        [Fact] public void NoMoreGotos3Region1Test() => TestSimplify(TestGraphs.NoMoreGotos3Region1, TestGraphs.NoMoreGotos3Region1Code); 
        [Fact] public void NoMoreGotos3Region2Test() => TestSimplify(TestGraphs.NoMoreGotos3Region2, TestGraphs.NoMoreGotos3Region2Code); 
        [Fact] public void NoMoreGotos3Region3Test() => TestSimplify(TestGraphs.NoMoreGotos3Region3, TestGraphs.NoMoreGotos3Region3Code); 

        [Fact] public void LoopBranchTest() => TestSimplify(TestGraphs.LoopBranch, TestGraphs.LoopBranchCode);
        [Fact] public void BreakBranchTest() => TestSimplify(TestGraphs.BreakBranch, TestGraphs.BreakBranchCode);
        [Fact] public void BreakBranch2Test() => TestSimplify(TestGraphs.BreakBranch2, TestGraphs.BreakBranch2Code);
        [Fact] public void NestedLoopTest() => TestSimplify(TestGraphs.ContinueBranch, TestGraphs.ContinueBranchCode);
        */
    }
}
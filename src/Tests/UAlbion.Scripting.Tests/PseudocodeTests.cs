using UAlbion.Scripting.Ast;
using Xunit;

namespace UAlbion.Scripting.Tests
{
    public class PseudocodeTests
    {
        static Statement S(ICfgNode head, params ICfgNode[] p) => Emit.Statement(head, p);
        [Fact] public void FormatSequenceTest() 
            => TestUtil.VerifyPseudocode(
                "0, 1, 2",
                Emit.Seq(S(Emit.Const(0)), S(Emit.Const(1)), S(Emit.Const(2))));

        [Fact] public void FormatIfThenTest() => TestUtil.VerifyPseudocode(
            "if (0) { 1 }, 2",
            Emit.Seq(Emit.If(Emit.Const(0), S(Emit.Const(1))), S(Emit.Const(2))));

        [Fact] public void FormatIfThenElseTest() => TestUtil.VerifyPseudocode(
            "if (0) { 1 } else { 2 }, 3",
            Emit.Seq(Emit.IfElse(Emit.Const(0), S(Emit.Const(1)), S(Emit.Const(2))), S(Emit.Const(3))));

        [Fact] public void FormatSimpleWhileTest() => TestUtil.VerifyPseudocode(
            "0, while (1) { }, 2",
            Emit.Seq(S(Emit.Const(0)), Emit.While(Emit.Const(1), null), S(Emit.Const(2))));

        [Fact] public void FormatWhileTest() => TestUtil.VerifyPseudocode(
            "0, while (1) { 2 }, 3",
            Emit.Seq(S(Emit.Const(0)), Emit.While(Emit.Const(1), S(Emit.Const(2))), S(Emit.Const(3))));

        [Fact] public void FormatDoWhileTest() => TestUtil.VerifyPseudocode(
            "0, do { 1 } while (2), 3",
            Emit.Seq(S(Emit.Const(0)), Emit.Do(Emit.Const(2), S(Emit.Const(1))), S(Emit.Const(3))));

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
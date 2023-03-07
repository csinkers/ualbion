using UAlbion.Scripting.Ast;
using Xunit;

namespace UAlbion.Scripting.Tests;

public class PseudocodeTests
{
    static Statement S(ICfgNode head, params ICfgNode[] p) => UAEmit.Statement(head, p);
    [Fact] public void FormatSequenceTest() 
        => TestUtil.VerifyAstVsScript(
            "0, 1, 2",
            UAEmit.Seq(S(UAEmit.Const(0)), S(UAEmit.Const(1)), S(UAEmit.Const(2))));

    [Fact] public void FormatIfThenTest() => TestUtil.VerifyAstVsScript(
        "if (0) { 1 }, 2",
        UAEmit.Seq(UAEmit.If(UAEmit.Const(0), S(UAEmit.Const(1))), S(UAEmit.Const(2))));

    [Fact] public void FormatIfThenElseTest() => TestUtil.VerifyAstVsScript(
        "if (0) { 1 } else { 2 }, 3",
        UAEmit.Seq(UAEmit.IfElse(UAEmit.Const(0), S(UAEmit.Const(1)), S(UAEmit.Const(2))), S(UAEmit.Const(3))));

    [Fact] public void FormatSimpleWhileTest() => TestUtil.VerifyAstVsScript(
        "0, while (1) { }, 2",
        UAEmit.Seq(S(UAEmit.Const(0)), UAEmit.While(UAEmit.Const(1), null), S(UAEmit.Const(2))));

    [Fact] public void FormatWhileTest() => TestUtil.VerifyAstVsScript(
        "0, while (1) { 2 }, 3",
        UAEmit.Seq(S(UAEmit.Const(0)), UAEmit.While(UAEmit.Const(1), S(UAEmit.Const(2))), S(UAEmit.Const(3))));

    [Fact] public void FormatDoWhileTest() => TestUtil.VerifyAstVsScript(
        "0, do { 1 } while (2), 3",
        UAEmit.Seq(S(UAEmit.Const(0)), UAEmit.Do(UAEmit.Const(2), S(UAEmit.Const(1))), S(UAEmit.Const(3))));

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
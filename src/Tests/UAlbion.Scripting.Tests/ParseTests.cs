using System;
using Superpower;
using UAlbion.Api.Eventing;
using UAlbion.Scripting.Ast;
using Xunit;

namespace UAlbion.Scripting.Tests;

public class ParseTests
{
    static Statement S(ICfgNode head, params ICfgNode[] p) => UAEmit.Statement(head, p);
    [Fact] public void NameTest() => TestRoundTrip("foo", UAEmit.Name("foo"), ScriptParser.Name);
    [Fact] public void NumberTest() => TestRoundTrip("100", UAEmit.Const(100), ScriptParser.Number);
    [Fact] public void IdentifierNameTest() => TestRoundTrip("foo", UAEmit.Name("foo"), ScriptParser.Identifier);
    [Fact] public void IdentifierConstTest() => TestRoundTrip("100", UAEmit.Const(100), ScriptParser.Identifier);
    [Fact] public void IdentifierZeroTest() => TestRoundTrip("0", UAEmit.Const(0), ScriptParser.Identifier);
    [Fact] public void StatementTest() =>
        TestRoundTrip("a b c",
            S(UAEmit.Name("a"), UAEmit.Name("b"), UAEmit.Name("c")),
            ScriptParser.Expression);
    [Fact] public void LabelTest() => TestRoundTrip("foo:", UAEmit.Label("foo"), ScriptParser.Label);
    [Fact] public void GotoTest() => TestRoundTrip("goto foo", UAEmit.Goto("foo"), ScriptParser.Goto);

    [Fact] public void MemberTest() => TestRoundTrip("a.b", UAEmit.Member(UAEmit.Name("a"), UAEmit.Name("b")), ScriptParser.Expression);
    [Fact] public void MemberNumTest() => TestRoundTrip("a.100", UAEmit.Member(UAEmit.Name("a"), UAEmit.Const(100)), ScriptParser.Expression);
    [Fact] public void TestEq() => TestRoundTrip("a == 1", UAEmit.Eq(UAEmit.Name("a"), UAEmit.Const(1)), ScriptParser.Expression);
    [Fact] public void TestNeq() => TestRoundTrip("foo != bar", UAEmit.Neq(UAEmit.Name("foo"), UAEmit.Name("bar")), ScriptParser.Expression);
    [Fact] public void TestGt() => TestRoundTrip("a > 1", UAEmit.Gt(UAEmit.Name("a"), UAEmit.Const(1)), ScriptParser.Expression);
    [Fact] public void TestLt() => TestRoundTrip("a < 1", UAEmit.Lt(UAEmit.Name("a"), UAEmit.Const(1)), ScriptParser.Expression);
    [Fact] public void TestGte() => TestRoundTrip("a >= 1", UAEmit.Gte(UAEmit.Name("a"), UAEmit.Const(1)), ScriptParser.Expression);
    [Fact] public void TestLte() => TestRoundTrip("a <= 1", UAEmit.Lte(UAEmit.Name("a"), UAEmit.Const(1)), ScriptParser.Expression);

    [Fact] public void TestAnd() => TestRoundTrip("a == 1 && b != 2",
        UAEmit.And(
            UAEmit.Eq(UAEmit.Name("a"), UAEmit.Const(1)),
            UAEmit.Neq(UAEmit.Name("b"), UAEmit.Const(2))),
        ScriptParser.Expression);

    [Fact] public void TestOr() => TestRoundTrip("a == 1 || b != 2",
        UAEmit.Or(
            UAEmit.Eq(UAEmit.Name("a"), UAEmit.Const(1)),
            UAEmit.Neq(UAEmit.Name("b"), UAEmit.Const(2))),
        ScriptParser.Expression);

    [Fact] public void TestAssign() => TestRoundTrip("foo = 12", UAEmit.Assign(UAEmit.Name("foo"), UAEmit.Const(12)), ScriptParser.Expression);
    [Fact] public void TestAdd() => TestRoundTrip("x += 3", UAEmit.Add(UAEmit.Name("x"), UAEmit.Const(3)), ScriptParser.Expression);
    [Fact] public void TestSub() => TestRoundTrip("x -= 3", UAEmit.Sub(UAEmit.Name("x"), UAEmit.Const(3)), ScriptParser.Expression);
    [Fact] public void TestBitwiseOr() => TestRoundTrip("Underlay|Overlay", UAEmit.BitwiseOr(UAEmit.Name("Underlay"), UAEmit.Name("Overlay")), ScriptParser.Expression);

    [Fact] public void ExpressionNameTest() => TestRoundTrip("foo", UAEmit.Name("foo"), ScriptParser.Expression);
    [Fact] public void ExpressionNumberTest() => TestRoundTrip("100", UAEmit.Const(100), ScriptParser.Expression);
    [Fact] public void ExpressionIdentifierNameTest() => TestRoundTrip("foo", UAEmit.Name("foo"), ScriptParser.Expression);
    [Fact] public void ExpressionIdentifierConstTest() => TestRoundTrip("100", UAEmit.Const(100), ScriptParser.Expression);
    [Fact] public void ExpressionIdentifierZeroTest() => TestRoundTrip("0", UAEmit.Const(0), ScriptParser.Expression);
    [Fact] public void ExpressionMemberTest() => TestRoundTrip("a.b", UAEmit.Member(UAEmit.Name("a"), UAEmit.Name("b")), ScriptParser.Expression);
    [Fact] public void ExpressionMemberNumTest() => TestRoundTrip("a.100", UAEmit.Member(UAEmit.Name("a"), UAEmit.Const(100)), ScriptParser.Expression);

    [Fact] public void StatementNameTest()            => TestRoundTrip("foo",   S(UAEmit.Name("foo")), ScriptParser.Statement);
    [Fact] public void StatementNumberTest()          => TestRoundTrip("100",   S(UAEmit.Const(100)), ScriptParser.Statement);
    [Fact] public void StatementIdentifierNameTest()  => TestRoundTrip("foo",   S(UAEmit.Name("foo")), ScriptParser.Statement);
    [Fact] public void StatementIdentifierConstTest() => TestRoundTrip("100",   S(UAEmit.Const(100)), ScriptParser.Statement);
    [Fact] public void StatementIdentifierZeroTest()  => TestRoundTrip("0",     S(UAEmit.Const(0)), ScriptParser.Statement);
    [Fact] public void StatementMemberTest()          => TestRoundTrip("a.b",   S(UAEmit.Member(UAEmit.Name("a"), UAEmit.Name("b"))), ScriptParser.Statement);
    [Fact] public void StatementMemberNumTest()       => TestRoundTrip("a.100", S(UAEmit.Member(UAEmit.Name("a"), UAEmit.Const(100))), ScriptParser.Statement);
    [Fact] public void StatementStatementTest()       => TestRoundTrip("a b c", S(UAEmit.Name("a"), UAEmit.Name("b"), UAEmit.Name("c")), ScriptParser.Statement);
    [Fact] public void StatementLabelTest()           => TestRoundTrip("foo:",  UAEmit.Label("foo"), ScriptParser.Statement);
    [Fact] public void StatementGotoTest()            => TestRoundTrip("goto foo", UAEmit.Goto("foo"), ScriptParser.Statement);

    [Fact] public void NegationNameTest() => TestRoundTrip("!foo", UAEmit.Negation(UAEmit.Name("foo")), ScriptParser.Negation);
    [Fact] public void NegationNumberTest() => TestRoundTrip("!100", UAEmit.Negation(UAEmit.Const(100)), ScriptParser.Negation);
    [Fact] public void NegationCompoundTest() =>
        TestRoundTrip("!(ticker.100 == 23)",
            UAEmit.Negation(
                UAEmit.Eq(
                    UAEmit.Member(
                        UAEmit.Name("ticker"),
                        UAEmit.Const(100)),
                    UAEmit.Const(23)
                )
            ), ScriptParser.Negation);

    [Fact] public void ParenthesesTest1() =>
        TestRoundTrip("a > b && c <= b",
            UAEmit.And(
                UAEmit.Gt(UAEmit.Name("a"), UAEmit.Name("b")),
                UAEmit.Lte(UAEmit.Name("c"), UAEmit.Name("b"))),
            ScriptParser.Expression);

    [Fact] public void ParenthesesTest2() =>
        TestParse("(a > b && (c <= b))",
            UAEmit.And(
                UAEmit.Gt(UAEmit.Name("a"), UAEmit.Name("b")),
                UAEmit.Lte(UAEmit.Name("c"), UAEmit.Name("b"))),
            ScriptParser.Expression);

    [Fact] public void ParenthesesTest3() =>
        TestParse("(a > b) && (c <= b)",
            UAEmit.And(
                UAEmit.Gt(UAEmit.Name("a"), UAEmit.Name("b")),
                UAEmit.Lte(UAEmit.Name("c"), UAEmit.Name("b"))),
            ScriptParser.Expression);

    [Fact] public void ParenthesesTest4() =>
        TestParse("((a > b) && c <= b)",
            UAEmit.And(
                UAEmit.Gt(UAEmit.Name("a"), UAEmit.Name("b")),
                UAEmit.Lte(UAEmit.Name("c"), UAEmit.Name("b"))),
            ScriptParser.Expression);

    [Fact] public void OrderTest01() => TestRoundTrip(
        "a && (b || c)",
        UAEmit.And(
            UAEmit.Name("a"),
            UAEmit.Or(UAEmit.Name("b"), UAEmit.Name("c"))), ScriptParser.Expression);

    [Fact] public void OrderTest02() => TestRoundTrip(
        "a && b || c",
        UAEmit.Or(
            UAEmit.And(UAEmit.Name("a"), UAEmit.Name("b")),
            UAEmit.Name("c")),
        ScriptParser.Expression);

    [Fact] public void OrderTest03() => TestRoundTrip(
        "a > b || c < d",
        UAEmit.Or(
            UAEmit.Gt(UAEmit.Name("a"), UAEmit.Name("b")),
            UAEmit.Lt(UAEmit.Name("c"), UAEmit.Name("d"))
        ), ScriptParser.Expression);

    [Fact] public void OrderTest04() => TestRoundTrip(
        "a > b && c < d",
        UAEmit.And(
            UAEmit.Gt(UAEmit.Name("a"), UAEmit.Name("b")),
            UAEmit.Lt(UAEmit.Name("c"), UAEmit.Name("d"))
        ), ScriptParser.Expression);

    [Fact] public void OrderTest05() => TestRoundTrip(
        "a == b || a != c",
        UAEmit.Or(
            UAEmit.Eq(UAEmit.Name("a"), UAEmit.Name("b")),
            UAEmit.Neq(UAEmit.Name("a"), UAEmit.Name("c"))
        ), ScriptParser.Expression);

    [Fact] public void OrderTest06() => TestRoundTrip(
        "x.y += 10",
        UAEmit.Add(
            UAEmit.Member(UAEmit.Name("x"), UAEmit.Name("y")),
            UAEmit.Const(10)
        ), ScriptParser.Expression);

    [Fact] public void OrderTest07() => TestRoundTrip(
        "a.b.c.d",
        UAEmit.Member(
            UAEmit.Member(
                UAEmit.Member(UAEmit.Name("a"), UAEmit.Name("b")),
                UAEmit.Name("c")
            ),
            UAEmit.Name("d")
        ), ScriptParser.Expression);

    [Fact] public void BlockTest() =>
        TestParse("{ a, b, c }",
            UAEmit.Seq(S(UAEmit.Name("a")), S(UAEmit.Name("b")), S(UAEmit.Name("c"))),
            ScriptParser.Block);

    [Fact] public void SequenceTest() =>
        TestRoundTrip("a, b, c",
            UAEmit.Seq(S(UAEmit.Name("a")), S(UAEmit.Name("b")), S(UAEmit.Name("c"))),
            ScriptParser.Sequence);

    [Fact] public void IfTest() =>
        TestRoundTrip("if (a) { b }",
            UAEmit.If(UAEmit.Name("a"), S(UAEmit.Name("b"))),
            ScriptParser.If);

    [Fact] public void IfElseTest() =>
        TestRoundTrip("if (a) { b } else { c }",
            UAEmit.IfElse(UAEmit.Name("a"), S(UAEmit.Name("b")), S(UAEmit.Name("c"))),
            ScriptParser.IfElse);

    [Fact] public void WhileTest() =>
        TestRoundTrip("while (a) { b }",
            UAEmit.While(UAEmit.Name("a"), S(UAEmit.Name("b"))),
            ScriptParser.While);

    [Fact] public void DoTest() =>
        TestRoundTrip("do { b } while (a)",
            UAEmit.Do(UAEmit.Name("a"), S(UAEmit.Name("b"))),
            ScriptParser.Do);

    [Fact] public void BreakTest() =>
        TestRoundTrip("while (a) { if (b) { break }, c }",
            UAEmit.While(UAEmit.Name("a"),
                UAEmit.Seq(
                    UAEmit.If(UAEmit.Name("b"), UAEmit.Break()),
                    S(UAEmit.Name("c")))),
            ScriptParser.TopLevel);

    [Fact] public void ContinueTest() =>
        TestRoundTrip("while (a) { if (b) { continue }, c }",
            UAEmit.While(UAEmit.Name("a"),
                UAEmit.Seq(
                    UAEmit.If(UAEmit.Name("b"), UAEmit.Continue()),
                    S(UAEmit.Name("c")))),
            ScriptParser.TopLevel);

    [Fact] public void IfEventTest() =>
        TestRoundTrip("if (a 1 2) { b foo }",
            UAEmit.If(
                S(UAEmit.Name("a"), UAEmit.Const(1), UAEmit.Const(2)),
                S(UAEmit.Name("b"), UAEmit.Name("foo"))
            ),
            ScriptParser.TopLevel);

    [Fact] public void BlockPrettyTest() =>
        TestParse(@"{
    a
    b
    c
}",
            UAEmit.Seq(S(UAEmit.Name("a")), S(UAEmit.Name("b")), S(UAEmit.Name("c"))),
            ScriptParser.Block);

    [Fact] public void SequencePrettyTest() =>
        TestRoundTrip(@"a
b
c",
            UAEmit.Seq(S(UAEmit.Name("a")), S(UAEmit.Name("b")), S(UAEmit.Name("c"))),
            ScriptParser.Sequence, true);

    [Fact] public void SequenceEmptyLinePrettyTest() =>
        TestParse(@"a
b



c",
            UAEmit.Seq(S(UAEmit.Name("a")), S(UAEmit.Name("b")), S(UAEmit.Name("c"))),
            ScriptParser.Sequence);

    [Fact] public void IfPrettyTest() =>
        TestRoundTrip(@"if (a) {
    b
}",
            UAEmit.If(UAEmit.Name("a"), S(UAEmit.Name("b"))),
            ScriptParser.If, true);

    [Fact] public void IfElsePrettyTest() =>
        TestRoundTrip(@"if (a) {
    b
} else {
    c
}",
            UAEmit.IfElse(UAEmit.Name("a"), S(UAEmit.Name("b")), S(UAEmit.Name("c"))),
            ScriptParser.IfElse, true);

    [Fact] public void WhilePrettyTest() =>
        TestRoundTrip(@"while (a) {
    b
}",
            UAEmit.While(UAEmit.Name("a"), S(UAEmit.Name("b"))),
            ScriptParser.While, true);

    [Fact] public void DoPrettyTest() =>
        TestRoundTrip(@"do {
    b
} while (a)",
            UAEmit.Do(UAEmit.Name("a"), S(UAEmit.Name("b"))),
            ScriptParser.Do, true);

    [Fact] public void BreakPrettyTest() =>
        TestRoundTrip(@"while (a) {
    if (b) {
        break
    }
    c
}",
            UAEmit.While(UAEmit.Name("a"),
                UAEmit.Seq(
                    UAEmit.If(UAEmit.Name("b"), UAEmit.Break()),
                    S(UAEmit.Name("c")))),
            ScriptParser.TopLevel, true);

    [Fact] public void ContinuePrettyTest() =>
        TestRoundTrip(@"while (a) {
    if (b) {
        continue
    }
    c
}",
            UAEmit.While(UAEmit.Name("a"),
                UAEmit.Seq(
                    UAEmit.If(UAEmit.Name("b"), UAEmit.Continue()),
                    S(UAEmit.Name("c")))),
            ScriptParser.TopLevel, true);

    [Fact] public void IfEventPrettyTest() =>
        TestRoundTrip(@"if (a 1 2) {
    b foo
}",
            UAEmit.If(
                S(UAEmit.Name("a"), UAEmit.Const(1), UAEmit.Const(2)),
                S(UAEmit.Name("b"), UAEmit.Name("foo"))
            ),
            ScriptParser.TopLevel, true);

    static T TestParse<T>(string source, ICfgNode expected, TokenListParser<ScriptToken, T> parser) where T : ICfgNode
    {
        var tokens = ScriptTokenizer.Tokenize(source);
        if (!tokens.HasValue)
            throw new InvalidOperationException(
                $"Tokenization failure: {tokens.ErrorMessage} at {tokens.ErrorPosition}");

        var filtered = ScriptParser.FilterTokens(tokens.Value);
        var parsed = parser.TryParse(filtered);
        if (!parsed.HasValue)
            throw new InvalidOperationException($"Parse failure: {parsed}");

        Assert.Equal(expected, parsed.Value);
        return parsed.Value;
    }

    static void TestRoundTrip<T>(
        string source,
        ICfgNode expected,
        TokenListParser<ScriptToken, T> parser,
        bool pretty = false) where T : ICfgNode
    {
        var parsed = TestParse(source, expected, parser);
        var builder = new UnformattedScriptBuilder(false);
        var visitor = new FormatScriptVisitor(builder) { PrettyPrint = pretty, WrapStatements = false };
        parsed.Accept(visitor);
        Assert.Equal(source, builder.Build());
    }
}
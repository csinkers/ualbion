using System;
using Superpower;
using UAlbion.Scripting.Ast;
using Xunit;

namespace UAlbion.Scripting.Tests
{
    public class ParseTests
    {
        static Statement S(ICfgNode head, params ICfgNode[] p) => Emit.Statement(head, p);
        [Fact] public void NameTest() => TestRoundTrip("foo", Emit.Name("foo"), ScriptParser.Name);
        [Fact] public void NumberTest() => TestRoundTrip("100", Emit.Const(100), ScriptParser.Number);
        [Fact] public void IdentifierNameTest() => TestRoundTrip("foo", Emit.Name("foo"), ScriptParser.Identifier);
        [Fact] public void IdentifierConstTest() => TestRoundTrip("100", Emit.Const(100), ScriptParser.Identifier);
        [Fact] public void IdentifierZeroTest() => TestRoundTrip("0", Emit.Const(0), ScriptParser.Identifier);
        [Fact] public void StatementTest() =>
            TestRoundTrip("a b c",
                S(Emit.Name("a"), Emit.Name("b"), Emit.Name("c")),
                ScriptParser.Expression);
        [Fact] public void LabelTest() => TestRoundTrip("foo:", Emit.Label("foo"), ScriptParser.Label);

        [Fact] public void MemberTest() => TestRoundTrip("a.b", Emit.Member(Emit.Name("a"), Emit.Name("b")), ScriptParser.Expression);
        [Fact] public void MemberNumTest() => TestRoundTrip("a.100", Emit.Member(Emit.Name("a"), Emit.Const(100)), ScriptParser.Expression);
        [Fact] public void TestEq() => TestRoundTrip("a == 1", Emit.Eq(Emit.Name("a"), Emit.Const(1)), ScriptParser.Expression);
        [Fact] public void TestNeq() => TestRoundTrip("foo != bar", Emit.Neq(Emit.Name("foo"), Emit.Name("bar")), ScriptParser.Expression);
        [Fact] public void TestGt() => TestRoundTrip("a > 1", Emit.Gt(Emit.Name("a"), Emit.Const(1)), ScriptParser.Expression);
        [Fact] public void TestLt() => TestRoundTrip("a < 1", Emit.Lt(Emit.Name("a"), Emit.Const(1)), ScriptParser.Expression);
        [Fact] public void TestGte() => TestRoundTrip("a >= 1", Emit.Gte(Emit.Name("a"), Emit.Const(1)), ScriptParser.Expression);
        [Fact] public void TestLte() => TestRoundTrip("a <= 1", Emit.Lte(Emit.Name("a"), Emit.Const(1)), ScriptParser.Expression);

        [Fact] public void TestAnd() => TestRoundTrip("a == 1 && b != 2",
            Emit.And(
                Emit.Eq(Emit.Name("a"), Emit.Const(1)),
                Emit.Neq(Emit.Name("b"), Emit.Const(2))),
            ScriptParser.Expression);

        [Fact] public void TestOr() => TestRoundTrip("a == 1 || b != 2",
            Emit.Or(
                Emit.Eq(Emit.Name("a"), Emit.Const(1)),
                Emit.Neq(Emit.Name("b"), Emit.Const(2))),
            ScriptParser.Expression);

        [Fact] public void TestAssign() => TestRoundTrip("foo = 12", Emit.Assign(Emit.Name("foo"), Emit.Const(12)), ScriptParser.Expression);
        [Fact] public void TestAdd() => TestRoundTrip("x += 3", Emit.Add(Emit.Name("x"), Emit.Const(3)), ScriptParser.Expression);
        [Fact] public void TestSub() => TestRoundTrip("x -= 3", Emit.Sub(Emit.Name("x"), Emit.Const(3)), ScriptParser.Expression);

        [Fact] public void ExpressionNameTest() => TestRoundTrip("foo", Emit.Name("foo"), ScriptParser.Expression);
        [Fact] public void ExpressionNumberTest() => TestRoundTrip("100", Emit.Const(100), ScriptParser.Expression);
        [Fact] public void ExpressionIdentifierNameTest() => TestRoundTrip("foo", Emit.Name("foo"), ScriptParser.Expression);
        [Fact] public void ExpressionIdentifierConstTest() => TestRoundTrip("100", Emit.Const(100), ScriptParser.Expression);
        [Fact] public void ExpressionIdentifierZeroTest() => TestRoundTrip("0", Emit.Const(0), ScriptParser.Expression);
        [Fact] public void ExpressionMemberTest() => TestRoundTrip("a.b", Emit.Member(Emit.Name("a"), Emit.Name("b")), ScriptParser.Expression);
        [Fact] public void ExpressionMemberNumTest() => TestRoundTrip("a.100", Emit.Member(Emit.Name("a"), Emit.Const(100)), ScriptParser.Expression);

        [Fact] public void StatementNameTest()            => TestRoundTrip("foo",   S(Emit.Name("foo")), ScriptParser.Statement);
        [Fact] public void StatementNumberTest()          => TestRoundTrip("100",   S(Emit.Const(100)), ScriptParser.Statement);
        [Fact] public void StatementIdentifierNameTest()  => TestRoundTrip("foo",   S(Emit.Name("foo")), ScriptParser.Statement);
        [Fact] public void StatementIdentifierConstTest() => TestRoundTrip("100",   S(Emit.Const(100)), ScriptParser.Statement);
        [Fact] public void StatementIdentifierZeroTest()  => TestRoundTrip("0",     S(Emit.Const(0)), ScriptParser.Statement);
        [Fact] public void StatementMemberTest()          => TestRoundTrip("a.b",   S(Emit.Member(Emit.Name("a"), Emit.Name("b"))), ScriptParser.Statement);
        [Fact] public void StatementMemberNumTest()       => TestRoundTrip("a.100", S(Emit.Member(Emit.Name("a"), Emit.Const(100))), ScriptParser.Statement);
        [Fact] public void StatementStatementTest()       => TestRoundTrip("a b c", S(Emit.Name("a"), Emit.Name("b"), Emit.Name("c")), ScriptParser.Statement);
        [Fact] public void StatementLabelTest()           => TestRoundTrip("foo:",  Emit.Label("foo"), ScriptParser.Statement);

        [Fact] public void NegationNameTest() => TestRoundTrip("!foo", Emit.Negation(Emit.Name("foo")), ScriptParser.Negation);
        [Fact] public void NegationNumberTest() => TestRoundTrip("!100", Emit.Negation(Emit.Const(100)), ScriptParser.Negation);
        [Fact] public void NegationCompoundTest() =>
            TestRoundTrip("!(ticker.100 == 23)",
                Emit.Negation(
                    Emit.Eq(
                        Emit.Member(
                            Emit.Name("ticker"),
                            Emit.Const(100)),
                        Emit.Const(23)
                    )
                ), ScriptParser.Negation);

        [Fact] public void ParenthesesTest1() =>
            TestRoundTrip("a > b && c <= b",
                Emit.And(
                    Emit.Gt(Emit.Name("a"), Emit.Name("b")),
                    Emit.Lte(Emit.Name("c"), Emit.Name("b"))),
                ScriptParser.Expression);

        [Fact] public void ParenthesesTest2() =>
            TestParse("(a > b && (c <= b))",
                Emit.And(
                    Emit.Gt(Emit.Name("a"), Emit.Name("b")),
                    Emit.Lte(Emit.Name("c"), Emit.Name("b"))),
                ScriptParser.Expression);

        [Fact] public void ParenthesesTest3() =>
            TestParse("(a > b) && (c <= b)",
                Emit.And(
                    Emit.Gt(Emit.Name("a"), Emit.Name("b")),
                    Emit.Lte(Emit.Name("c"), Emit.Name("b"))),
                ScriptParser.Expression);

        [Fact] public void ParenthesesTest4() =>
            TestParse("((a > b) && c <= b)",
                Emit.And(
                    Emit.Gt(Emit.Name("a"), Emit.Name("b")),
                    Emit.Lte(Emit.Name("c"), Emit.Name("b"))),
                ScriptParser.Expression);

        [Fact] public void OrderTest01() => TestRoundTrip(
            "a && (b || c)",
            Emit.And(
                Emit.Name("a"),
                Emit.Or(Emit.Name("b"), Emit.Name("c"))), ScriptParser.Expression);

        [Fact] public void OrderTest02() => TestRoundTrip(
            "a && b || c",
            Emit.Or(
                Emit.And(Emit.Name("a"), Emit.Name("b")),
                Emit.Name("c")),
            ScriptParser.Expression);

        [Fact] public void OrderTest03() => TestRoundTrip(
            "a > b || c < d",
            Emit.Or(
                Emit.Gt(Emit.Name("a"), Emit.Name("b")),
                Emit.Lt(Emit.Name("c"), Emit.Name("d"))
            ), ScriptParser.Expression);

        [Fact] public void OrderTest04() => TestRoundTrip(
            "a > b && c < d",
            Emit.And(
                Emit.Gt(Emit.Name("a"), Emit.Name("b")),
                Emit.Lt(Emit.Name("c"), Emit.Name("d"))
            ), ScriptParser.Expression);

        [Fact] public void OrderTest05() => TestRoundTrip(
            "a == b || a != c",
            Emit.Or(
                Emit.Eq(Emit.Name("a"), Emit.Name("b")),
                Emit.Neq(Emit.Name("a"), Emit.Name("c"))
            ), ScriptParser.Expression);

        [Fact] public void OrderTest06() => TestRoundTrip(
            "x.y += 10",
            Emit.Add(
                Emit.Member(Emit.Name("x"), Emit.Name("y")),
                Emit.Const(10)
            ), ScriptParser.Expression);

        [Fact] public void OrderTest07() => TestRoundTrip(
            "a.b.c.d",
            Emit.Member(
                Emit.Member(
                    Emit.Member(Emit.Name("a"), Emit.Name("b")),
                    Emit.Name("c")
                ),
                Emit.Name("d")
            ), ScriptParser.Expression);

        [Fact] public void BlockTest() =>
            TestParse("{ a, b, c }",
                Emit.Seq(S(Emit.Name("a")), S(Emit.Name("b")), S(Emit.Name("c"))),
                ScriptParser.Block);

        [Fact] public void SequenceTest() =>
            TestRoundTrip("a, b, c",
                Emit.Seq(S(Emit.Name("a")), S(Emit.Name("b")), S(Emit.Name("c"))),
                ScriptParser.Sequence);

        [Fact] public void IfTest() =>
            TestRoundTrip("if (a) { b }",
                Emit.If(Emit.Name("a"), S(Emit.Name("b"))),
                ScriptParser.If);

        [Fact] public void IfElseTest() =>
            TestRoundTrip("if (a) { b } else { c }",
                Emit.IfElse(Emit.Name("a"), S(Emit.Name("b")), S(Emit.Name("c"))),
                ScriptParser.IfElse);

        [Fact] public void WhileTest() =>
            TestRoundTrip("while (a) { b }",
                Emit.While(Emit.Name("a"), S(Emit.Name("b"))),
                ScriptParser.While);

        [Fact] public void DoTest() =>
            TestRoundTrip("do { b } while (a)",
                Emit.Do(Emit.Name("a"), S(Emit.Name("b"))),
                ScriptParser.Do);

        [Fact] public void BreakTest() =>
            TestRoundTrip("while (a) { if (b) { break }, c }",
                Emit.While(Emit.Name("a"),
                    Emit.Seq(
                        Emit.If(Emit.Name("b"), Emit.Break()),
                        S(Emit.Name("c")))),
                ScriptParser.TopLevel);

        [Fact] public void ContinueTest() =>
            TestRoundTrip("while (a) { if (b) { continue }, c }",
                Emit.While(Emit.Name("a"),
                    Emit.Seq(
                        Emit.If(Emit.Name("b"), Emit.Continue()),
                        S(Emit.Name("c")))),
                ScriptParser.TopLevel);

        [Fact] public void IfEventTest() =>
            TestRoundTrip("if (a 1 2) { b foo }",
                Emit.If(
                    S(Emit.Name("a"), Emit.Const(1), Emit.Const(2)),
                    S(Emit.Name("b"), Emit.Name("foo"))
                ),
                ScriptParser.TopLevel);

        [Fact] public void BlockPrettyTest() =>
            TestParse(@"{
    a
    b
    c
}",
                Emit.Seq(S(Emit.Name("a")), S(Emit.Name("b")), S(Emit.Name("c"))),
                ScriptParser.Block);

        [Fact] public void SequencePrettyTest() =>
            TestRoundTrip(@"a
b
c",
                Emit.Seq(S(Emit.Name("a")), S(Emit.Name("b")), S(Emit.Name("c"))),
                ScriptParser.Sequence, true);

        [Fact] public void SequenceEmptyLinePrettyTest() =>
            TestParse(@"a
b



c",
                Emit.Seq(S(Emit.Name("a")), S(Emit.Name("b")), S(Emit.Name("c"))),
                ScriptParser.Sequence);

        [Fact] public void IfPrettyTest() =>
            TestRoundTrip(@"if (a) {
    b
}",
                Emit.If(Emit.Name("a"), S(Emit.Name("b"))),
                ScriptParser.If, true);

        [Fact] public void IfElsePrettyTest() =>
            TestRoundTrip(@"if (a) {
    b
} else {
    c
}",
                Emit.IfElse(Emit.Name("a"), S(Emit.Name("b")), S(Emit.Name("c"))),
                ScriptParser.IfElse, true);

        [Fact] public void WhilePrettyTest() =>
            TestRoundTrip(@"while (a) {
    b
}",
                Emit.While(Emit.Name("a"), S(Emit.Name("b"))),
                ScriptParser.While, true);

        [Fact] public void DoPrettyTest() =>
            TestRoundTrip(@"do {
    b
} while (a)",
                Emit.Do(Emit.Name("a"), S(Emit.Name("b"))),
                ScriptParser.Do, true);

        [Fact] public void BreakPrettyTest() =>
            TestRoundTrip(@"while (a) {
    if (b) {
        break
    }
    c
}",
                Emit.While(Emit.Name("a"),
                    Emit.Seq(
                        Emit.If(Emit.Name("b"), Emit.Break()),
                        S(Emit.Name("c")))),
                ScriptParser.TopLevel, true);

        [Fact] public void ContinuePrettyTest() =>
            TestRoundTrip(@"while (a) {
    if (b) {
        continue
    }
    c
}",
                Emit.While(Emit.Name("a"),
                    Emit.Seq(
                        Emit.If(Emit.Name("b"), Emit.Continue()),
                        S(Emit.Name("c")))),
                ScriptParser.TopLevel, true);

        [Fact] public void IfEventPrettyTest() =>
            TestRoundTrip(@"if (a 1 2) {
    b foo
}",
                Emit.If(
                    S(Emit.Name("a"), Emit.Const(1), Emit.Const(2)),
                    S(Emit.Name("b"), Emit.Name("foo"))
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
            var visitor = new EmitPseudocodeVisitor { PrettyPrint = pretty };
            parsed.Accept(visitor);
            Assert.Equal(source, visitor.Code);
        }
    }
}

using System;
using Superpower;
using UAlbion.Scripting.Ast;
using Xunit;

namespace UAlbion.Scripting.Tests
{
    public class ParseTests
    {
        static Statement S(ICfgNode head, params ICfgNode[] p) => Emit.Statement(head, p);
        [Fact] public void NameTest() => Test("foo", Emit.Name("foo"), ScriptParser.Name); 
        [Fact] public void NumberTest() => Test("100", Emit.Const(100), ScriptParser.Number);
        [Fact] public void IdentifierNameTest() => Test("foo", Emit.Name("foo"), ScriptParser.Identifier);
        [Fact] public void IdentifierConstTest() => Test("100", Emit.Const(100), ScriptParser.Identifier);
        [Fact] public void IdentifierZeroTest() => Test("0", Emit.Const(0), ScriptParser.Identifier);
        [Fact] public void MemberTest() => Test("a.b", Emit.Member(Emit.Name("a"), Emit.Name("b")), ScriptParser.Member);
        [Fact] public void MemberNumTest() => Test("a.100", Emit.Member(Emit.Name("a"), Emit.Const(100)), ScriptParser.Member);
        [Fact] public void IndexTest() => Test("a[b]", Emit.Index(Emit.Name("a"), Emit.Name("b")), ScriptParser.Indexer);
        [Fact] public void IndexNumTest() => Test("a[100]", Emit.Index(Emit.Name("a"), Emit.Const(100)), ScriptParser.Indexer);
        [Fact] public void StatementTest() => Test("a b c", Emit.Statement(Emit.Name("a"), Emit.Name("b"), Emit.Name("c")), ScriptParser.EventStatement);
        [Fact] public void LabelTest() => Test("foo:", Emit.Label("foo"), ScriptParser.Label);

        [Fact] public void TestEq() => Test("a == 1", Emit.Op(Operation.Equal, Emit.Name("a"), Emit.Const(1)), ScriptParser.Expression);
        [Fact] public void TestNeq() => Test("foo != bar", Emit.Op(Operation.NotEqual, Emit.Name("foo"), Emit.Name("bar")), ScriptParser.Expression);
        [Fact] public void TestGt() => Test("a > 1", Emit.Op(Operation.Greater, Emit.Name("a"), Emit.Const(1)), ScriptParser.Expression);
        [Fact] public void TestLt() => Test("a < 1", Emit.Op(Operation.Lesser, Emit.Name("a"), Emit.Const(1)), ScriptParser.Expression);
        [Fact] public void TestGte() => Test("a >= 1", Emit.Op(Operation.GreaterEqual, Emit.Name("a"), Emit.Const(1)), ScriptParser.Expression);
        [Fact] public void TestLte() => Test("a <= 1", Emit.Op(Operation.LesserEqual, Emit.Name("a"), Emit.Const(1)), ScriptParser.Expression);

        [Fact] public void TestAnd() => Test("a == 1 && b != 2",
            Emit.Op(Operation.And,
                Emit.Op(Operation.Equal, Emit.Name("a"), Emit.Const(1)),
                Emit.Op(Operation.NotEqual, Emit.Name("b"), Emit.Const(2))),
            ScriptParser.Expression);

        [Fact] public void TestOr() => Test("a == 1 || b != 2", 
            Emit.Op(Operation.Or,
                Emit.Op(Operation.Equal, Emit.Name("a"), Emit.Const(1)),
                Emit.Op(Operation.NotEqual, Emit.Name("b"), Emit.Const(2))),
            ScriptParser.Expression);

        [Fact] public void TestAssign() => Test("foo = 12", Emit.Op(Operation.Assign, Emit.Name("foo"), Emit.Const(12)), ScriptParser.Expression);
        [Fact] public void TestAdd() => Test("x += 3", Emit.Op(Operation.Add, Emit.Name("x"), Emit.Const(3)), ScriptParser.Expression);
        [Fact] public void TestSub() => Test("x -= 3", Emit.Op(Operation.Subtract, Emit.Name("x"), Emit.Const(3)), ScriptParser.Expression);

        [Fact] public void ExpressionNameTest() => Test("foo", Emit.Name("foo"), ScriptParser.Expression); 
        [Fact] public void ExpressionNumberTest() => Test("100", Emit.Const(100), ScriptParser.Expression);
        [Fact] public void ExpressionIdentifierNameTest() => Test("foo", Emit.Name("foo"), ScriptParser.Expression);
        [Fact] public void ExpressionIdentifierConstTest() => Test("100", Emit.Const(100), ScriptParser.Expression);
        [Fact] public void ExpressionIdentifierZeroTest() => Test("0", Emit.Const(0), ScriptParser.Expression);
        [Fact] public void ExpressionMemberTest() => Test("a.b", Emit.Member(Emit.Name("a"), Emit.Name("b")), ScriptParser.Expression);
        [Fact] public void ExpressionMemberNumTest() => Test("a.100", Emit.Member(Emit.Name("a"), Emit.Const(100)), ScriptParser.Expression);
        [Fact] public void ExpressionIndexTest() => Test("a[b]", Emit.Index(Emit.Name("a"), Emit.Name("b")), ScriptParser.Expression);
        [Fact] public void ExpressionIndexNumTest() => Test("a[100]", Emit.Index(Emit.Name("a"), Emit.Const(100)), ScriptParser.Expression);

        [Fact] public void StatementNameTest()            => Test("foo",   S(Emit.Name("foo")), ScriptParser.Statement);
        [Fact] public void StatementNumberTest()          => Test("100",   S(Emit.Const(100)), ScriptParser.Statement);
        [Fact] public void StatementIdentifierNameTest()  => Test("foo",   S(Emit.Name("foo")), ScriptParser.Statement);
        [Fact] public void StatementIdentifierConstTest() => Test("100",   S(Emit.Const(100)), ScriptParser.Statement);
        [Fact] public void StatementIdentifierZeroTest()  => Test("0",     S(Emit.Const(0)), ScriptParser.Statement);
        [Fact] public void StatementMemberTest()          => Test("a.b",   S(Emit.Member(Emit.Name("a"), Emit.Name("b"))), ScriptParser.Statement);
        [Fact] public void StatementMemberNumTest()       => Test("a.100", S(Emit.Member(Emit.Name("a"), Emit.Const(100))), ScriptParser.Statement);
        [Fact] public void StatementIndexTest()           => Test("a[b]",  S(Emit.Index(Emit.Name("a"), Emit.Name("b"))), ScriptParser.Statement);
        [Fact] public void StatementIndexNumTest()        => Test("a[100]",S(Emit.Index(Emit.Name("a"), Emit.Const(100))), ScriptParser.Statement);
        [Fact] public void StatementStatementTest()       => Test("a b c", S(Emit.Name("a"), Emit.Name("b"), Emit.Name("c")), ScriptParser.Statement);
        [Fact] public void StatementLabelTest()           => Test("foo:",  Emit.Label("foo"), ScriptParser.Statement);

        [Fact] public void NegationNameTest() => Test("!foo", Emit.Negation(Emit.Name("foo")), ScriptParser.Negation); 
        [Fact] public void NegationNumberTest() => Test("!100", Emit.Negation(Emit.Const(100)), ScriptParser.Negation); 
        [Fact] public void NegationCompoundTest() =>
            Test("!(Ticker[100] == 23)",
                Emit.Negation(
                    Emit.Op(Operation.Equal,
                        Emit.Index(
                            Emit.Name("Ticker"),
                            Emit.Const(100)),
                        Emit.Const(23)
                    )
                ), ScriptParser.Negation);

        [Fact] public void ParenthesesTest() =>
            Test("(a > b && (c <= b))",
                Emit.Op(Operation.And,
                    Emit.Op(Operation.Greater, Emit.Name("a"), Emit.Name("b")),
                    Emit.Op(Operation.LesserEqual, Emit.Name("c"), Emit.Name("b"))),
                ScriptParser.Expression);

        [Fact] public void BlockTest() =>
            Test("{ a, b, c }",
                Emit.Seq(S(Emit.Name("a")), S(Emit.Name("b")), S(Emit.Name("c"))),
                ScriptParser.Block);

        [Fact] public void SequenceTest() =>
            Test("a, b, c",
                Emit.Seq(Emit.Statement(Emit.Name("a")), Emit.Statement(Emit.Name("b")), Emit.Statement(Emit.Name("c"))),
                ScriptParser.Sequence);

        [Fact] public void IfTest() =>
            Test("if(a) { b }",
                Emit.If(Emit.Name("a"), S(Emit.Name("b"))),
                ScriptParser.If);

        [Fact] public void IfElseTest() =>
            Test("if(a) { b } else {c}",
                Emit.IfElse(Emit.Name("a"), S(Emit.Name("b")), S(Emit.Name("c"))),
                ScriptParser.IfElse);

        [Fact] public void WhileTest() =>
            Test("while(a) { b }",
                Emit.While(Emit.Name("a"), S(Emit.Name("b"))),
                ScriptParser.While);

        [Fact] public void DoTest() =>
            Test("do {b} while(a)",
                Emit.Do(Emit.Name("a"), S(Emit.Name("b"))),
                ScriptParser.Do);

        [Fact] public void BreakTest() =>
            Test("while(a) { if (b) { break }, c}",
                Emit.While(Emit.Name("a"),
                    Emit.Seq(
                        Emit.If(Emit.Name("b"), Emit.Break()),
                        S(Emit.Name("c")))),
                ScriptParser.TopLevel);

        [Fact] public void ContinueTest() =>
            Test("while(a) { if (b) { continue }, c}",
                Emit.While(Emit.Name("a"),
                    Emit.Seq(
                        Emit.If(Emit.Name("b"), Emit.Continue()),
                        S(Emit.Name("c")))),
                ScriptParser.TopLevel);

        static void Test<T>(string source, ICfgNode expected, TokenListParser<ScriptToken, T> parser) where T : ICfgNode
        {
            var tokens = ScriptTokenizer.Tokenize(source);
            if (!tokens.HasValue)
                throw new InvalidOperationException($"Tokenization failure: {tokens.ErrorMessage} at {tokens.ErrorPosition}");

            var parsed = parser.TryParse(tokens.Value);
            if (!parsed.HasValue)
                throw new InvalidOperationException($"Parse failure: {parsed}");

            Assert.Equal(expected, parsed.Value);
        }
    }
}

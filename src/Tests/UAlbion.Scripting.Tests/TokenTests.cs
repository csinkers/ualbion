using System;
using System.Collections.Generic;
using System.Linq;
using Superpower.Model;
using Xunit;

namespace UAlbion.Scripting.Tests
{
    public class TokenTests
    {
        [Fact]
        public void AllTokens()
        {
            const string text = @"10 test ()[]{}.: = == != >= <= > < ! && || += -= ; comment";
            var expected = new (ScriptToken kind, string param)[]
            {
                (ScriptToken.Number, "10"),
                (ScriptToken.Identifier, "test"),
                (ScriptToken.LParen, "("),
                (ScriptToken.RParen, ")"),
                (ScriptToken.LBracket, "["),
                (ScriptToken.RBracket, "]"),
                (ScriptToken.LBrace, "{"),
                (ScriptToken.RBrace, "}"),
                (ScriptToken.Dot, "."),
                (ScriptToken.Colon, ":"),
                (ScriptToken.Assign, "="),
                (ScriptToken.Equal, "=="),
                (ScriptToken.NotEqual, "!="),
                (ScriptToken.GreaterEqual, ">="),
                (ScriptToken.LesserEqual, "<="),
                (ScriptToken.Greater, ">"),
                (ScriptToken.Lesser, "<"),
                (ScriptToken.Not, "!"),
                (ScriptToken.And, "&&"),
                (ScriptToken.Or, "||"),
                (ScriptToken.Add, "+="),
                (ScriptToken.Sub, "-="),
                (ScriptToken.Comment, "; comment"),
            };

            var result = ScriptTokenizer.Tokenize(text);
            Verify(expected, result);
        }

        [Fact]
        public void ScriptTokenTest1()
        {
            const string text = @"if (party[Tom].isPresent) {
    while (!party.hasItem[Axe] && party.gold < 100) {
        simple_chest 1 Axe ; Give an axe
        party.gold += 5
    }
}
";
            var expected = new (ScriptToken kind, string param)[]
            {
                (ScriptToken.Identifier, "if"),
                (ScriptToken.LParen, "("),
                (ScriptToken.Identifier, "party"),
                (ScriptToken.LBracket, "["),
                (ScriptToken.Identifier, "Tom"),
                (ScriptToken.RBracket, "]"),
                (ScriptToken.Dot, "."),
                (ScriptToken.Identifier, "isPresent"),
                (ScriptToken.RParen, ")"),
                (ScriptToken.LBrace, "{"),
                (ScriptToken.NewLine, Environment.NewLine),
                (ScriptToken.Identifier, "while"),
                (ScriptToken.LParen, "("),
                (ScriptToken.Not, "!"),
                (ScriptToken.Identifier, "party"),
                (ScriptToken.Dot, "."),
                (ScriptToken.Identifier, "hasItem"),
                (ScriptToken.LBracket, "["),
                (ScriptToken.Identifier, "Axe"),
                (ScriptToken.RBracket, "]"),
                (ScriptToken.And, "&&"),
                (ScriptToken.Identifier, "party"),
                (ScriptToken.Dot, "."),
                (ScriptToken.Identifier, "gold"),
                (ScriptToken.Lesser, "<"),
                (ScriptToken.Number, "100"),
                (ScriptToken.RParen, ")"),
                (ScriptToken.LBrace, "{"),
                (ScriptToken.NewLine, Environment.NewLine),
                (ScriptToken.Identifier, "simple_chest"),
                (ScriptToken.Number, "1"),
                (ScriptToken.Identifier, "Axe"),
                (ScriptToken.Comment, "; Give an axe"),
                (ScriptToken.NewLine, Environment.NewLine),
                (ScriptToken.Identifier, "party"),
                (ScriptToken.Dot, "."),
                (ScriptToken.Identifier, "gold"),
                (ScriptToken.Add, "+="),
                (ScriptToken.Number, "5"),
                (ScriptToken.NewLine, Environment.NewLine),
                (ScriptToken.RBrace, "}"),
                (ScriptToken.NewLine, Environment.NewLine),
                (ScriptToken.RBrace, "}"),
                    (ScriptToken.NewLine, Environment.NewLine),
            };

            var result = ScriptTokenizer.Tokenize(text);
            Verify(expected, result);
        }

        static void Verify(IList<(ScriptToken kind, string param)> expected, Result<TokenList<ScriptToken>> actual)
        {
            var list = actual.Value.ToList();
            Assert.Equal(expected.Count, list.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].kind, list[i].Kind);
                Assert.Equal(expected[i].param, list[i].Span.ToString());
            }
        }
    }
}

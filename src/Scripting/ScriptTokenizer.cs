using System;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace UAlbion.Scripting
{
    public static class ScriptTokenizer
    {
        static Tokenizer<ScriptToken> TokenParser { get; } = new TokenizerBuilder<ScriptToken>()
            .Match(Span.EqualTo(Environment.NewLine), ScriptToken.NewLine)
            .Ignore(Span.WhiteSpace)
            .Match(Span.EqualTo("&&"), ScriptToken.And)
            .Match(Span.EqualTo("||"), ScriptToken.Or)
            .Match(Span.EqualTo("+="), ScriptToken.Add)
            .Match(Span.EqualTo("-="), ScriptToken.Sub)
            .Match(Span.EqualTo("=="), ScriptToken.Equal)
            .Match(Span.EqualTo("!="), ScriptToken.NotEqual)
            .Match(Span.EqualTo(">="), ScriptToken.GreaterEqual)
            .Match(Span.EqualTo("<="), ScriptToken.LesserEqual)
            .Match(Character.EqualTo('>'), ScriptToken.Greater)
            .Match(Character.EqualTo('<'), ScriptToken.Lesser)
            .Match(Character.EqualTo('='), ScriptToken.Assign)
            .Match(Character.EqualTo('!'), ScriptToken.Not)
            .Match(Character.EqualTo('('), ScriptToken.LParen)
            .Match(Character.EqualTo(')'), ScriptToken.RParen)
            .Match(Character.EqualTo('['), ScriptToken.LBracket)
            .Match(Character.EqualTo(']'), ScriptToken.RBracket)
            .Match(Character.EqualTo('{'), ScriptToken.LBrace)
            .Match(Character.EqualTo('}'), ScriptToken.RBrace)
            .Match(Character.EqualTo('.'), ScriptToken.Dot)
            .Match(Character.EqualTo(':'), ScriptToken.Colon)
            .Match(Character.EqualTo(','), ScriptToken.Comma)
            .Match(Comment.ToEndOfLine(Span.EqualTo(';')), ScriptToken.Comment)
            .Match(Numerics.Natural, ScriptToken.Number)
            .Match(Identifier.CStyle, ScriptToken.Identifier)
            .Build();

        public static Result<TokenList<ScriptToken>> Tokenize(string input) => TokenParser.TryTokenize(input);
    }
}
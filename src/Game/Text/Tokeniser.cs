using System;
using System.Collections.Generic;
using System.Text;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Text;

public static class Tokeniser
{
    enum TokeniserState
    {
        Neutral,
        InBraces,
        InPercentage,
    }

    public static IEnumerable<(Token Token, object Argument)> Tokenise(string template)
    {
        if (template == null) yield break;
        var sb = new StringBuilder();
        var state = TokeniserState.Neutral;

        foreach (var c in template)
        {
            switch (state)
            {
                case TokeniserState.Neutral:
                    switch (c)
                    {
                        case '%':
                            if (sb.Length > 0)
                            {
                                yield return (Token.Text, sb.ToString());
                                sb.Clear();
                            }

                            state = TokeniserState.InPercentage;
                            break;

                        case '{':
                            if (sb.Length > 0)
                            {
                                yield return (Token.Text, sb.ToString());
                                sb.Clear();
                            }

                            state = TokeniserState.InBraces;
                            break;

                        case '^':
                            if (sb.Length > 0)
                            {
                                yield return (Token.Text, sb.ToString());
                                sb.Clear();
                            }

                            yield return (Token.NewLine, null);
                            break;

                        default:
                            sb.Append(c);
                            break;
                    }

                    break;

                case TokeniserState.InBraces:
                    switch (c)
                    {
                        case '}':
                            var inner = sb.ToString().Trim();
                            var token = GetBraceToken(inner);
                            if (token != null)
                                yield return token.Value;

                            state = TokeniserState.Neutral;
                            sb.Clear();
                            break;

                        default:
                            sb.Append(c);
                            break;
                    }

                    break;
                case TokeniserState.InPercentage:
                    switch (c)
                    {
                        case '%':
                            yield return (Token.Text, "%");
                            state = TokeniserState.Neutral;
                            sb.Clear();
                            break;

                        case 'l':
                            sb.Append(c);
                            break;

                        case 's':
                        case 'd':
                        case 'u':
                            sb.Append(c);
                            yield return (Token.Parameter, sb.ToString());
                            state = TokeniserState.Neutral;
                            sb.Clear();
                            break;

                        default:
                            yield return (Token.Text, "%" + c);
                            state = TokeniserState.Neutral;
                            sb.Clear();
                            break;
                    }

                    break;
            }
        }

        if (sb.Length > 0)
        {
            yield return (Token.Text, sb.ToString());
            sb.Clear();
        }
    }

 
    static readonly Dictionary<string, Token> SimpleBraceTokens = new()
    {
        { "HE",  Token.He },
        { "HIM", Token.Him },
        { "HIS", Token.His },
        { "ME",  Token.Me },
        { "CLAS", Token.Class },
        { "RACE", Token.Race },
        { "SEXC", Token.Sex },
        { "NAME", Token.Name },
        { "DAMG", Token.Damage },
        { "PRIC", Token.Price },
        { "COMB", Token.Combatant },
        { "INVE", Token.Inventory },
        { "SUBJ", Token.Subject },
        { "VICT", Token.Victim },
        { "WEAP", Token.Weapon },
        { "LEAD", Token.Leader },
        { "BIG",  Token.Big },
        { "FAT",  Token.Fat },
        { "LEFT", Token.Left },
        { "CNTR", Token.Centre },
        { "JUST", Token.Justify },
        { "FAHI", Token.FatHigh },
        { "HIGH", Token.High },
        { "NORS", Token.NormalSize },
        { "TECF", Token.Tecf },
        { "UNKN", Token.Unknown },
    };

    static (Token, object)? GetBraceToken(string inner)
    {
        if (SimpleBraceTokens.TryGetValue(inner, out var token))
            return (token, null);

        if (inner.StartsWith("BLOK", StringComparison.Ordinal))
        {
            var number = int.Parse(inner[4..]);
            return (Token.Block, number);
        }

        if (inner.StartsWith("INK ", StringComparison.Ordinal))
        {
            var number = int.Parse(inner[4..]);
            return (Token.Ink, new InkId(number));
        }

        if (inner.StartsWith("WORD", StringComparison.Ordinal))
        {
            var word = inner[4..];
            return (Token.Word, word);
        }

        return null;
    }
}

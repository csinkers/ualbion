using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace UAlbion.Game.Text
{
    public static class Tokeniser
    {
        enum TokeniserState
        {
            Neutral,
            InBraces,
            InPercentage,
        }

        public static IEnumerable<(Token, object)> Tokenise(string template)
        {
            if (template == null) yield break;
            StringBuilder sb = new StringBuilder();
            TokeniserState state = TokeniserState.Neutral;
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

                            default: sb.Append(c); break;
                        }
                        break;

                    case TokeniserState.InBraces:
                        switch (c)
                        {
                            case '}':
                                var inner = sb.ToString().Trim();
                                switch (inner)
                                {
                                    case "HE": yield return (Token.He, null); break;
                                    case "HIM": yield return (Token.Him, null); break;
                                    case "HIS": yield return (Token.His, null); break;
                                    case "ME": yield return (Token.Me, null); break;
                                    case "CLAS": yield return (Token.Class, null); break;
                                    case "RACE": yield return (Token.Race, null); break;
                                    case "SEXC": yield return (Token.Sex, null); break;
                                    case "NAME": yield return (Token.Name, null); break;
                                    case "DAMG": yield return (Token.Damage, null); break;
                                    case "PRIC": yield return (Token.Price, null); break;
                                    case "COMB": yield return (Token.Combatant, null); break;
                                    case "INVE": yield return (Token.Inventory, null); break;
                                    case "SUBJ": yield return (Token.Subject, null); break;
                                    case "VICT": yield return (Token.Victim, null); break;
                                    case "WEAP": yield return (Token.Weapon, null); break;
                                    case "LEAD": yield return (Token.Leader, null); break;
                                    case "BIG": yield return (Token.Big, null); break;
                                    case "FAT": yield return (Token.Fat, null); break;
                                    case "LEFT": yield return (Token.Left, null); break;
                                    case "CNTR": yield return (Token.Centre, null); break;
                                    case "JUST": yield return (Token.Justify, null); break;
                                    case "FAHI": yield return (Token.FatHigh, null); break;
                                    case "HIGH": yield return (Token.High, null); break;
                                    case "NORS": yield return (Token.NormalSize, null); break;
                                    case "TECF": yield return (Token.Tecf, null); break;
                                    case "UNKN": yield return (Token.Unknown, null); break;
                                    default:
                                        if (inner.StartsWith("BLOK", StringComparison.Ordinal))
                                        {
                                            var number = int.Parse(inner.Substring(4), CultureInfo.InvariantCulture);
                                            yield return (Token.Block, number);
                                        }
                                        else if (inner.StartsWith("INK ", StringComparison.Ordinal))
                                        {
                                            var number = int.Parse(inner.Substring(4), CultureInfo.InvariantCulture);
                                            yield return (Token.Ink, number);
                                        }
                                        else if (inner.StartsWith("WORD", StringComparison.Ordinal))
                                        {
                                            var word = inner.Substring(4);
                                            yield return (Token.Word, word);
                                        }
                                        break;
                                }

                                state = TokeniserState.Neutral;
                                sb.Clear();
                                break;

                            default: sb.Append(c); break;
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
    }
}

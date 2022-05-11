using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Text;

public class TextFormatter : ServiceComponent<ITextFormatter>, ITextFormatter
{
    ICharacterSheet _leader;
    ICharacterSheet _subject;
    ICharacterSheet _inventory;
    ICharacterSheet _combatant;
    ICharacterSheet _victim;
    ItemData _weapon;

    public TextFormatter()
    {
        On<SetContextEvent>(e =>
        {
            var assets = Resolve<IAssetManager>();
            var state = Resolve<IGameState>();

            var asset = e.AssetId.Type switch
            {
                AssetType.Party => (object)state.GetSheet(e.AssetId), // TODO: Load game state assets via AssetManager?
                AssetType.Npc => state.GetSheet(e.AssetId),
                AssetType.Monster => assets.LoadSheet(e.AssetId),
                AssetType.Item => assets.LoadItem(e.AssetId),
                _ => null
            };

            switch (e.Type)
            {
                case ContextType.Leader: _leader = (ICharacterSheet)asset; break;
                case ContextType.Subject: _subject = (ICharacterSheet)asset; break;
                case ContextType.Inventory: _inventory = (ICharacterSheet)asset; break;
                case ContextType.Combatant: _combatant = (ICharacterSheet)asset; break;
                case ContextType.Victim: _victim = (ICharacterSheet)asset; break;
                case ContextType.Weapon: _weapon = (ItemData)asset; break;
            }
        });
    }

    public ITextFormatter NoWrap() => new CustomisedTextFormatter(this).NoWrap();
    public ITextFormatter Left() => new CustomisedTextFormatter(this).Left();
    public ITextFormatter Center() => new CustomisedTextFormatter(this).Center();
    public ITextFormatter Right() => new CustomisedTextFormatter(this).Right();
    public ITextFormatter Justify() => new CustomisedTextFormatter(this).Justify();
    public ITextFormatter Fat() => new CustomisedTextFormatter(this).Fat();
    public ITextFormatter Ink(FontColor color) => new CustomisedTextFormatter(this).Ink(color);
    public ITextFormatter Block(int blockNumber) => new CustomisedTextFormatter(this).Block(blockNumber);

    IEnumerable<(Token, object)> Substitute(
        IAssetManager assets,
        IEnumerable<(Token, object)> tokens,
        object[] args)
    {
        object active = null;
        int argNumber = 0;
        foreach(var (token, p) in tokens)
        {
            switch(token)
            {
                case Token.Damage: throw new NotImplementedException();
                case Token.Me: throw new NotImplementedException();

                case Token.Class:
                {
                    if (active is not ICharacterSheet character)
                    {
                        yield return (Token.Text, "{CLAS}");
                        break; // throw new FormatException($"Expected the active item to be a character, was actually {active ?? "null"}");
                    }

                    switch (character.PlayerClass)
                    {
                        case PlayerClass.Pilot: yield return (Token.Text, assets.LoadString(Base.SystemText.Class_Pilot)); break;
                        case PlayerClass.Scientist: yield return (Token.Text, assets.LoadString(Base.SystemText.Class_Scientist)); break;
                        case PlayerClass.IskaiWarrior: yield return (Token.Text, assets.LoadString(Base.SystemText.Class_Warrior)); break;
                        case PlayerClass.DjiKasMage: yield return (Token.Text, assets.LoadString(Base.SystemText.Class_DjiKasMage)); break;
                        case PlayerClass.Druid: yield return (Token.Text, assets.LoadString(Base.SystemText.Class_Druid)); break;
                        case PlayerClass.EnlightenedOne: yield return (Token.Text, assets.LoadString(Base.SystemText.Class_EnlightenedOne)); break;
                        case PlayerClass.Technician: yield return (Token.Text, assets.LoadString(Base.SystemText.Class_Technician)); break;
                        case PlayerClass.OquloKamulos: yield return (Token.Text, assets.LoadString(Base.SystemText.Class_OquloKamulos)); break;
                        case PlayerClass.Warrior: yield return (Token.Text, assets.LoadString(Base.SystemText.Class_Warrior2)); break;
                        case PlayerClass.Monster: yield return (Token.Text, "Monster"); break;
                        default: throw new InvalidEnumArgumentException(nameof(character.PlayerClass), (int)character.PlayerClass, typeof(PlayerClass));
                    }
                    break;
                }

                case Token.He:
                case Token.Him:
                case Token.His:
                {
                    if (active is not ICharacterSheet character)
                    {
                        yield return (Token.Text, $"{{{token}}}");
                        break; // throw new FormatException($"Expected the active item to be a character, was actually {active ?? "null"}");
                    }

                    var word = (token, character.Gender) switch
                    {
                        (Token.He, Gender.Male) => Base.SystemText.Meta_He,
                        (Token.He, Gender.Female) => Base.SystemText.Meta_She,
                        (Token.He, Gender.Neuter) => Base.SystemText.Meta_ItNominative,
                        (Token.Him, Gender.Male) => Base.SystemText.Meta_HimAccusative,
                        (Token.Him, Gender.Female) => Base.SystemText.Meta_HerAccusative,
                        (Token.Him, Gender.Neuter) => Base.SystemText.Meta_ItAccusative,
                        (Token.His, Gender.Male) => Base.SystemText.Meta_His,
                        (Token.His, Gender.Female) => Base.SystemText.Meta_Her,
                        (Token.His, Gender.Neuter) => Base.SystemText.Meta_Its,
                        _ => throw new NotImplementedException()
                    };

                    yield return (Token.Text, assets.LoadString(word));
                    break;
                }

                case Token.Name:
                {
                    if (active is ICharacterSheet character)
                    {
                        var language = Resolve<ISettings>().Gameplay.Language;
                        yield return (Token.Text, character.GetName(language));
                    }
                    else if (active is ItemData item)
                        yield return (Token.Text, item.Name);
                    else 
                        yield return (Token.Text, "{NAME}");
                    break;
                }

                case Token.Price:
                {
                    if (active is not ItemData item)
                    {
                        yield return (Token.Text, "{PRIC}");
                        break; // throw new FormatException($"Expected the active item to be an item, was actually {active ?? "null"}");
                    }

                    yield return (Token.Text, $"${item.Value/10}.{item.Value % 10}"); // TODO: Does this need extra logic?
                    break;
                }

                case Token.Race:
                {
                    if (active is not ICharacterSheet character)
                    {
                        yield return (Token.Text, "{RACE}");
                        break; // throw new FormatException($"Expected the active item to be a character, was actually {active ?? "null"}");
                    }

                    switch (character.Race)
                    {
                        case PlayerRace.Terran: yield return (Token.Text, assets.LoadString(Base.SystemText.Race_Terran)); break;
                        case PlayerRace.Iskai: yield return (Token.Text, assets.LoadString(Base.SystemText.Race_Iskai)); break;
                        case PlayerRace.Celt: yield return (Token.Text, assets.LoadString(Base.SystemText.Race_Celt)); break;
                        case PlayerRace.KengetKamulos: yield return (Token.Text, assets.LoadString(Base.SystemText.Race_KengetKamulos)); break;
                        case PlayerRace.DjiCantos: yield return (Token.Text, assets.LoadString(Base.SystemText.Race_DjiCantos)); break;
                        case PlayerRace.Mahino: yield return (Token.Text, assets.LoadString(Base.SystemText.Race_Mahino)); break;
                        case PlayerRace.Decadent: yield return (Token.Text, assets.LoadString(Base.SystemText.Race_Decadent)); break;
                        case PlayerRace.Umajo: yield return (Token.Text, assets.LoadString(Base.SystemText.Race_Umajo)); break;
                        case PlayerRace.Monster: yield return (Token.Text, assets.LoadString(Base.SystemText.Race_Monster)); break;
                        default: throw new InvalidEnumArgumentException(nameof(character.Race), (int)character.Race, typeof(PlayerRace));
                    }
                    break;
                }

                case Token.Sex:
                {
                    if (active is not ICharacterSheet character)
                    {
                        yield return (Token.Text, "{SEX}");
                        break; // throw new FormatException($"Expected the active item to be a character, was actually {active ?? "null"}");
                    }

                    switch (character.Gender)
                    {
                        case Gender.Male: yield return (Token.Text, "♂"); break;
                        case Gender.Female: yield return (Token.Text, "♀"); break;
                    }

                    break;
                }

                // Change context
                case Token.Combatant: active = _combatant; break;
                case Token.Inventory: active = _inventory; break;
                case Token.Leader: active = _leader; break;
                case Token.Subject: active = _subject; break;
                case Token.Victim: active = _victim; break;
                case Token.Weapon: active = _weapon; break;

                case Token.Parameter:
                    yield return (Token.Text, args[argNumber].ToString());
                    argNumber++;
                    break;

                default: yield return (token, p); break;
            }
        }
    }

    static IEnumerable<TextBlock> TokensToBlocks(IWordLookup wordLookup, IEnumerable<(Token, object)> tokens, string raw)
    {
        var sb = new StringBuilder();
        var block = new TextBlock { Raw = raw };

        foreach (var (token, p) in tokens)
        {
            if (sb.Length > 0 && token != Token.Text || token == Token.Block)
            {
                block.Text = sb.ToString();
                yield return block;

                sb.Clear();
                int blockId = token == Token.Block ? (int)p : block.BlockId;
                block = new TextBlock(blockId)
                {
                    Alignment = block.Alignment,
                    Style = block.Style,
                    Color = block.Color,
                };
                block.Raw = raw;
            }

            switch (token)
            {
                case Token.Ink: block.Color = (FontColor)(int)p; break;

                case Token.NormalSize: block.Style = TextStyle.Normal; break;
                case Token.Big: block.Style = TextStyle.Big; break;
                case Token.Fat: block.Style = TextStyle.Fat; break;
                case Token.FatHigh: block.Style = TextStyle.FatAndHigh; break;
                case Token.High: block.Style = TextStyle.High; break;

                case Token.Left: block.Alignment = TextAlignment.Left; break;
                case Token.Centre: block.Alignment = TextAlignment.Center; break;
                case Token.Right: block.Alignment = TextAlignment.Right; break;
                case Token.Justify: block.Alignment = TextAlignment.Justified; break;

                case Token.NewLine: block.ArrangementFlags |= TextArrangementFlags.ForceNewLine; break;
                case Token.NoWrap: block.ArrangementFlags |= TextArrangementFlags.NoWrap; break;

                case Token.Text:
                    sb.Append((string) p);
                    break;

                case Token.Block: break; // Handled above
                case Token.Tecf: break; // ???

                case Token.Word:
                {
                    WordId word = wordLookup.Parse((string)p);
                    if (word.IsNone)
                        sb.Append((string) p);
                    else
                        block.AddWord(word);
                    break;
                }
            }
        }

        if (sb.Length > 0)
        {
            block.Text = sb.ToString();
            yield return block;
        }
    }

    IEnumerable<TextBlock> InnerFormat(string template, object[] arguments, IList<(Token, object)> implicitTokens, IAssetManager assets)
    {
        PerfTracker.IncrementFrameCounter("Format text");

        var tokens = Tokeniser.Tokenise(template);
        if (implicitTokens != null)
            tokens = implicitTokens.Concat(tokens);

        IEnumerable<(Token, object)> substituted = Substitute(assets, tokens, arguments);

#if DEBUG
        substituted = substituted.ToList();
#endif

        var blocks = TokensToBlocks(Resolve<IWordLookup>(), substituted, template);

#if DEBUG
        blocks = blocks.ToList();
#endif

        return blocks;
    }

    public IText Format(TextId textId, params object[] arguments)
        => Format(textId, null, arguments);

    public IText Format(StringId stringId, params object[] arguments)
        => Format(stringId, null, arguments);

    public IText Format(string template, params object[] arguments)
        => Format(template, null, arguments);

    public IText Format(TextId textId, IList<(Token, object)> implicitTokens, params object[] arguments)
        => Format((StringId)textId, implicitTokens, arguments);

    public IText Format(StringId stringId, IList<(Token, object)> implicitTokens, params object[] arguments)
        => new DynamicText(() =>
        {
            var assets = Resolve<IAssetManager>();
            string template = assets.LoadString(stringId);
            return InnerFormat(template, arguments, implicitTokens, assets);
        });

    public IText Format(string template, IList<(Token, object)> implicitTokens, params object[] arguments)
        => new DynamicText(() =>
        {
            var assets = Resolve<IAssetManager>();
            return InnerFormat(template, arguments, implicitTokens, assets);
        });
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Text
{
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

                var asset = e.AssetType switch
                {
                    AssetType.PartyMember => (object)state.GetPartyMember((PartyCharacterId)e.AssetId),
                    AssetType.Npc => state.GetNpc((NpcCharacterId)e.AssetId),
                    AssetType.Monster => assets.LoadMonster((MonsterCharacterId)e.AssetId),
                    AssetType.ItemList => assets.LoadItem((ItemId)e.AssetId),
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
        public ITextFormatter Language(GameLanguage language) => new CustomisedTextFormatter(this).Language(language);

        IEnumerable<(Token, object)> Substitute(
            IAssetManager assets,
            GameLanguage language,
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
                        if (!(active is ICharacterSheet character))
                        {
                            yield return (Token.Text, "{CLAS}");
                            break; // throw new FormatException($"Expected the active item to be a character, was actually {active ?? "null"}");
                        }

                        switch (character.PlayerClass)
                        {
                            case PlayerClass.Pilot: yield return (Token.Text, assets.LoadString(SystemTextId.Class_Pilot, language)); break;
                            case PlayerClass.Scientist: yield return (Token.Text, assets.LoadString(SystemTextId.Class_Scientist, language)); break;
                            case PlayerClass.IskaiWarrior: yield return (Token.Text, assets.LoadString(SystemTextId.Class_Warrior, language)); break;
                            case PlayerClass.DjiKasMage: yield return (Token.Text, assets.LoadString(SystemTextId.Class_DjiKasMage, language)); break;
                            case PlayerClass.Druid: yield return (Token.Text, assets.LoadString(SystemTextId.Class_Druid, language)); break;
                            case PlayerClass.EnlightenedOne: yield return (Token.Text, assets.LoadString(SystemTextId.Class_EnlightenedOne, language)); break;
                            case PlayerClass.Technician: yield return (Token.Text, assets.LoadString(SystemTextId.Class_Technician, language)); break;
                            case PlayerClass.OquloKamulos: yield return (Token.Text, assets.LoadString(SystemTextId.Class_OquloKamulos, language)); break;
                            case PlayerClass.Warrior: yield return (Token.Text, assets.LoadString(SystemTextId.Class_Warrior2, language)); break;
                            case PlayerClass.Monster: yield return (Token.Text, "Monster"); break;
                            default: throw new InvalidEnumArgumentException(nameof(character.PlayerClass), (int)character.PlayerClass, typeof(PlayerClass));
                        }
                        break;
                    }

                    case Token.He:
                    case Token.Him:
                    case Token.His:
                    {
                        if (!(active is ICharacterSheet character))
                        {
                            yield return (Token.Text, $"{{{token}}}");
                            break; // throw new FormatException($"Expected the active item to be a character, was actually {active ?? "null"}");
                        }

                        var word = (token, character.Gender) switch
                            {
                                (Token.He, Gender.Male) => SystemTextId.Meta_He,
                                (Token.He, Gender.Female) => SystemTextId.Meta_She,
                                (Token.He, Gender.Neuter) => SystemTextId.Meta_ItNominative,
                                (Token.Him, Gender.Male) => SystemTextId.Meta_HimAccusative,
                                (Token.Him, Gender.Female) => SystemTextId.Meta_HerAccusative,
                                (Token.Him, Gender.Neuter) => SystemTextId.Meta_ItAccusative,
                                (Token.His, Gender.Male) => SystemTextId.Meta_His,
                                (Token.His, Gender.Female) => SystemTextId.Meta_Her,
                                (Token.His, Gender.Neuter) => SystemTextId.Meta_Its,
                                _ => throw new NotImplementedException()
                            };

                        yield return (Token.Text, assets.LoadString(word, language));
                        break;
                    }

                    case Token.Name:
                    {
                        if (active is ICharacterSheet character)
                            yield return (Token.Text, character.GetName(language));
                        else if (active is ItemData item)
                            yield return (Token.Text, assets.LoadString(item.Id, language));
                        else 
                            yield return (Token.Text, "{NAME}");
                        break;
                    }

                    case Token.Price:
                    {
                        if (!(active is ItemData item))
                        {
                            yield return (Token.Text, "{PRIC}");
                            break; // throw new FormatException($"Expected the active item to be an item, was actually {active ?? "null"}");
                        }

                        yield return (Token.Text, $"${item.Value/10}.{item.Value % 10}"); // TODO: Does this need extra logic?
                        break;
                    }

                    case Token.Race:
                    {
                        if (!(active is ICharacterSheet character))
                        {
                            yield return (Token.Text, "{RACE}");
                            break; // throw new FormatException($"Expected the active item to be a character, was actually {active ?? "null"}");
                        }

                        switch (character.Race)
                        {
                            case PlayerRace.Terran: yield return (Token.Text, assets.LoadString(SystemTextId.Race_Terran, language)); break;
                            case PlayerRace.Iskai: yield return (Token.Text, assets.LoadString(SystemTextId.Race_Iskai, language)); break;
                            case PlayerRace.Celt: yield return (Token.Text, assets.LoadString(SystemTextId.Race_Celt, language)); break;
                            case PlayerRace.KengetKamulos: yield return (Token.Text, assets.LoadString(SystemTextId.Race_KengetKamulos, language)); break;
                            case PlayerRace.DjiCantos: yield return (Token.Text, assets.LoadString(SystemTextId.Race_DjiCantos, language)); break;
                            case PlayerRace.Mahino: yield return (Token.Text, assets.LoadString(SystemTextId.Race_Mahino, language)); break;
                            case PlayerRace.Decadent: yield return (Token.Text, assets.LoadString(SystemTextId.Race_Decadent, language)); break;
                            case PlayerRace.Umajo: yield return (Token.Text, assets.LoadString(SystemTextId.Race_Umajo, language)); break;
                            case PlayerRace.Monster: yield return (Token.Text, assets.LoadString(SystemTextId.Race_Monster, language)); break;
                            default: throw new InvalidEnumArgumentException(nameof(character.Race), (int)character.Race, typeof(PlayerRace));
                        }
                        break;
                    }

                    case Token.Sex:
                    {
                        if (!(active is ICharacterSheet character))
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

        static IEnumerable<TextBlock> TokensToBlocks(IAssetManager assets, IEnumerable<(Token, object)> tokens, string raw)
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
                        WordId? word = assets.ParseWord((string)p);
                        if (word == null)
                        {
                            sb.Append((string)p);
                        }
                        else
                        {
                            // sb.Append(assets.LoadString(word.Value, language));
                            block.AddWord(word.Value);
                        }
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

        IEnumerable<TextBlock> InnerFormat(string template, object[] arguments, IList<(Token, object)> implicitTokens, IAssetManager assets, GameLanguage language)
        {
            PerfTracker.IncrementFrameCounter("Format text");

            var tokens = Tokeniser.Tokenise(template);
            if (implicitTokens != null)
                tokens = implicitTokens.Concat(tokens);

            IEnumerable<(Token, object)> substituted = Substitute(assets, language, tokens, arguments);
            return TokensToBlocks(assets, substituted, template);
        }

        public IText Format(StringId stringId, params object[] arguments)
            => Format(stringId, null, null, arguments);

        public IText Format(string template, params object[] arguments)
            => Format(template, null, null, arguments);

        public IText Format(StringId stringId, IList<(Token, object)> implicitTokens, GameLanguage? language, params object[] arguments)
            => new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var curLanguage = language ?? Resolve<ISettings>().Gameplay.Language;
                string template = assets.LoadString(stringId, curLanguage);
                return InnerFormat(template, arguments, implicitTokens, assets, curLanguage);
            });

        public IText Format(string template, IList<(Token, object)> implicitTokens, GameLanguage? language, params object[] arguments)
            => new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var curLanguage = language ?? Resolve<ISettings>().Gameplay.Language;
                return InnerFormat(template, arguments, implicitTokens, assets, curLanguage);
            });
    }
}

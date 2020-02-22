using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Text
{
    public class TextFormatter
    {
        readonly IAssetManager _assets;
        readonly IList<(Token, object)> _implicitTokens = new List<(Token, object)>();
        ICharacterSheet _leader;
        ICharacterSheet _subject;
        ICharacterSheet _inventory;
        ICharacterSheet _combatant;
        ICharacterSheet _victim;
        ItemData _weapon;
        GameLanguage _language;

        public TextFormatter(IAssetManager assets, GameLanguage language)
        {
            _assets = assets;
            _language = language;
        }

        public TextFormatter Leader(ICharacterSheet character) { _leader = character; return this; }
        public TextFormatter Subject(ICharacterSheet character) { _subject = character; return this; }
        public TextFormatter Inventory(ICharacterSheet character) { _inventory = character; return this; }
        public TextFormatter Combatant(ICharacterSheet character) { _combatant = character; return this; }
        public TextFormatter Victim(ICharacterSheet character) { _victim = character; return this; }
        public TextFormatter Weapon(ItemData weapon) { _weapon = weapon; return this; }
        public TextFormatter Language(GameLanguage language) { _language = language; return this; }
        public TextFormatter NoWrap() { _implicitTokens.Add((Token.NoWrap, null)); return this; }
        public TextFormatter Centre() { _implicitTokens.Add((Token.Centre, null)); return this; }

        IEnumerable<(Token, object)> Substitute(IEnumerable<(Token, object)> tokens, object[] args)
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
                            throw new FormatException($"Expected the active item to be a character, was actually {active}");
                        switch (character.Class)
                        {
                            case PlayerClass.Pilot: yield return (Token.Text, _assets.LoadString(SystemTextId.Class_Pilot, _language)); break;
                            case PlayerClass.Scientist: yield return (Token.Text, _assets.LoadString(SystemTextId.Class_Scientist, _language)); break;
                            case PlayerClass.IskaiWarrior: yield return (Token.Text, _assets.LoadString(SystemTextId.Class_Warrior1, _language)); break;
                            case PlayerClass.DjiKasMage: yield return (Token.Text, _assets.LoadString(SystemTextId.Class_DjiKasMage, _language)); break;
                            case PlayerClass.Druid: yield return (Token.Text, _assets.LoadString(SystemTextId.Class_Druid, _language)); break;
                            case PlayerClass.EnlightenedOne: yield return (Token.Text, _assets.LoadString(SystemTextId.Class_EnlightenedOne, _language)); break;
                            case PlayerClass.Technician: yield return (Token.Text, _assets.LoadString(SystemTextId.Class_Technician, _language)); break;
                            case PlayerClass.OquloKamulos: yield return (Token.Text, _assets.LoadString(SystemTextId.Class_OquloKamulos, _language)); break;
                            case PlayerClass.Warrior: yield return (Token.Text, _assets.LoadString(SystemTextId.Class_Warrior2, _language)); break;
                            case PlayerClass.Monster: yield return (Token.Text, "Monster"); break;
                            default: throw new ArgumentOutOfRangeException();
                        }
                        break;
                    }

                    case Token.He:
                    case Token.Him: 
                    case Token.His: 
                    {
                        if (!(active is ICharacterSheet character))
                            throw new FormatException($"Expected the active item to be a character, was actually {active}");

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

                        yield return (Token.Text, _assets.LoadString(word, _language));
                        break;
                    }

                    case Token.Name: 
                    {
                        if (active is ICharacterSheet character)
                            yield return (Token.Text, character.GetName(_language));

                        if (active is ItemData item)
                            yield return (Token.Text, item.GetName(_language));
                        break;
                    }

                    case Token.Price:
                    {
                        if (!(active is ItemData item))
                            throw new FormatException($"Expected the active item to be an item, was actually {active}");
                        yield return (Token.Text, $"${item.Value/10}.{item.Value % 10}"); // TODO: Does this need extra logic?
                        break;
                    }

                    case Token.Race:
                    {
                        if (!(active is ICharacterSheet character))
                            throw new FormatException($"Expected the active item to be a character, was actually {active}");
                        switch (character.Race)
                        {
                            case PlayerRace.Terran: yield return (Token.Text, _assets.LoadString(SystemTextId.Race_Terran, _language)); break;
                            case PlayerRace.Iskai: yield return (Token.Text, _assets.LoadString(SystemTextId.Race_Iskai, _language)); break;
                            case PlayerRace.Celt: yield return (Token.Text, _assets.LoadString(SystemTextId.Race_Celt, _language)); break;
                            case PlayerRace.KengetKamulos: yield return (Token.Text, _assets.LoadString(SystemTextId.Race_KengetKamulos, _language)); break;
                            case PlayerRace.DjiCantos: yield return (Token.Text, _assets.LoadString(SystemTextId.Race_DjiCantos, _language)); break;
                            case PlayerRace.Mahino: yield return (Token.Text, _assets.LoadString(SystemTextId.Race_Mahino, _language)); break;
                            default: throw new ArgumentOutOfRangeException();
                        }
                        break;
                    }

                    case Token.Sex:
                    {
                        if (!(active is ICharacterSheet character))
                            throw new FormatException($"Expected the active item to be a character, was actually {active}");
                        switch (character.Gender)
                        {
                            case Gender.Male: yield return (Token.Text, "♂"); break;
                            case Gender.Female: yield return (Token.Text, "♀"); break;
                        }

                        break;
                    }

                    case Token.Word:
                    { 
                        WordId? word = _assets.ParseWord((string)p);
                        if(word == null)
                            yield return (Token.Text, p);
                        else
                            yield return (Token.Text, _assets.LoadString(word.Value, _language));
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

        IEnumerable<TextBlock> TokensToBlocks(IEnumerable<(Token, object)> tokens)
        {
            var sb = new StringBuilder();
            var block = new TextBlock();
            foreach (var (token, p) in tokens)
            {
                if (sb.Length > 0 && token != Token.Text)
                {
                    block.Text = sb.ToString();
                    yield return block;
                    sb.Clear();
                    block = new TextBlock();
                }

                switch(token)
                {
                    case Token.Ink: block.Color = (FontColor)(int)p; break;

                    case Token.NormalSize: block.Style = TextStyle.Normal; break;
                    case Token.Big: block.Style = TextStyle.Big; break;
                    case Token.Fat: block.Style = TextStyle.Fat; break;
                    case Token.FatHigh: block.Style = TextStyle.FatAndHigh; break;
                    case Token.High: block.Style = TextStyle.High; break;

                    case Token.Left: block.Alignment = TextAlignment.Left; break;
                    case Token.Centre: block.Alignment = TextAlignment.Center; break;
                    case Token.Justify: block.Alignment = TextAlignment.Justified; break;

                    case Token.NewLine: block.Arrangement |= TextArrangement.ForceNewLine; break;
                    case Token.NoWrap: block.Arrangement |= TextArrangement.NoWrap; break;

                    case Token.Text:
                        sb.Append((string) p);
                        break;

                    case Token.Block: break; // ???
                    case Token.Tecf: break; // ???
                }
            }

            if (sb.Length > 0)
            {
                block.Text = sb.ToString();
                yield return block;
            }
        }

        public TextFormatResult Format(SystemTextId template, params object[] arguments)
        {
            var templateText = _assets.LoadString(template, _language);
            return Format(templateText, arguments);
        }

        public TextFormatResult Format(string template, params object[] arguments)
        {
            var tokens = 
                _implicitTokens.Concat(
                    Tokeniser.Tokenise(template)
                ).ToList();
            var words = tokens
                .Where(x => x.Item1 == Token.Word)
                .Select(x => (string)x.Item2)
                .Select(Enum.Parse<WordId>)
                .ToList();

            var substituted = Substitute(tokens, arguments);
            var blocks = TokensToBlocks(substituted);
            return new TextFormatResult(blocks, words);
        }
    }

    public class TextFormatResult
    {
        public TextFormatResult(IEnumerable<TextBlock> blocks, IList<WordId> words)
        {
            Blocks = blocks;
            Words = words;
        }

        public IEnumerable<TextBlock> Blocks { get; }
        public IList<WordId> Words { get; }
    }
}

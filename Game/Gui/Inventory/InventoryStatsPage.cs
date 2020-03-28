using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Entities;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryStatsPage : UiElement // Stats
    {
        readonly PartyCharacterId _activeCharacter;

        IEnumerable<TextBlock> BuildHoverText(SystemTextId id, Func<ICharacterSheet, int> getValue, Func<ICharacterSheet, int> getMax)
        {
            var assets = Resolve<IAssetManager>();
            var party = Resolve<IParty>();
            var settings = Resolve<ISettings>();
            var formatter = new TextFormatter(assets, settings.Gameplay.Language);
            var member = party[_activeCharacter]?.Apparent;
            if (member == null)
                yield break;

            var block = formatter.Format(assets.LoadString(id, settings.Gameplay.Language)).Blocks.First();
            block.Text += $" {getValue(member)} / {getMax(member)}";
            yield return block;
        }

        public InventoryStatsPage(PartyCharacterId activeCharacter)
        {
            _activeCharacter = activeCharacter;
            StringId S(SystemTextId id) => new StringId(AssetType.SystemText, 0, (int)id);

            ProgressBar Progress(SystemTextId id, Func<ICharacterSheet, int> getValue, Func<ICharacterSheet, int> getMax)
            {
                var source = new DynamicText(() => BuildHoverText(id, getValue, getMax));
                return new ProgressBar(source,
                    () =>
                    {
                        var member = Resolve<IParty>()[activeCharacter];
                        return member == null ? 0 : getValue(member.Apparent);
                    },
                    () =>
                    {
                        var member = Resolve<IParty>()[activeCharacter];
                        return member == null ? 0 : getMax(member.Apparent);
                    },
                    100);
            }

            var stack = new VerticalStack(
                new Header(S(SystemTextId.Inv2_Attributes)),
                new HorizontalStack(
                    new VerticalStack(
                        new TextBlockElement(S(SystemTextId.Attrib_STR)).Right(),
                        new Spacing(0,2),
                        new TextBlockElement(S(SystemTextId.Attrib_INT)).Right(),
                        new Spacing(0,2),
                        new TextBlockElement(S(SystemTextId.Attrib_DEX)).Right(),
                        new Spacing(0,2),
                        new TextBlockElement(S(SystemTextId.Attrib_SPD)).Right(),
                        new Spacing(0,2),
                        new TextBlockElement(S(SystemTextId.Attrib_STA)).Right(),
                        new Spacing(0,2),
                        new TextBlockElement(S(SystemTextId.Attrib_LUC)).Right(),
                        new Spacing(0,2),
                        new TextBlockElement(S(SystemTextId.Attrib_MR)).Right(),
                        new Spacing(0,2),
                        new TextBlockElement(S(SystemTextId.Attrib_MT)).Right()
                    ),
                    new Spacing(2,0),
                    new VerticalStack(
                        new Spacing(105, 0),
                        Progress(SystemTextId.Attrib_Strength, x => x.Attributes.Strength, x => x.Attributes.StrengthMax),
                        new Spacing(0,3),
                        Progress(SystemTextId.Attrib_Intelligence, x => x.Attributes.Intelligence, x => x.Attributes.IntelligenceMax),
                        new Spacing(0,3),
                        Progress(SystemTextId.Attrib_Dexterity, x => x.Attributes.Dexterity, x => x.Attributes.DexterityMax),
                        new Spacing(0,3),
                        Progress(SystemTextId.Attrib_Speed, x => x.Attributes.Speed, x => x.Attributes.SpeedMax),
                        new Spacing(0,3),
                        Progress(SystemTextId.Attrib_Stamina, x => x.Attributes.Stamina, x => x.Attributes.StaminaMax),
                        new Spacing(0,3),
                        Progress(SystemTextId.Attrib_Luck, x => x.Attributes.Luck, x => x.Attributes.LuckMax),
                        new Spacing(0,3),
                        Progress(SystemTextId.Attrib_MagicResistance, x => x.Attributes.MagicResistance, x => x.Attributes.MagicResistanceMax),
                        new Spacing(0,3),
                        Progress(SystemTextId.Attrib_MagicTalent, x => x.Attributes.MagicTalent, x => x.Attributes.MagicTalentMax)
                    )
                ),
                new Header(S(SystemTextId.Inv2_Skills)),
                new HorizontalStack(
                    new VerticalStack(
                        new TextBlockElement(S(SystemTextId.Skill_CLO)).Right(),
                        new Spacing(0,2),
                        new TextBlockElement(S(SystemTextId.Skill_LON)).Right(),
                        new Spacing(0,2),
                        new TextBlockElement(S(SystemTextId.Skill_CRI)).Right(),
                        new Spacing(0,2),
                        new TextBlockElement(S(SystemTextId.Skill_LP)).Right()
                    ),
                    new Spacing(2,0),
                    new VerticalStack(
                        new Spacing(105, 0),
                        Progress(SystemTextId.Skill_CloseRangeCombat, x => x.Skills.CloseCombat, x => x.Skills.CloseCombatMax),
                        new Spacing(0,3),
                        Progress(SystemTextId.Skill_LongRangeCombat, x => x.Skills.RangedCombat, x => x.Skills.RangedCombatMax),
                        new Spacing(0,3),
                        Progress(SystemTextId.Skill_CriticalHit, x => x.Skills.CriticalChance, x => x.Skills.CriticalChanceMax),
                        new Spacing(0,3),
                        Progress(SystemTextId.Skill_Lockpicking, x => x.Skills.LockPicking, x => x.Skills.LockPickingMax)
                    )
                )
            );

            Children.Add(new HorizontalStack(new Spacing(4,0), stack, new Spacing(4,0)));
        }
    }
}

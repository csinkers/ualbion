using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Entities;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryStatsPage : UiElement // Stats
    {
        readonly PartyCharacterId _activeCharacter;

        IEnumerable<TextBlock> BuildHoverText(SystemTextId id, Func<ICharacterSheet, int> getValue, Func<ICharacterSheet, int> getMax)
        {
            var assets = Resolve<IAssetManager>();
            var state = Resolve<IStateManager>().State;
            var settings = Resolve<ISettings>();
            var formatter = new TextFormatter(assets, settings.Language);
            var member = state.GetPartyMember(_activeCharacter);

            var block = formatter.Format(assets.LoadString(id, settings.Language)).Item1.First();
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
                return new ProgressBar(source, () =>
                    {
                        var state = Resolve<IStateManager>().State;
                        var member = state.GetPartyMember(activeCharacter);
                        return getValue(member);
                    }, () =>
                    {
                        var state = Resolve<IStateManager>().State;
                        var member = state.GetPartyMember(activeCharacter);
                        return getMax(member);
                    }, 100);
            }

            var stack = new VerticalStack(
                new Header(S(SystemTextId.Inv2_Attributes)),
                new HorizontalStack(
                    new VerticalStack(
                        new Text(S(SystemTextId.Attrib_STR)).Right(),
                        new Padding(0,2),
                        new Text(S(SystemTextId.Attrib_INT)).Right(),
                        new Padding(0,2),
                        new Text(S(SystemTextId.Attrib_DEX)).Right(),
                        new Padding(0,2),
                        new Text(S(SystemTextId.Attrib_SPD)).Right(),
                        new Padding(0,2),
                        new Text(S(SystemTextId.Attrib_STA)).Right(),
                        new Padding(0,2),
                        new Text(S(SystemTextId.Attrib_LUC)).Right(),
                        new Padding(0,2),
                        new Text(S(SystemTextId.Attrib_MR)).Right(),
                        new Padding(0,2),
                        new Text(S(SystemTextId.Attrib_MT)).Right()
                    ),
                    new Padding(2,0),
                    new VerticalStack(
                        new Padding(105, 0),
                        Progress(SystemTextId.Attrib_Strength, x => x.Attributes.Strength, x => x.Attributes.StrengthMax),
                        new Padding(0,3),
                        Progress(SystemTextId.Attrib_Intelligence, x => x.Attributes.Intelligence, x => x.Attributes.IntelligenceMax),
                        new Padding(0,3),
                        Progress(SystemTextId.Attrib_Dexterity, x => x.Attributes.Dexterity, x => x.Attributes.DexterityMax),
                        new Padding(0,3),
                        Progress(SystemTextId.Attrib_Speed, x => x.Attributes.Speed, x => x.Attributes.SpeedMax),
                        new Padding(0,3),
                        Progress(SystemTextId.Attrib_Stamina, x => x.Attributes.Stamina, x => x.Attributes.StaminaMax),
                        new Padding(0,3),
                        Progress(SystemTextId.Attrib_Luck, x => x.Attributes.Luck, x => x.Attributes.LuckMax),
                        new Padding(0,3),
                        Progress(SystemTextId.Attrib_MagicResistance, x => x.Attributes.MagicResistance, x => x.Attributes.MagicResistanceMax),
                        new Padding(0,3),
                        Progress(SystemTextId.Attrib_MagicTalent, x => x.Attributes.MagicTalent, x => x.Attributes.MagicTalentMax)
                    )
                ),
                new Header(S(SystemTextId.Inv2_Skills)),
                new HorizontalStack(
                    new VerticalStack(
                        new Text(S(SystemTextId.Skill_CLO)).Right(),
                        new Padding(0,2),
                        new Text(S(SystemTextId.Skill_LON)).Right(),
                        new Padding(0,2),
                        new Text(S(SystemTextId.Skill_CRI)).Right(),
                        new Padding(0,2),
                        new Text(S(SystemTextId.Skill_LP)).Right()
                    ),
                    new Padding(2,0),
                    new VerticalStack(
                        new Padding(105, 0),
                        Progress(SystemTextId.Skill_CloseRangeCombat, x => x.Skills.CloseCombat, x => x.Skills.CloseCombatMax),
                        new Padding(0,3),
                        Progress(SystemTextId.Skill_LongRangeCombat, x => x.Skills.RangedCombat, x => x.Skills.RangedCombatMax),
                        new Padding(0,3),
                        Progress(SystemTextId.Skill_CriticalHit, x => x.Skills.CriticalChance, x => x.Skills.CriticalChanceMax),
                        new Padding(0,3),
                        Progress(SystemTextId.Skill_Lockpicking, x => x.Skills.LockPicking, x => x.Skills.LockPickingMax)
                    )
                )
            );

            Children.Add(new HorizontalStack(new Padding(4,0), stack, new Padding(4,0)));
        }
    }
}
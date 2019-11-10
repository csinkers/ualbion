using System.Collections.Generic;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventorySummaryPage : UiElement // Summary
    {
        readonly PartyCharacterId _activeMember;

        public InventorySummaryPage(PartyCharacterId activeMember)
        {
            _activeMember = activeMember;
            var background = new FixedPosition(
                new Rectangle(0, 26, 135, 144),
                new UiSprite<SmallPortraitId>((SmallPortraitId)(int)activeMember));

            var summarySource = new DynamicText(BuildSummary);
            var pointsSource = new DynamicText(BuildPoints);
            var pointsHeadingSource = new DynamicText(BuildPointsHeadings);

            var frameTheme = new FrameTheme();
            var stack = 
                new HorizontalStack(
                    new Padding(4,0),
                    new VerticalStack(
                        new Padding(0, 4),
                        new ButtonFrame(new FixedSize(125, 41, new Text(summarySource))) { Theme = frameTheme, State = ButtonState.Pressed},
                        new Padding(0, 80),
                        new ButtonFrame(
                            new FixedSize(125, 41, 
                                new HorizontalStack(
                                    new Text(pointsHeadingSource),
                                    new Padding(5,0),
                                    new FixedSize(41, 41,
                                        new Text(pointsSource)
                                    ))))
                        { Theme = frameTheme, State = ButtonState.Pressed }
                    ),
                    new Padding(4,0)
                );
            Children.Add(background);
            Children.Add(stack);
        }

        IEnumerable<TextBlock> BuildSummary()
        {
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();
            var state = Resolve<IStateManager>();
            var member = state.State.GetPartyMember(_activeMember);

            // {INVE}{NAME} ({SEXC}), %u years old, {RACE}, {CLAS}, level %d.
            var (formatBlocks, _) = new TextFormatter(assets, settings.Language)
                .Inventory(member)
                .Format(
                    assets.LoadString(SystemTextId.Inv1_NYearsOldRaceClassLevelN, settings.Language),
                    member.Age, member.Level);

            foreach (var block in formatBlocks)
                yield return block;
        }

        IEnumerable<TextBlock> BuildPointsHeadings()
        {
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();
            var state = Resolve<IStateManager>();
            var member = state.State.GetPartyMember(_activeMember);
            var formatter = new TextFormatter(assets, settings.Language);
            string S(SystemTextId id) => assets.LoadString(id, settings.Language);

            foreach (var block in formatter.Format(S(SystemTextId.Inv1_LifePoints)).Item1)
            {
                block.Arrangement = TextArrangement.NoWrap;
                block.Alignment = TextAlignment.Right;
                yield return block;
            }

            if (member.Magic.SpellPointsMax > 0)
            {
                foreach (var block in formatter.Format(S(SystemTextId.Inv1_SpellPoints)).Item1)
                {
                    block.Arrangement = TextArrangement.ForceNewLine | TextArrangement.NoWrap;
                    block.Alignment = TextAlignment.Right;
                    yield return block;
                }
            }
            else yield return new TextBlock("") { Arrangement = TextArrangement.ForceNewLine };

            foreach (var block in formatter.Format(S(SystemTextId.Inv1_ExperiencePoints)).Item1)
            {
                block.Arrangement = TextArrangement.ForceNewLine | TextArrangement.NoWrap;
                block.Alignment = TextAlignment.Right;
                yield return block;
            }

            foreach (var block in formatter.Format(S(SystemTextId.Inv1_TrainingPoints)).Item1)
            {
                block.Arrangement = TextArrangement.ForceNewLine | TextArrangement.NoWrap;
                block.Alignment = TextAlignment.Right;
                yield return block;
            }
        }

        IEnumerable<TextBlock> BuildPoints()
        {
            var state = Resolve<IStateManager>();
            var member = state.State.GetPartyMember(_activeMember);
            yield return new TextBlock($"{member.LifePoints}/{member.LifePointsMax}") { Arrangement = TextArrangement.NoWrap };

            yield return new TextBlock(
                member.Magic.SpellPointsMax > 0
                    ? $"{member.Magic.SpellPoints}/{member.Magic.SpellPointsMax}"
                    : "")
            {
                Arrangement = TextArrangement.ForceNewLine | TextArrangement.NoWrap
            };

            yield return new TextBlock($"{member.ExperiencePoints}") { Arrangement = TextArrangement.ForceNewLine | TextArrangement.NoWrap };
            yield return new TextBlock($"{member.TrainingPoints}") { Arrangement = TextArrangement.ForceNewLine | TextArrangement.NoWrap };
        }
    }
}
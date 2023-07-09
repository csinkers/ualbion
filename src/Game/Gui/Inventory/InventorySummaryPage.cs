using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory;

public class InventorySummaryPage : UiElement // Summary
{
    readonly PartyMemberId _activeMember;
    readonly UiSpriteElement _portrait;

    public InventorySummaryPage(PartyMemberId activeMember)
    {
        _activeMember = activeMember;

        var summarySource = new DynamicText(BuildSummary);
        var pointsSource = new DynamicText(BuildPoints);
        var pointsHeadingSource = new DynamicText(BuildPointsHeadings);

        var stack =
            new HorizontalStacker(
                new Spacing(4,0),
                new VerticalStacker(
                    new Spacing(0, 4),
                    new GroupingFrame(new FixedSize(125, 41, new UiText(summarySource))),
                    new Spacing(0, 80),
                    new GroupingFrame(
                        new FixedSize(125, 41,
                            new HorizontalStacker(
                                new UiText(pointsHeadingSource),
                                new Spacing(5,0),
                                new FixedSize(41, 41,
                                    new UiText(pointsSource)
                                ))))
                ),
                new Spacing(4,0)
            );

        _portrait = new UiSpriteElement(SpriteId.None)
        {
            Flags = SpriteFlags.GradientPixels
        };

        AttachChild(new LayerStacker(
            new FixedPosition( new Rectangle(0, 25, 135, 145), _portrait),
            stack));
    }

    protected override void Subscribed()
    {
        var assets = Resolve<IAssetManager>();
        var sheet = assets.LoadSheet(_activeMember.ToSheet());
        _portrait.Id = sheet.PortraitId;
        base.Subscribed();
    }

    IEnumerable<TextBlock> BuildSummary()
    {
        var assets = Resolve<IAssetManager>();
        var tf = Resolve<ITextFormatter>();
        var member = Resolve<IParty>()?[_activeMember];
        if (member == null)
            yield break;

        // {INVE}{NAME} ({SEXC}), %u years old, {RACE}, {CLAS}, level %d.
        var formatBlocks = tf
            .Format(
                assets.LoadStringSafe(Base.SystemText.Inv1_NYearsOldRaceClassLevelN),
                member.Apparent.Age, member.Apparent.Level).GetBlocks();

        foreach (var block in formatBlocks)
            yield return block;
    }

    IEnumerable<TextBlock> BuildPointsHeadings()
    {
        var tf = Resolve<ITextFormatter>();
        var member = Resolve<IParty>()?[_activeMember];

        if (member == null)
            yield break;

        foreach (var block in tf.Format(Base.SystemText.Inv1_LifePoints).GetBlocks())
        {
            block.ArrangementFlags = TextArrangementFlags.NoWrap;
            block.Alignment = TextAlignment.Right;
            yield return block;
        }

        if (member.Apparent.Magic.SpellPoints.Max > 0)
        {
            foreach (var block in tf.Format(Base.SystemText.Inv1_SpellPoints).GetBlocks())
            {
                block.ArrangementFlags = TextArrangementFlags.ForceNewLine | TextArrangementFlags.NoWrap;
                block.Alignment = TextAlignment.Right;
                yield return block;
            }
        }
        else yield return new TextBlock("") { ArrangementFlags = TextArrangementFlags.ForceNewLine };

        foreach (var block in tf.Format(Base.SystemText.Inv1_ExperiencePoints).GetBlocks())
        {
            block.ArrangementFlags = TextArrangementFlags.ForceNewLine | TextArrangementFlags.NoWrap;
            block.Alignment = TextAlignment.Right;
            yield return block;
        }

        foreach (var block in tf.Format(Base.SystemText.Inv1_TrainingPoints).GetBlocks())
        {
            block.ArrangementFlags = TextArrangementFlags.ForceNewLine | TextArrangementFlags.NoWrap;
            block.Alignment = TextAlignment.Right;
            yield return block;
        }
    }

    IEnumerable<TextBlock> BuildPoints()
    {
        var member = Resolve<IParty>()[_activeMember];
        if (member == null)
            yield break;

        var lp = member.Apparent.Combat.LifePoints;
        var sp = member.Apparent.Magic.SpellPoints;
        var flags = TextArrangementFlags.ForceNewLine | TextArrangementFlags.NoWrap;

        yield return new TextBlock($"{lp.Current}/{lp.Max}") { ArrangementFlags = TextArrangementFlags.NoWrap };
        yield return new TextBlock(sp.Max > 0 ? $"{sp.Current}/{sp.Max}" : "") { ArrangementFlags = flags };
        yield return new TextBlock($"{member.Apparent.Combat.ExperiencePoints}") { ArrangementFlags = flags };
        yield return new TextBlock($"{member.Apparent.Combat.TrainingPoints}") { ArrangementFlags = flags };
    }
}
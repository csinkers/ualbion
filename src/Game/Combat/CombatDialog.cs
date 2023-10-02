using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Combat;

public class CombatDialog : Dialog
{
    public record EndCombatEvent(CombatResult Result) : EventRecord;
    readonly IReadOnlyBattle _battle;

    public CombatDialog(int depth, IReadOnlyBattle battle) : base(DialogPositioning.Center, depth)
    {
        _battle = battle ?? throw new ArgumentNullException(nameof(battle));
        var stack = new List<IUiElement>();

        for (int row = 0; row < SavedGame.CombatRows; row++)
            stack.Add(BuildRow(row));

        stack.Add(new Spacing(0, 2));
        var startRoundButton = 
            new Button(Base.SystemText.Combat_StartRound)
            {
                DoubleFrame = true
            }.OnClick(() =>
            {
            });

        stack.Add(new NonGreedy(startRoundButton));
        stack.Add(new Spacing(0, 2));

        AttachChild(new DialogFrame(new VerticalStacker(stack))
        {
            Background = DialogFrameBackgroundStyle.MainMenuPattern
        });
    }

    HorizontalStacker BuildRow(int row)
    {
        var stack = new List<IUiElement>();

        for (int col = 0; col < SavedGame.CombatColumns; col++)
            stack.Add(new LogicalCombatTile(col + row * SavedGame.CombatColumns, _battle));

        return new HorizontalStacker(stack);
    }
}
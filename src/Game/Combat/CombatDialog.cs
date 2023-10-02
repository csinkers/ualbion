using System;
using System.Collections.Generic;
using UAlbion.Formats.Assets.Save;
using UAlbion.Game.Gui;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Combat;

public class CombatDialog : Dialog
{
    readonly IReadOnlyBattle _battle;

    public CombatDialog(int depth, IReadOnlyBattle battle) : base(DialogPositioning.Center, depth)
    {
        _battle = battle ?? throw new ArgumentNullException(nameof(battle));
        var stack = new List<IUiElement> { new Spacing(0, 3) };

        for (int row = 0; row < SavedGame.CombatRows; row++)
        {
            if (row > 0)
                stack.Add(new Spacing(0, 2));
            stack.Add(BuildRow(row));
        }

        stack.Add(new Spacing(0, 4));
        var startRoundButton = 
            new Button(Base.SystemText.Combat_StartRound)
            {
                DoubleFrame = true
            }.OnClick(() =>
            {
            });

        stack.Add(new FixedSize(52, 13, startRoundButton));
        stack.Add(new Spacing(0, 3));

        AttachChild(new DialogFrame(new Padding(new VerticalStacker(stack), 6))
        {
            Background = DialogFrameBackgroundStyle.MainMenuPattern
        });
    }

    HorizontalStacker BuildRow(int row)
    {
        var stack = new List<IUiElement> { new Spacing(15, 0) };
        for (int col = 0; col < SavedGame.CombatColumns; col++)
            stack.Add(new LogicalCombatTile(col + row * SavedGame.CombatColumns, _battle));

        stack.Add(new Spacing(15, 0));
        return new HorizontalStacker(stack);
    }
}
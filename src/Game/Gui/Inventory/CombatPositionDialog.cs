using System.Collections.Generic;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory;

public class CombatPositionDialog : ModalDialog
{
    /*
content starts @ 52,44 - size 256 x 103

Double thick button "OK" @ 154, 131 - 52x13. Centred white.

6x2 array of cells
single width border
first @ 67, 47 - 36x39
7th @ 105,88 (i.e. horz padding 2, vert 2)
profile pic highlights on hover with name appearing in status. Clears on blur
on click, depresses. On release changes to small arrow cursor with profile below right
clicking another prof will exchange

while holding a prof the status text when hovering on 
a cell is "Position %held" or "Swap %existing and %held"
clicking on non-cell locations does nothing
right clicking returns held to original loc or undoes last swap

    Vertical alignment:
        -------
        pad3
        /--\
        |  | 39
        \--/
        pad2
        /--\
        |  | 39
        \--/
        pad4
        ------
        | OK |
        ------
        pad3
        -------

    Horz:

    |       |---|      |--|             |
    | pad15 |   | pad2 |  | ... | pad15 |
    |       |---|      |--|             |

*/

    PartyMemberId _inHand;
    public PartyMemberId InHand
    {
        get => _inHand;
        set
        {
            _inHand = value;
            var sheet = Resolve<IParty>()[_inHand]?.Effective;
            Raise(new SetCursorEvent(_inHand.IsNone ? Base.CoreGfx.Cursor : Base.CoreGfx.CursorSmall));
            Raise(new SetHeldItemCursorEvent(sheet?.PortraitId ?? SpriteId.None, 0, 1, 1, false));
        }
    }

    public CombatPositionDialog(int depth) : base(DialogPositioning.Center, depth)
    {
        var stack = new List<IUiElement> { new Spacing(0, 3) };

        for (int row = 0; row < SavedGame.CombatRowsForParty; row++)
        {
            if (row > 0)
                stack.Add(new Spacing(0, 2));

            stack.Add(BuildRow(row));
        }

        stack.Add(new Spacing(0, 4));
        var okButton = 
            new Button(Base.SystemText.MsgBox_OK)
            {
                DoubleFrame = true
            }.OnClick(() =>
            {
                if (_inHand.IsNone)
                    Remove();
            });

        stack.Add(new FixedSize(52, 13, okButton));
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
            stack.Add(new LogicalCombatPositionSlot(col + row * SavedGame.CombatColumns, this));

        stack.Add(new Spacing(15, 0));
        return new HorizontalStacker(stack);
    }
}
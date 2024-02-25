using System;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory;

public class LogicalCombatPositionSlot : UiElement
{
    readonly VisualCombatPositionSlot _visual;
    readonly CombatPositionDialog _owner;
    readonly int _slotNumber;

    public LogicalCombatPositionSlot(int slotNumber, CombatPositionDialog owner)
    {
        _slotNumber = slotNumber;
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _visual = AttachChild(new VisualCombatPositionSlot(slotNumber, GetSheet))
            .OnClick(Clicked)
            .OnHover(Hover)
            .OnBlur(Blur);
    }

    void Clicked()
    {
        var party = Resolve<IParty>();

        switch (GetSlotAction(_slotNumber))
        {
            case InventoryAction.Pickup:
                _owner.InHand = GetPlayer(_slotNumber).Id;
                break;
            case InventoryAction.PutDown:
                Raise(new SetPlayerCombatSlotEvent(_owner.InHand, _slotNumber));
                _owner.InHand = PartyMemberId.None;
                break;
            case InventoryAction.Swap:
                var oldPosition = party[_owner.InHand].CombatPosition;
                var currentOccupant = GetPlayer(_slotNumber).Id;
                Raise(new SetPlayerCombatSlotEvent(_owner.InHand, _slotNumber));
                Raise(new SetPlayerCombatSlotEvent(currentOccupant, oldPosition));
                _owner.InHand = currentOccupant;
                break;
        }
    }

    public override string ToString() => $"CombatSlot:{_slotNumber}";
    IPlayer GetPlayer(int combatSlotNumber)
    {
        var player = Resolve<IGameState>().GetPlayerForCombatPosition(combatSlotNumber);
        return player?.Id == _owner.InHand ? null : player; // Hide the real position of players that are currently picked up
    }

    ICharacterSheet GetSheet(int combatSlotNumber) => GetPlayer(combatSlotNumber)?.Effective;

    void Blur()
    {
        Raise(new SetCursorEvent(_owner.InHand.IsNone ? Base.CoreGfx.Cursor : Base.CoreGfx.CursorSmall));
        Raise(new HoverTextEvent(null));
    }

    void Hover()
    {
        var tf = Resolve<ITextFormatter>();
        var party = Resolve<IParty>();

        var sheet = GetSheet(_slotNumber);
        var lang = ReadVar(V.User.Gameplay.Language);

        string inHandName = null;
        if (!_owner.InHand.IsNone)
            inHandName = party[_owner.InHand].Effective.GetName(lang);

        _visual.Hoverable = true;
        switch (GetSlotAction(_slotNumber))
        {
            case InventoryAction.Nothing:
                _visual.Hoverable = false;
                break;
            case InventoryAction.Pickup: // <Item name>
            {
                if (sheet != null)
                {
                    Raise(new HoverTextEvent(new LiteralText(sheet.GetName(lang))));
                    Raise(new SetCursorEvent(Base.CoreGfx.CursorSelected));
                }
                break;
            }
            case InventoryAction.PutDown: // Position %s
            {
                if (inHandName != null)
                {
                    var text = tf.Format(Base.SystemText.Inv3_PositionX, inHandName);
                    Raise(new HoverTextEvent(text));
                }
                break;
            }
            case InventoryAction.Swap: // Swap %s and %s
            {
                if (inHandName != null && sheet != null)
                {
                    var text = tf.Format(Base.SystemText.Inv3_SwapXAndX, inHandName, sheet.GetName(lang));
                    Raise(new HoverTextEvent(text));
                }
                break;
            }
        }
    }

    InventoryAction GetSlotAction(int slot) =>
        (!_owner.InHand.IsNone, GetSheet(slot) != null) switch
        {
            // (hand, dest)
            (false, false) => InventoryAction.Nothing,
            (false, true) => InventoryAction.Pickup,
            (true, false) => InventoryAction.PutDown,
            (true, true) => InventoryAction.Swap
        };
}
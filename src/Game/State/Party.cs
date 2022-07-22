using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Inventory;
using UAlbion.Game.Text;

namespace UAlbion.Game.State;

public class Party : ServiceComponent<IParty>, IParty
{
    readonly IDictionary<SheetId, CharacterSheet> _characterSheets;
    readonly List<Player.PartyMember> _statusBarOrder = new();
    readonly List<Player.PartyMember> _walkOrder = new();
    readonly IReadOnlyList<Player.PartyMember> _readOnlyStatusBarOrder;
    readonly IReadOnlyList<Player.PartyMember> _readOnlyWalkOrder;
    readonly byte[] _combatPositions;

    public Party(IDictionary<SheetId, CharacterSheet> characterSheets, Func<InventoryId, Inventory> getInventory, byte[] combatPositions)
    {
        _characterSheets = characterSheets ?? throw new ArgumentNullException(nameof(characterSheets));
        _combatPositions = combatPositions ?? throw new ArgumentNullException(nameof(combatPositions));
        _readOnlyStatusBarOrder = _statusBarOrder.AsReadOnly();
        _readOnlyWalkOrder = _walkOrder.AsReadOnly();

        On<AddPartyMemberEvent>(e => SetLastResult(AddMember(e.PartyMemberId)));
        On<RemovePartyMemberEvent>(e => SetLastResult(RemoveMember(e.PartyMemberId)));
        On<SetPartyLeaderEvent>(e => SetLeader(e.PartyMemberId));
        On<SetPlayerCombatSlotEvent>(OnSetCombatSlot);

        AttachChild(new PartyInventory(getInventory));
    }

    void OnSetCombatSlot(SetPlayerCombatSlotEvent e)
    {
        for (int i = 0; i < _statusBarOrder.Count; i++)
        {
            if (_statusBarOrder[i].Id != e.Id)
                continue;

            _combatPositions[i] = (byte)e.Slot;
            return;
        }
    }

    [SuppressMessage("Design", "CA1043:Use Integral Or String Argument For Indexers", Justification = "<Pending>")]
    public IPlayer this[PartyMemberId id]
    {
        get
        {
            foreach (var x in _statusBarOrder) // Don't use LINQ, we want to avoid allocations
                if (x.Id == id)
                    return x;
            return null;
        }
    }

    public IPlayer Leader => _walkOrder[0];
    public IReadOnlyList<IPlayer> StatusBarOrder => _readOnlyStatusBarOrder;
    public IReadOnlyList<IPlayer> WalkOrder => _readOnlyWalkOrder;
    public int TotalGold => _statusBarOrder.Sum(x => x.Effective.Inventory.Gold.Amount);
    public int GetItemCount(ItemId itemId) =>
        _statusBarOrder
            .SelectMany(x => x.Effective.Inventory.EnumerateAll())
            .Where(x => x.Item is ItemData item && item.Id == itemId)
            .Sum(x => x.Amount);

    // The current party leader (shown with a white outline on
    // health bar and slightly raised in the status bar)
    void SetLeader(PartyMemberId value)
    {
        int index = _walkOrder.FindIndex(x => x.Id == value);
        if (index == -1)
            return;

        var player = _walkOrder[index];
        _walkOrder.RemoveAt(index);
        _walkOrder.Insert(0, player);
        Raise(new SetContextEvent(ContextType.Leader, value));
    }

    public bool AddMember(PartyMemberId id)
    {
        if (_statusBarOrder.Any(x => x.Id == id))
            return false;

        var player = new Player.PartyMember(id, _characterSheets[id.ToSheet()]);
        if (_statusBarOrder.Count >= SavedGame.MaxPartySize) 
            return false;

        int index = _statusBarOrder.Count;
        _statusBarOrder.Add(player);

        // Allocate combat position
        if (_combatPositions[index] == 0)
        {
            for (int combatSlot = SavedGame.CombatSlotColumns * SavedGame.CombatSlotRows - 1; combatSlot >= 0; combatSlot--)
            {
                bool occupied = false;
                for (int i = 0; i < index && !occupied; i++)
                    if (_combatPositions[i] == combatSlot)
                        occupied = true;

                if (occupied)
                    continue;

                _combatPositions[index] = (byte)combatSlot;
                break;
            }
        }

        _walkOrder.Add(player);
        AttachChild(player);
        Raise(new PartyChangedEvent());
        return true;
    }

    bool RemoveMember(PartyMemberId id)
    {
        var player = _statusBarOrder.FirstOrDefault(x => x.Id == id);
        if (player == null)
            return false;

        _walkOrder.Remove(player);
        _statusBarOrder.Remove(player);
        player.Remove();
        Raise(new PartyChangedEvent());
        return true;
    }

    void SetLastResult(bool result) => Resolve<IEventManager>().LastEventResult = result;

    public void Clear()
    {
        foreach(var id in _statusBarOrder.Select(x => x.Id).ToList())
            RemoveMember(id);
    }
}

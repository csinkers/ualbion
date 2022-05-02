using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Events.Transitions;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.State;

public class PartyInventory : Component
{
    readonly Func<InventoryId, Inventory> _getInventory;

    public PartyInventory(Func<InventoryId, Inventory> getInventory)
    {
        _getInventory = getInventory;
        On<InventoryTakeAllEvent>(TakeAll);
        On<ModifyGoldEvent>(e => ChangePartyItemAmount(AssetId.Gold, e.Operation, e.Amount));
        On<ModifyRationsEvent>(e => ChangePartyItemAmount(AssetId.Rations, e.Operation, e.Amount));
        On<ModifyItemCountEvent>(e => GiveToParty(e.ItemId, e.Amount));
        On<SimpleChestEvent>(e =>
        {
            var itemId = e.ChestType switch
            {
                SimpleChestItemType.Item => (AssetId)e.ItemId,
                SimpleChestItemType.Gold => AssetId.Gold,
                SimpleChestItemType.Rations => AssetId.Rations,
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(nameof(e.ChestType), (int)e.ChestType, typeof(SimpleChestItemType))
            };

            var recipient = ChangePartyItemAmount(itemId, NumericOperation.AddAmount, e.Amount);
            if (recipient.HasValue)
                MapItemTransition(itemId, recipient.Value);
        });
    }

    void TakeAll(InventoryTakeAllEvent e)
    {
        var party = Resolve<IParty>();
        var inventoryManager = Resolve<IInventoryManager>();
        var chest = _getInventory(new InventoryId(e.ChestId));
        if (chest == null)
            return;

        var changedMembers = new HashSet<PartyMemberId>();
        foreach (var slot in chest.EnumerateAll())
        {
            if (slot.ItemId.IsNone) 
                continue;

            foreach (var member in party.WalkOrder)
            {
                ushort itemsGiven = inventoryManager.TryGiveItems(new InventoryId(member.Id), slot, null);
                if (itemsGiven > 0)
                    changedMembers.Add(member.Id);

                if (slot.ItemId.IsNone)
                    break;
            }
        }

        foreach(var memberId in changedMembers)
            Raise(new InventoryChangedEvent(new InventoryId(memberId)));
        Raise(new InventoryChangedEvent(new InventoryId(e.ChestId)));
    }

    void SetLastResult(bool result) => Resolve<IEventManager>().LastEventResult = result;

    int GetTotalItemCount(ItemId itemId)
    {
        var im = Resolve<IInventoryManager>();
        var party = Resolve<IParty>();
        return party.WalkOrder.Sum(x => im.GetItemCount(new InventoryId(x.Id), itemId));
    }

    IContents ContentsFromItemId(ItemId itemId) =>
        itemId.Type switch
        {
            AssetType.Gold => Gold.Instance,
            AssetType.Rations => Rations.Instance,
            _ => Resolve<IAssetManager>().LoadItem(itemId),
        };

    PartyMemberId? ChangePartyItemAmount(ItemId itemId, NumericOperation operation, ushort amount)
    {
        int currentTotal = GetTotalItemCount(itemId);
        int newTotal = operation.Apply(currentTotal, amount, 0, int.MaxValue);
        var delta = newTotal - currentTotal;

        if (delta > 0)
            return GiveToParty(itemId, (ushort)delta);
        if (delta < 0)
            return TakeFromParty(itemId, (ushort)-delta);
        return null;
    }

    PartyMemberId? GiveToParty(ItemId itemId, ushort amount)
    {
        PartyMemberId? recipient = null;
        var party = Resolve<IParty>();
        var inventoryManager = Resolve<IInventoryManager>();
        var slot = new ItemSlot(new InventorySlotId(new InventoryId(InventoryType.Temporary, 0), 0));
        var contents = ContentsFromItemId(itemId);
        slot.Set(contents, amount);

        foreach (var member in party.WalkOrder)
        {
            ushort amountGiven = inventoryManager.TryGiveItems(new InventoryId(member.Id), slot, slot.Amount);

            if (amountGiven > 0)
            {
                Raise(new InventoryChangedEvent((InventoryId)member.Id));
                recipient = member.Id;
            }

            if (slot.Amount == 0)
                break;
        }
        SetLastResult(slot.Amount == 0);
        return recipient;
    }

    PartyMemberId? TakeFromParty(ItemId itemId, ushort amount)
    {
        var party = Resolve<IParty>();
        var inventoryManager = Resolve<IInventoryManager>();
        var slot = new ItemSlot(new InventorySlotId(new InventoryId(InventoryType.Temporary, 0), 0));
        PartyMemberId? donor = null;

        foreach (var member in party.WalkOrder)
        {
            ushort amountTaken = inventoryManager.TryTakeItems(new InventoryId(member.Id), slot, itemId, amount);
            amount -= amountTaken;

            if (amountTaken > 0)
            {
                Raise(new InventoryChangedEvent((InventoryId)member.Id));
                donor = member.Id;
            }

            if (amount == 0)
                break;
        }
        SetLastResult(slot.Amount == 0);
        return donor;
    }

    void MapItemTransition(ItemId itemId, PartyMemberId recipientId)
    {
        var context = TryResolve<IEventManager>()?.Context;
        if (context?.Source.AssetId.Type != AssetType.Map)
            return;

        var map = TryResolve<IMapManager>()?.Current;
        var window = TryResolve<IWindowManager>();
        var party = TryResolve<IParty>();
        var camera = TryResolve<ICamera>();

        if (map == null || window == null || party == null || camera == null)
            return;

        var player = party[recipientId];
        var worldPosition = new Vector3(context.Source.X, context.Source.Y, 0) * map.TileSize;
        var normPosition = camera.ProjectWorldToNorm(worldPosition);
        var uiPosition = window.NormToUi(new Vector2(normPosition.X, normPosition.Y));

        Raise(new LinearItemTransitionEvent(itemId,
            (int)uiPosition.X,
            (int)uiPosition.Y,
            (int)player.StatusBarUiPosition.X,
            (int)player.StatusBarUiPosition.Y,
            null));
    }
}
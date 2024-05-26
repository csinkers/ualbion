using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Assets;

public class Inventory : IInventory
{
    readonly IReadOnlyList<IReadOnlyItemSlot> _readOnlyList;

    public Inventory(InventoryId id)
    {
        Id = id;
        Slots = new ItemSlot[(int)(id.Type switch
        {
            InventoryType.Player => ItemSlotId.FullSlotCount,
            InventoryType.Monster => ItemSlotId.FullSlotCount,
            InventoryType.CombatLoot => ItemSlotId.FullSlotCount,
            InventoryType.Chest => ItemSlotId.ChestSlotCount,
            _ => ItemSlotId.NormalSlotCount // just the backpack/normal slots, e.g. merchants
        })];

        for (int i = 0; i < Slots.Length; i++)
            Slots[i] = new ItemSlot(new InventorySlotId(id, (ItemSlotId)i));

        _readOnlyList = new ReadOnlyCollection<IReadOnlyItemSlot>(Slots);
    }

    ItemSlot TrySlot(ItemSlotId id) => Slots.Length > (int)id ? Slots[(int)id] : null;

    [JsonIgnore] public InventoryId Id { get; }
    [JsonInclude] public ItemSlot[] Slots { get; private set; }
    [JsonIgnore] public IEnumerable<ItemSlot> BackpackSlots { get { for (int i = 0; i < (int)ItemSlotId.NormalSlotCount; i++) yield return Slots[i]; } }
    [JsonIgnore] public ItemSlot Gold        => TrySlot(ItemSlotId.Gold);
    [JsonIgnore] public ItemSlot Rations     => TrySlot(ItemSlotId.Rations);
    [JsonIgnore] public ItemSlot Neck        => TrySlot(ItemSlotId.Neck);
    [JsonIgnore] public ItemSlot Head        => TrySlot(ItemSlotId.Head);
    [JsonIgnore] public ItemSlot Tail        => TrySlot(ItemSlotId.Tail);
    [JsonIgnore] public ItemSlot LeftHand    => TrySlot(ItemSlotId.LeftHand);
    [JsonIgnore] public ItemSlot Chest       => TrySlot(ItemSlotId.Chest);
    [JsonIgnore] public ItemSlot RightHand   => TrySlot(ItemSlotId.RightHand);
    [JsonIgnore] public ItemSlot LeftFinger  => TrySlot(ItemSlotId.LeftFinger);
    [JsonIgnore] public ItemSlot Feet        => TrySlot(ItemSlotId.Feet);
    [JsonIgnore] public ItemSlot RightFinger => TrySlot(ItemSlotId.RightFinger);
    public static Inventory SerdesChest(int n, Inventory inv, AssetMapping mapping, ISerializer s) => Serdes(n, inv, mapping, s, InventoryType.Chest);
    public static Inventory SerdesMerchant(int n, Inventory inv, AssetMapping mapping, ISerializer s) => Serdes(n, inv, mapping, s, InventoryType.Merchant);
    public static Inventory SerdesCharacter(int n, Inventory inv, AssetMapping mapping, ISerializer s) => Serdes(n, inv, mapping, s, InventoryType.Player);
    public static Inventory SerdesMonster(int n, Inventory inv, AssetMapping mapping, ISerializer s) => Serdes(n, inv, mapping, s, InventoryType.Monster);
    public IEnumerable<ItemSlot> EnumerateAll() => Slots.Where(x => x != null);

    static Inventory Serdes(int n, Inventory inv, AssetMapping mapping, ISerializer s, InventoryType type)
    {
        ArgumentNullException.ThrowIfNull(s);
        var invId = new InventoryId(type, (ushort)n);
        void S(string name, ItemSlot existing, ItemSlotId slotId)
            => s.Object(name, existing,
                (_, x, s2) => ItemSlot.Serdes(new InventorySlotId(invId, slotId), x, mapping, s2));

        inv ??= new Inventory(invId);
        if (s.BytesRemaining <= 0)
            return inv;

        if (s.IsCommenting())
            s.Begin(invId.ToString());

        if (type == InventoryType.Player)
        {
            S(nameof(inv.Neck), inv.Neck, ItemSlotId.Neck);
            S(nameof(inv.Head), inv.Head, ItemSlotId.Head);
            S(nameof(inv.Tail), inv.Tail, ItemSlotId.Tail);
            S(nameof(inv.RightHand), inv.RightHand, ItemSlotId.RightHand);
            S(nameof(inv.Chest), inv.Chest, ItemSlotId.Chest);
            S(nameof(inv.LeftHand), inv.LeftHand, ItemSlotId.LeftHand);
            S(nameof(inv.RightFinger), inv.RightFinger, ItemSlotId.RightFinger);
            S(nameof(inv.Feet), inv.Feet, ItemSlotId.Feet);
            S(nameof(inv.LeftFinger), inv.LeftFinger, ItemSlotId.LeftFinger);
        }

        for (int i = 0; i < (int)ItemSlotId.NormalSlotCount; i++)
            S($"Slot{i}", inv.Slots[i], (ItemSlotId)((int)ItemSlotId.Slot0 + i));

        // Note: Gold + Rations for players are added in the sheet loader. Merchants have no gold/rations.
        if (type == InventoryType.Chest)
        {
            if (s.IsReading())
            {
                inv.Gold.Item = AssetId.Gold;
                inv.Rations.Item = AssetId.Rations;
            }

            inv.Gold.Amount = s.UInt16(nameof(inv.Gold), inv.Gold.Amount);
            inv.Rations.Amount = s.UInt16(nameof(inv.Rations), inv.Rations.Amount);
        }

        if (s.IsCommenting())
            s.End();

        return inv;
    }

    public IEnumerable<ItemSlot> EnumerateBodyParts()
    {
        if (Neck != null) yield return Neck;
        if (Head != null) yield return Head;
        if (Tail != null) yield return Tail;
        if (LeftHand != null) yield return LeftHand;
        if (Chest != null) yield return Chest;
        if (RightHand != null) yield return RightHand;
        if (LeftFinger != null) yield return LeftFinger;
        if (Feet != null) yield return Feet;
        if (RightFinger != null) yield return RightFinger;
    }

    public ItemSlot GetSlot(ItemSlotId itemSlotId)
    {
        int slotNumber = (int)itemSlotId;
        return slotNumber >= Slots.Length ? null : Slots[slotNumber];
    }

    [JsonIgnore] public bool IsEmpty => !EnumerateAll().Any(x => !x.Item.IsNone && x.Amount > 0);

    public void SetSlotUiPosition(ItemSlotId itemSlotId, Vector2 position)
    {
        var slot = GetSlot(itemSlotId);
        if (slot != null)
            slot.LastUiPosition = position;
    }

    public Inventory DeepClone()
    {
        var clone = new Inventory(Id)
        {
            Gold = { Amount = Gold.Amount },
            Rations = { Amount = Rations.Amount }
        };

        for (int i = 0; i < Slots.Length; i++)
            clone.Slots[i] = Slots[i]?.DeepClone();

        return clone;
    }

    IReadOnlyItemSlot IInventory.Gold => Gold;
    IReadOnlyItemSlot IInventory.Rations => Rations;
    IReadOnlyItemSlot IInventory.Neck => Neck;
    IReadOnlyItemSlot IInventory.Head => Head;
    IReadOnlyItemSlot IInventory.Tail => Tail;
    IReadOnlyItemSlot IInventory.LeftHand => LeftHand;
    IReadOnlyItemSlot IInventory.Chest => Chest;
    IReadOnlyItemSlot IInventory.RightHand => RightHand;
    IReadOnlyItemSlot IInventory.LeftFinger => LeftFinger;
    IReadOnlyItemSlot IInventory.Feet => Feet;
    IReadOnlyItemSlot IInventory.RightFinger => RightFinger;
    IReadOnlyList<IReadOnlyItemSlot> IInventory.Slots => _readOnlyList;
    IEnumerable<IReadOnlyItemSlot> IInventory.EnumerateAll() => EnumerateAll();
    IEnumerable<IReadOnlyItemSlot> IInventory.EnumerateBodyParts() => EnumerateBodyParts();
    IReadOnlyItemSlot IInventory.GetSlot(ItemSlotId itemSlotId) => GetSlot(itemSlotId);
}
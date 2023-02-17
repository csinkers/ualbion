using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using UAlbion.Config;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Editor;

public class ItemSlotEditor : AssetEditor
{
    static readonly string[] _itemNames;
    static readonly IDictionary<ItemId, int> _itemNameIndexLookup;
    readonly ItemSlot _slot;

    // TODO: Nice visual editor with icons etc

    static ItemSlotEditor()
    {
        var itemIdInfo =
            AssetMapping.Global.EnumerateAssetsOfType(AssetType.Item)
                .Select(x => ((ItemId)x, x.ToString()))
                .OrderBy(x => x.Item2)
                .Select((x,i) => (x.Item1, i, x.Item2))
                .ToArray();

        _itemNames = new[] { "Empty" }.Concat(itemIdInfo.Select(x => x.Item3)).ToArray();
        _itemNameIndexLookup = itemIdInfo.ToDictionary(x => x.Item1, x => x.i + 1);
    }

    static int IndexForItemId(ItemId? x) => x == null ? 0 : _itemNameIndexLookup[x.Value];
    static ItemId ItemIdForIndex(int x) => x == 0 ? ItemId.None : Enum.Parse<Base.Item>(_itemNames[x]); // TODO: Proper item id support

    public ItemSlotEditor(ItemSlot slot) : base(slot)
    {
        _slot = slot ?? throw new ArgumentNullException(nameof(slot));
    }

    public override void Render()
    {
        int index = IndexForItemId(_slot.Item);
        int oldIndex = index;
        ImGui.Combo(_slot.Id.Slot.ToString(), ref index, _itemNames, _itemNames.Length);
        if (index != oldIndex)
        {
            var assetId = Resolve<IEditorAssetManager>().GetIdForAsset(Asset);
            var itemId = ItemIdForIndex(index);
            Raise(new EditorSetPropertyEvent(assetId, nameof(_slot.Item), _slot.Item, itemId));
        }

        if (_slot.Item.IsNone) 
            return;

        if (_slot.Item.Type == AssetType.Item)
        {
            var item = Resolve<IAssetManager>().LoadItem(_slot.Item);
            if (item.IsStackable)
            {
                ImGui.SameLine();
                UInt16Slider(nameof(_slot.Amount), _slot.Amount, 1, ItemSlot.MaxItemCount);
            }

            if (item.MaxCharges > 0)
            {
                ImGui.SameLine();
                UInt8Slider(nameof(_slot.Charges), _slot.Charges, 0, item.MaxCharges);
            }

            if (item.MaxEnchantmentCount > 0)
            {
                ImGui.SameLine();
                UInt8Slider(nameof(_slot.Enchantment), _slot.Enchantment, 0, item.MaxEnchantmentCount);
            }
            // TODO Flags: Broken, Cursed, Extra Info, unk, show as icons?
            // TODO: Clear button?
        }
        else
        {
            ImGui.SameLine();
            UInt16Slider(nameof(_slot.Amount), _slot.Amount, 1, ItemSlot.MaxItemCount);
        }
    }
}
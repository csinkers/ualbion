using System;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets;

public class InventoryPostProcessor : Component, IAssetPostProcessor
{
    static void ResolveItemProxies(AssetId id, Inventory inventory, IAssetManager assets)
    {
        if (inventory == null)
            return;

        // The first merchant has strange corrupt data, just zero it out
        if (id == AssetId.From(Base.Merchant.Unknown1))
        {
            foreach (var slot in inventory.Slots)
                slot.Clear();
            return;
        }

        foreach (var slot in inventory.EnumerateAll())
            if (slot.Item is ItemProxy proxy)
                slot.Item = assets.LoadItem(proxy.Id) ?? throw new InvalidOperationException($"Could not resolve item proxy for {slot.ItemId}");
    }

    public object Process(object asset, AssetInfo info)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        var assets = Resolve<IAssetManager>();
        switch (asset)
        {
            case CharacterSheet sheet: ResolveItemProxies(info.AssetId, sheet.Inventory, assets); break;
            case Inventory x: ResolveItemProxies(info.AssetId, x, assets); break;
            case SavedGame save:
                foreach (var sheet in save.Sheets.Values)
                    ResolveItemProxies(info.AssetId, sheet.Inventory, assets);
                foreach (var inv in save.Inventories.Values)
                    ResolveItemProxies(info.AssetId, inv, assets);

                break;
            default: throw new InvalidOperationException($"Unexpected asset type in inventory post processor: {asset}");
        }

        return asset;
    }
}
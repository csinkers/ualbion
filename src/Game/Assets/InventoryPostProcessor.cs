using System;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets
{
    public class InventoryPostProcessor : Component, IAssetPostProcessor
    {
        static void ResolveItemProxies(Inventory inventory, IAssetManager assets)
        {
            if (inventory == null)
                return;

            // The first merchant has strange corrupt data, just zero it out
            if (inventory.Id == new InventoryId((MerchantId)Base.Merchant.Unknown1))
            {
                foreach (var slot in inventory.Slots)
                    slot.Clear();
                return;
            }

            foreach (var slot in inventory.EnumerateAll())
                if (slot.Item is ItemProxy proxy)
                    slot.Item = assets.LoadItem(proxy.Id) ?? throw new InvalidOperationException($"Could not resolve item proxy for {slot.ItemId}");
        }

        public object Process(object asset, AssetInfo info, ICoreFactory factory)
        {
            var assets = Resolve<IAssetManager>();
            switch (asset)
            {
                case CharacterSheet sheet: ResolveItemProxies(sheet.Inventory, assets); break;
                case Inventory x: ResolveItemProxies(x, assets); break;
                case SavedGame save:
                    foreach (var sheet in save.Sheets.Values)
                        ResolveItemProxies(sheet.Inventory, assets);
                    foreach (var inv in save.Inventories.Values)
                        ResolveItemProxies(inv, assets);

                    break;
                default: throw new InvalidOperationException($"Unexpected asset type in inventory post processor: {asset}");
            }

            return asset;
        }
    }
}

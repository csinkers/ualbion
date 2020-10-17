using System;
using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets
{
    public class InventoryPostProcessor : IAssetPostProcessor
    {
        public IEnumerable<Type> SupportedTypes => new[] { typeof(CharacterSheet), typeof(Inventory), typeof(SavedGame) };
        void ResolveItemProxies(Inventory inventory, SerializationContext context, Func<AssetId, SerializationContext, object> loaderFunc)
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
                    slot.Item = (ItemData)loaderFunc(proxy.Id, context);
        }
        public object Process(ICoreFactory factory, AssetId key, object asset, SerializationContext context, Func<AssetId, SerializationContext, object> loaderFunc)
        {
            switch (asset)
            {
                case CharacterSheet sheet: ResolveItemProxies(sheet.Inventory, context, loaderFunc); break;
                case Inventory x: ResolveItemProxies(x, context, loaderFunc); break;
                case SavedGame save:
                    foreach(var sheet in save.Sheets.Values)
                        ResolveItemProxies(sheet.Inventory, context, loaderFunc);
                    foreach (var inv in save.Inventories.Values)
                        ResolveItemProxies(inv, context, loaderFunc);

                    break;
                default: throw new InvalidOperationException($"Unexpected asset type in inventory post processor: {asset}");
            }

            return asset;
        }
    }
}

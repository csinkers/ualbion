using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Save;

namespace UAlbion.Game.Assets
{
    public class InventoryPostProcessor : IAssetPostProcessor
    {
        public IEnumerable<Type> SupportedTypes => new[] { typeof(CharacterSheet), typeof(Inventory), typeof(SavedGame) };
        void ResolveItemProxies(Inventory inventory, Func<AssetKey, object> loaderFunc)
        {
            if (inventory == null)
                return;

            // Merchant 0 has strange corrupt data, just zero it out
            if (inventory.Id == new InventoryId(MerchantId.Unknown0))
            {
                foreach (var slot in inventory.Slots)
                    slot.Clear();
                return;
            }

            foreach (var slot in inventory.EnumerateAll())
                if (slot.Item is ItemProxy proxy)
                    slot.Item = ((IList<ItemData>)loaderFunc((AssetId)proxy.Id))[(ushort)proxy.Id];
        }
        public object Process(ICoreFactory factory, AssetKey key, object asset, Func<AssetKey, object> loaderFunc)
        {
            switch (asset)
            {
                case CharacterSheet sheet: ResolveItemProxies(sheet.Inventory, loaderFunc); break;
                case Inventory x: ResolveItemProxies(x, loaderFunc); break;
                case SavedGame save:
                    foreach(var sheet in save.PartyMembers.Values)
                        ResolveItemProxies(sheet.Inventory, loaderFunc);
                    foreach (var inv in save.Chests.Values)
                        ResolveItemProxies(inv, loaderFunc);
                    foreach (var inv in save.Merchants.Values)
                        ResolveItemProxies(inv, loaderFunc);

                    break;
                default: throw new InvalidOperationException($"Unexpected asset type in inventory post processor: {asset}");
            }

            return asset;
        }
    }
}
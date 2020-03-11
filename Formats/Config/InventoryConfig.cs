using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Config
{
    public class InventoryConfig
    {
        public class PlayerInventory : Dictionary<ItemSlotId, Position2D> { }
        public IDictionary<PartyCharacterId, PlayerInventory> Positions { get; } = new Dictionary<PartyCharacterId, PlayerInventory>();
        public static InventoryConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", "inventory.json");
            InventoryConfig config;
            if (File.Exists(configPath))
            {
                var configText = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<InventoryConfig>(configText);
            }
            else
            {
                config = new InventoryConfig();
            }
            return config;
        }
    }
}

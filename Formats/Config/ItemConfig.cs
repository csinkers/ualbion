using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public class ItemConfig
    {
        public enum ItemType
        {
            Misc = 0,
            Ammo,
            Amour,
            Armour,
            Key,
            MagicItem,
            Melee,
            PlotItem,
            Potion,
            Ranged,
            Spell,
            Tool
        }

        public class Item
        {
            public string Name;
            public ItemType Type;
        }

        public IDictionary<int, Item> Items { get; } = new Dictionary<int, Item>();

        public static ItemConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", "items.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException();

            var configText = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<ItemConfig>(configText);
        }
    }
}

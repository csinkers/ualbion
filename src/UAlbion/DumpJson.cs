using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game;
using Newtonsoft.Json;

namespace UAlbion
{
    static class DumpJson
    {
        static IEnumerable<T> All<T>() where T : struct, Enum => Enum.GetValues(typeof(T)).OfType<T>();
        static IDictionary<TKey, TValue> AllAssets<TKey, TValue>(Func<TKey, TValue> fetcher) where TKey : struct, Enum
            => All<TKey>()
               .Select(x => (x, fetcher(x)))
               .Where(x => x.Item2 != null)
               .ToDictionary(x => x.x, x => x.Item2);

        public static void DumpAll(string baseDir, IAssetManager assets)
        {
            var disposeList = new List<IDisposable>();

            TextWriter Writer(string name)
            {
                var directory = Path.Combine(baseDir, "data", "exported");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var stream = File.Open(Path.Combine(directory, name), FileMode.Create);
                var writer = new StreamWriter(stream);
                disposeList.Add(writer);
                disposeList.Add(stream);
                return writer;
            }

            void Flush()
            {
                foreach (var d in disposeList)
                    d.Dispose();
                disposeList.Clear();
            }

            var settings = new JsonSerializerSettings
            {
                // ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                // PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            };

            var s = JsonSerializer.Create(settings);
            TextWriter tw;

            foreach (var id in All<TilesetId>())
            {
                TilesetData asset = assets.LoadTileData(id);
                if (asset == null) continue;
                tw = Writer($"tileset{(int)id}.json");
                s.Serialize(tw, asset);
            }
            Flush();

            foreach (var id in All<LabyrinthDataId>())
            {
                LabyrinthData asset = assets.LoadLabyrinthData(id);
                if (asset == null) continue;
                tw = Writer($"labyrinth{(int)id}.json");
                s.Serialize(tw, asset);
            }
            Flush();

            // string str = assets.LoadString(StringId id, GameLanguage language);

            foreach (var id in All<MapDataId>())
            {
                IMapData asset = assets.LoadMap(id);
                if (asset == null) continue;
                tw = Writer($"map{(int)id}_{id}.json");
                s.Serialize(tw, asset);
            }
            Flush();

            tw = Writer("items.json");
            s.Serialize(tw, AllAssets<ItemId, ItemData>(assets.LoadItem));
            Flush();

            tw = Writer("partymembers.json");
            s.Serialize(tw, AllAssets<PartyCharacterId, CharacterSheet>(assets.LoadPartyMember));
            Flush();

            tw = Writer("npcs.json");
            s.Serialize(tw, AllAssets<NpcCharacterId, CharacterSheet>(assets.LoadNpc));
            Flush();

            tw = Writer("monsters.json");
            s.Serialize(tw, AllAssets<MonsterCharacterId, CharacterSheet>(assets.LoadMonster));
            Flush();

            tw = Writer("chests.json");
            s.Serialize(tw, AllAssets<ChestId, Inventory>(assets.LoadChest));
            Flush();

            tw = Writer("merchants.json");
            s.Serialize(tw, AllAssets<MerchantId, Inventory>(assets.LoadMerchant));
            Flush();

            foreach (var id in All<BlockListId>())
            {
                IList<Block> asset = assets.LoadBlockList(id);
                if (asset == null) continue;
                tw = Writer($"blocklist{(int)id}.json");
                s.Serialize(tw, asset);
            }
            Flush();

            tw = Writer("eventsets.json");
            s.Serialize(tw, AllAssets<EventSetId, EventSet>(assets.LoadEventSet));
            Flush();

            /*
                        foreach (var id in All<ScriptId>())
                        {
                            s = Writer($"script{(int)id}.json");
                            IList<IEvent> asset = assets.LoadScript(id);
                        }
            */

            tw = Writer("spells.json");
            s.Serialize(tw, AllAssets<SpellId, SpellData>(assets.LoadSpell));
            Flush();

            tw = Writer("monstergroups.json");
            s.Serialize(tw, AllAssets<MonsterGroupId, MonsterGroup>(assets.LoadMonsterGroup));
            Flush();
        }

#if false
        public static void DumpAll(string baseDir, IAssetManager assets)
        {
            ISerializer s;
            var disposeList = new List<IDisposable>();
            JsonWriter Writer(string name)
            {
                var directory = Path.Combine(baseDir, "data", "exported");
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var stream = File.OpenWrite(Path.Combine(directory, name));
                var tw = new StreamWriter(stream);
                disposeList.Add(tw);
                disposeList.Add(stream);
                return new JsonWriter(tw);
            }

            foreach (var id in All<TilesetId>())
            {
                TilesetData asset = assets.LoadTileData(id);
                if (asset == null) continue;
                s = Writer($"tileset{(int)id}.json");
                s.Object(null, asset, (i, x, s2) => TilesetData.Serdes(x, s2, null));
            }

            foreach (var id in All<LabyrinthDataId>())
            {
                LabyrinthData asset = assets.LoadLabyrinthData(id);
                if (asset == null) continue;
                s = Writer($"labyrinth{(int)id}.json");
                s.Comment(id.ToString());
                s.Object($"{(int)id}", asset, LabyrinthData.Serdes);
            }

            // string str = assets.LoadString(StringId id, GameLanguage language);

            foreach (var id in All<MapDataId>())
            {
                IMapData asset = assets.LoadMap(id);
                if (asset == null) continue;
                s = Writer($"map{(int)id}.json");
                s.Object($"{(int)id}", asset, BaseMapData.Serdes);
            }

            s = Writer("items.json");
            foreach (var id in All<ItemId>())
            {
                ItemData asset = assets.LoadItem(id);
                s.Object($"{(int)id}", asset, ItemData.Serdes);
            }

            foreach (var id in All<PartyCharacterId>())
            {
                CharacterSheet asset = assets.LoadPartyMember(id);
                if (asset == null) continue;
                s = Writer($"partymember{(int)id}.json");
                s.Object($"{(int) id}", asset,
                    (i, x, s2) => CharacterSheet.Serdes(id.ToAssetId(), x, s2));
            }

            foreach (var id in All<NpcCharacterId>())
            {
                CharacterSheet asset = assets.LoadNpc(id);
                if (asset == null) continue;
                s = Writer($"npc{(int)id}.json");
                s.Object($"{(int) id}", asset,
                    (i, x, s2) => CharacterSheet.Serdes(id.ToAssetId(), x, s2));
            }

            foreach (var id in All<MonsterCharacterId>())
            {
                CharacterSheet asset = assets.LoadMonster(id);
                if (asset == null) continue;
                s = Writer($"monster{(int)id}.json");
                s.Object($"{(int)id}", asset,
                    (i, x, s2) => CharacterSheet.Serdes(id.ToAssetId(), x, s2));
            }

            s = Writer("chests.json");
            foreach (var id in All<ChestId>())
            {
                Inventory asset = assets.LoadChest(id);
                s.Object($"{(int) id}", asset, Inventory.SerdesChest);
            }

            s = Writer("merchants.json");
            foreach (var id in All<MerchantId>())
            {
                var asset = assets.LoadMerchant(id);
                s.Object($"{(int) id}", asset, Inventory.SerdesMerchant);
            }

            foreach (var id in All<BlockListId>())
            {
                IList<Block> asset = assets.LoadBlockList(id);
                if (asset == null) continue;
                s = Writer($"blocklist{(int)id}.json");
                s.Object($"{(int)id}", asset, Block.Serdes);
            }

            s = Writer("eventsets.json");
            foreach (var id in All<EventSetId>())
            {
                EventSet asset = assets.LoadEventSet(id);
                s.Object($"{(int)id}", asset, EventSet.Serdes);
            }
/*
            foreach (var id in All<ScriptId>())
            {
                s = Writer($"script{(int)id}.json");
                IList<IEvent> asset = assets.LoadScript(id);
            }
*/

            s = Writer("spells.json");
            foreach (var id in All<SpellId>())
            {
                SpellData asset = assets.LoadSpell(id);
                s.Object($"{(int)id}", asset, SpellData.Serdes);
            }

            s = Writer("monstergroups.json");
            foreach (var id in All<MonsterGroupId>())
            {
                MonsterGroup asset = assets.LoadMonsterGroup(id);
                s.Object($"{(int)id}", asset, MonsterGroup.Serdes);
            }

            foreach (var d in disposeList)
                d.Dispose();
        }
#endif
    }
}

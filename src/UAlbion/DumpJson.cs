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
using UAlbion.Api;

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

        public static void Dump(string baseDir, IAssetManager assets, ISet<AssetType> types)
        {
            var disposeList = new List<IDisposable>();

            TextWriter Writer(string name)
            {
                var filename = Path.Combine(baseDir, "data", "exported", "json", name);
                var directory = Path.GetDirectoryName(filename);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var stream = File.Open(filename, FileMode.Create);
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
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            };

            var s = JsonSerializer.Create(settings);
            TextWriter tw;
            if (types.Contains(AssetType.Tileset))
            {
                foreach (var id in All<TilesetId>())
                {
                    TilesetData asset = assets.LoadTileData(id);
                    if (asset == null) continue;
                    tw = Writer($"tilesets/tileset{(int)id}.json");
                    s.Serialize(tw, asset);
                }

                Flush();
            }

            if (types.Contains(AssetType.LabData))
            {
                foreach (var id in All<LabyrinthDataId>())
                {
                    LabyrinthData asset = assets.LoadLabyrinthData(id);
                    if (asset == null) continue;
                    tw = Writer($"labdata/labyrinth{(int)id}.json");
                    s.Serialize(tw, asset);
                }

                Flush();
            }

            // string str = assets.LoadString(StringId id, GameLanguage language);

            if (types.Contains(AssetType.MapData))
            {
                foreach (var id in All<MapDataId>())
                {
                    IMapData asset = assets.LoadMap(id);
                    if (asset == null) continue;
                    tw = Writer($"maps/map{(int)id}_{id}.json");
                    s.Serialize(tw, asset);
                }

                Flush();
            }

            if (types.Contains(AssetType.ItemList) || types.Contains(AssetType.ItemNames))
            {
                tw = Writer("items.json");
                s.Serialize(tw, AllAssets<ItemId, ItemData>(assets.LoadItem));
                Flush();
            }

            if (types.Contains(AssetType.PartyMember))
            {
                tw = Writer("party_members.json");
                s.Serialize(tw, AllAssets<PartyCharacterId, CharacterSheet>(assets.LoadPartyMember));
                Flush();

            }

            if (types.Contains(AssetType.Npc))
            {
                tw = Writer("npcs.json");
                s.Serialize(tw, AllAssets<NpcCharacterId, CharacterSheet>(assets.LoadNpc));
                Flush();
            }

            if (types.Contains(AssetType.Monster))
            {

                tw = Writer("monsters.json");
                s.Serialize(tw, AllAssets<MonsterCharacterId, CharacterSheet>(assets.LoadMonster));
                Flush();
            }

            if (types.Contains(AssetType.ChestData))
            {

                tw = Writer("chests.json");
                s.Serialize(tw, AllAssets<ChestId, Inventory>(assets.LoadChest));
                Flush();
            }

            if (types.Contains(AssetType.MerchantData))
            {

                tw = Writer("merchants.json");
                s.Serialize(tw, AllAssets<MerchantId, Inventory>(assets.LoadMerchant));
                Flush();
            }

            if (types.Contains(AssetType.BlockList))
            {
                foreach (var id in All<BlockListId>())
                {
                    IList<Block> asset = assets.LoadBlockList(id);
                    if (asset == null) continue;
                    tw = Writer($"blocks/blocklist{(int)id}.json");
                    s.Serialize(tw, asset);
                }
                Flush();
            }

            if (types.Contains(AssetType.EventSet))
            {
                tw = Writer("event_sets.json");
                s.Serialize(tw, AllAssets<EventSetId, EventSet>(assets.LoadEventSet));
                Flush();
            }

            if (types.Contains(AssetType.Script))
            {
                foreach (var id in All<ScriptId>())
                {
                    IList<IEvent> asset = assets.LoadScript(id);
                    if (asset == null) continue;
                    tw = Writer($"scripts/script{(int)id}.json");
                    s.Serialize(tw, asset.Select(x => x.ToString()).ToArray());
                }
                Flush();
            }

            if (types.Contains(AssetType.SpellData))
            {
                tw = Writer("spells.json");
                s.Serialize(tw, AllAssets<SpellId, SpellData>(assets.LoadSpell));
                Flush();
            }

            if (types.Contains(AssetType.MonsterGroup))
            {
                tw = Writer("monster_groups.json");
                s.Serialize(tw, AllAssets<MonsterGroupId, MonsterGroup>(assets.LoadMonsterGroup));
                Flush();
            }

            if (types.Contains(AssetType.Palette))
            {
                foreach (var id in All<PaletteId>())
                {
                    tw = Writer($"palettes/palette{(int)id}_{id}.json");
                    var palette = assets.LoadPalette(id);
                    s.Serialize(tw, palette);
                }
                Flush();
            }
        }
    }
}

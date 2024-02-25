using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game;

namespace UAlbion;

class DumpJson : GameComponent, IAssetDumper
{
    public void Dump(string baseDir, ISet<AssetType> types, AssetId[] dumpIds)
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

        var assets = Assets;
        var jsonUtil = Resolve<IJsonUtil>();
        TextWriter tw;
        if (types.Contains(AssetType.Tileset))
        {
            foreach (var id in DumpUtil.All(AssetType.Tileset, dumpIds))
            {
                TilesetData asset = assets.LoadTileData(id);
                if (asset == null) continue;
                tw = Writer($"tilesets/tileset{id.Id}.json");
                tw.WriteLine(jsonUtil.Serialize(asset));
            }

            Flush();
        }

        if (types.Contains(AssetType.Labyrinth))
        {
            foreach (var id in DumpUtil.All(AssetType.Labyrinth, dumpIds))
            {
                LabyrinthData asset = assets.LoadLabyrinthData(id);
                if (asset == null) continue;
                tw = Writer($"labdata/labyrinth{id.Id}.json");
                tw.WriteLine(jsonUtil.Serialize(asset));
            }

            Flush();
        }

        // string str = assets.LoadString(StringId id, GameLanguage language);

        if (types.Contains(AssetType.Map))
        {
            foreach (var id in DumpUtil.All(AssetType.Map, dumpIds))
            {
                IMapData asset = assets.LoadMap(id);
                if (asset == null) continue;
                tw = Writer($"maps/map{id.Id}_{id}.json");
                tw.WriteLine(jsonUtil.Serialize(asset));
            }

            Flush();
        }

        if (types.Contains(AssetType.Item))
        {
            tw = Writer("items.json");
            tw.WriteLine(jsonUtil.Serialize(DumpUtil.AllAssets(AssetType.Item, dumpIds, x => assets.LoadItem(x))));
            Flush();
        }

        if (types.Contains(AssetType.PartySheet))
        {
            tw = Writer("party_members.json");
            tw.WriteLine(jsonUtil.Serialize(DumpUtil.AllAssets(AssetType.PartySheet, dumpIds, x => assets.LoadSheet(x))));
            Flush();

        }

        if (types.Contains(AssetType.NpcSheet))
        {
            tw = Writer("npcs.json");
            tw.WriteLine(jsonUtil.Serialize(DumpUtil.AllAssets(AssetType.NpcSheet, dumpIds, x => assets.LoadSheet(x))));
            Flush();
        }

        if (types.Contains(AssetType.MonsterSheet))
        {

            tw = Writer("monsters.json");
            tw.WriteLine(jsonUtil.Serialize(DumpUtil.AllAssets(AssetType.MonsterSheet, dumpIds, x => assets.LoadSheet(x))));
            Flush();
        }

        if (types.Contains(AssetType.Chest))
        {

            tw = Writer("chests.json");
            tw.WriteLine(jsonUtil.Serialize(DumpUtil.AllAssets(AssetType.Chest, dumpIds, assets.LoadInventory)));
            Flush();
        }

        if (types.Contains(AssetType.Merchant))
        {

            tw = Writer("merchants.json");
            tw.WriteLine(jsonUtil.Serialize(DumpUtil.AllAssets(AssetType.Merchant, dumpIds, assets.LoadInventory)));
            Flush();
        }

        if (types.Contains(AssetType.BlockList))
        {
            foreach (var id in DumpUtil.All(AssetType.BlockList, dumpIds))
            {
                IList<Block> asset = assets.LoadBlockList(id);
                if (asset == null) continue;
                tw = Writer($"blocks/blocklist{id.Id}.json");
                tw.WriteLine(jsonUtil.Serialize(asset));
            }
            Flush();
        }

        if (types.Contains(AssetType.EventSet))
        {
            tw = Writer("event_sets.json");
            tw.WriteLine(jsonUtil.Serialize(DumpUtil.AllAssets(AssetType.EventSet, dumpIds, x => assets.LoadEventSet(x))));
            Flush();
        }

        if (types.Contains(AssetType.Script))
        {
            foreach (var id in DumpUtil.All(AssetType.Script, dumpIds))
            {
                IList<IEvent> asset = assets.LoadScript(id);
                if (asset == null) continue;
                tw = Writer($"scripts/script{id.Id}.json");
                tw.WriteLine(jsonUtil.Serialize(asset.Select(x => x.ToString()).ToArray()));
            }
            Flush();
        }

        if (types.Contains(AssetType.Spell))
        {
            tw = Writer("spells.json");
            tw.WriteLine(jsonUtil.Serialize(DumpUtil.AllAssets(AssetType.Spell, dumpIds, x => assets.LoadSpell(x))));
            Flush();
        }

        if (types.Contains(AssetType.MonsterGroup))
        {
            tw = Writer("monster_groups.json");
            tw.WriteLine(jsonUtil.Serialize(DumpUtil.AllAssets(AssetType.MonsterGroup, dumpIds, x => assets.LoadMonsterGroup(x))));
            Flush();
        }

        if (types.Contains(AssetType.Palette))
        {
            foreach (var id in DumpUtil.All(AssetType.Palette, dumpIds))
            {
                tw = Writer($"palettes/palette{id.Id}_{id}.json");
                var palette = assets.LoadPalette(id);
                tw.WriteLine(jsonUtil.Serialize(palette));
            }
            Flush();
        }
    }
}

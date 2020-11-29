using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Game;
using UAlbion.Game.Text;

namespace UAlbion
{
    static class DumpText
    {
        const string ChestPath            = "ChestInfo.txt";
        const string EventSetPath         = "EventSets.txt";
        const string ItemDataPath         = "ItemInfo.txt";
        const string LabData              = "Labyrinths.txt";
        const string MapEventPath         = "MapEvents.txt";
        const string MapInfoPath          = "MapInfo.txt";
        const string MerchantPath         = "MerchantInfo.txt";
        const string MonsterCharacterPath = "MonsterCharacters.txt";
        const string MonsterGroupPath     = "MonsterGroups.txt";
        const string NpcCharacterPath     = "NpcCharacters.txt";
        const string PartyCharacterPath   = "PartyCharacters.txt";
        const string SpellInfoPath        = "Spells.txt";
        const string ScriptPath           = "Scripts/{0}.txt";

        static IEnumerable<AssetId> Ids<T>() where T : unmanaged, Enum => Enum.GetValues(typeof(T)).Cast<T>().Select(AssetId.From);
        public static void Dump(IAssetManager assets, string baseDir, ITextFormatter tf, ISet<AssetType> types)
        {
            foreach (var type in types)
            {
                switch (type)
                {
                    case AssetType.Chest: Chests(assets, baseDir); break;
                    case AssetType.EventSet: EventSets(assets, baseDir); break;
                    case AssetType.Item: ItemData(assets, baseDir); break;
                    case AssetType.Labyrinth: Labyrinths(assets, baseDir); break;
                    case AssetType.Map: MapData(assets, tf, baseDir); MapEvents(assets, baseDir); break;
                    case AssetType.Merchant: Merchants(assets, baseDir); break;
                    case AssetType.Monster: MonsterCharacterSheets(assets, tf, baseDir); break;
                    case AssetType.MonsterGroup: MonsterGroups(assets, baseDir); break;
                    case AssetType.Npc: NpcCharacterSheets(assets, tf, baseDir); break;
                    case AssetType.PartyMember: PartyCharacterSheets(assets, tf, baseDir); break;
                    case AssetType.Script: Scripts(assets, baseDir); break;
                    case AssetType.Spell: Spells(assets, baseDir); break;
                }
            }
        }

        static StreamWriter Open(string baseDir, string name)
        {
            var filename = Path.Combine(baseDir, "data", "exported", "text", name);
            var directory = Path.GetDirectoryName(filename);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return File.CreateText(filename);
        }

        static void Scripts(IAssetManager assets, string baseDir)
        {
            foreach (var id in Ids<Base.Script>())
            {
                var events = assets.LoadScript(id);
                if (events == null) continue;
                using var sw = Open(baseDir, string.Format(CultureInfo.InvariantCulture, ScriptPath, id));
                foreach (var e in events)
                    sw.WriteLine(e.ToString());
            }
        }

        static void Labyrinths(IAssetManager assets, string baseDir)
        {
            using var sw = Open(baseDir, LabData);
            // Dump map and lab data
            for (int i = 100; i < 400; i++)
            {
                if (!(assets.LoadMap((Base.Map)i) is MapData3D map))
                    continue;

                sw.WriteLine($"{i} {(Base.Map)i} {map.Width}x{map.Height} L{map.LabDataId.Id} P{map.PaletteId.Id}:{map.PaletteId}");
                var floors = map.Floors.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Key);
                sw.WriteLine("    Floors: " + string.Join(" ", floors.Select(x => $"{x.Key}:{x.Item2}")));
                var ceilings = map.Ceilings.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Key);
                sw.WriteLine("    Ceilings: " + string.Join(" ", ceilings.Select(x => $"{x.Key}:{x.Item2}")));
                var contents = map.Contents.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Key);
                sw.WriteLine("    Contents: " + string.Join(" ", contents.Select(x => $"{x.Key}:{x.Item2}")));

                var l = assets.LoadLabyrinthData(map.LabDataId);
                if (l == null) 
                    continue;

                foreach (var (content, _) in contents)
                {
                    if (content == 0 || content >= l.ObjectGroups.Count)
                        continue;

                    var objectInfo = l.ObjectGroups[content - 1];

                    sw.Write($"        {content}: ");
                    foreach (var subObject in objectInfo.SubObjects)
                    {
                        var definition = l.Objects[subObject.ObjectInfoNumber];
                        sw.Write(definition.SpriteId);
                        sw.Write(" ");
                    }

                    sw.WriteLine();
                }
            }

            var labIds =
                    Enumerable.Range(0, 9)
                        .Concat(Enumerable.Range(101, 25))
                        .Concat(Enumerable.Range(200, 11))
                ;
            foreach(var i in labIds)
            {
                var l = assets.LoadLabyrinthData((Base.LabyrinthData)i);
                if (l == null)
                    continue;

                sw.WriteLine($"L{i}");
                for (int j = 0; j < l.FloorAndCeilings.Count; j++)
                {
                    var fc = l.FloorAndCeilings[j];
                    sw.WriteLine($"    F/C {j}: {fc.SpriteId} {fc.AnimationCount}");
                }

                for (int j = 0; j < l.Walls.Count; j++)
                {
                    var w = l.Walls[j];
                    sw.WriteLine($"    W {j}: {w.SpriteId} {w.AnimationFrames} P{w.TransparentColour}");
                }

                for (int j = 0; j < l.ObjectGroups.Count; j++)
                {
                    var o = l.ObjectGroups[j];
                    sw.WriteLine(
                        $"    Obj {j}: {o.AutoGraphicsId} [{string.Join(", ", o.SubObjects.Select(x => x.ObjectInfoNumber.ToString(CultureInfo.InvariantCulture)))}]");
                }

                for (int j = 0; j < l.Objects.Count; j++)
                {
                    var o = l.Objects[j];
                    sw.WriteLine(
                        $"    Extra {j}: {o.SpriteId} {o.AnimationFrames} {o.Width}x{o.Height} M:{o.MapWidth}x{o.MapHeight}");
                }
            }
        }

        static void MapData(IAssetManager assets, ITextFormatter tf, string baseDir)
        {
            using var sw = Open(baseDir, MapInfoPath);
            for (int i = 100; i < 400; i++)
            {
                IMapData map = assets.LoadMap((Base.Map)i);
                if (map == null)
                    continue;

                sw.Write($"{i} {(Base.Map)i} {map.MapType} ");
                sw.Write($"{map.Width}x{map.Height} ");
                sw.Write($"Palette:{map.PaletteId} ({map.PaletteId.Id}) ");
                if (map is MapData2D map2d)
                {
                    sw.Write($"FrameRate:{map2d.FrameRate} ");
                    sw.Write($"TileSet:{map2d.TilesetId} ");
                    sw.Write($"Flags:{map2d.Flags} ");
                    sw.Write($"Sound?:{map2d.Sound} ");
                }

                LabyrinthData lab = null;
                if (map is MapData3D map3d)
                {
                    sw.Write($"Flags: {map3d.Flags} ");
                    sw.Write($"Labyrinth: {map3d.LabDataId} ");
                    sw.Write($"Sound?:{map3d.AmbientSongId} ");
                    lab = assets.LoadLabyrinthData(map3d.LabDataId);
                }

                sw.Write($"Song:{map.SongId} ({map.SongId.Id}) ");
                sw.WriteLine($"CombatBackground:{map.CombatBackgroundId} ({map.CombatBackgroundId.Id})");

                for(int j = 0; j < map.Npcs.Count; j++)
                {
                    if (!map.Npcs.TryGetValue(j, out var npc))
                        continue;

                    var wp = npc.Waypoints.FirstOrDefault();
                    var idText = npc.Id.ToString().PadLeft(15);

                    sw.Write($"    Npc{j:D3}: {idText} ({npc.Id.Id:D3}) ");
                    sw.Write($"X:{wp.X:D3} Y:{wp.Y:D3} ");
                    sw.Write($"U8:{npc.Unk8} ");
                    sw.Write($"U9:{npc.Unk9} ");
                    sw.Write($"F{(int)npc.Flags:X2}:({npc.Flags}) ");
                    sw.Write($"M:{npc.Movement} ");
                    sw.Write($"S:{npc.Sound} ({(int?)npc.Sound}) ");
                    switch (map.MapType)
                    {
                        case MapType.TwoD: sw.Write($"GR:{npc.SpriteOrGroup} "); break;
                        case MapType.TwoDOutdoors: sw.Write($"KL:{npc.SpriteOrGroup} "); break;
                        case MapType.ThreeD:
                            if (lab != null)
                            {
                                if (npc.SpriteOrGroup.Id >= lab.ObjectGroups.Count)
                                {
                                    sw.Write($"InvalidObjGroup:{npc.SpriteOrGroup.Id}");
                                }
                                else
                                {
                                    var objectData = lab.ObjectGroups[npc.SpriteOrGroup.Id];
                                    sw.Write($"3D:{npc.SpriteOrGroup.Id}=(");
                                    bool first = true;
                                    foreach (var subObject in objectData.SubObjects)
                                    {
                                        if (!first) sw.Write(", ");
                                        first = false;
                                        var def = lab.Objects[subObject.ObjectInfoNumber];
                                        sw.Write(def.SpriteId);
                                    }

                                    sw.Write(")");
                                }
                            }

                            break;  
                    }

                    sw.WriteLine();

                    if (npc.Chain != null)
                    {
                        // var context = new EventContext(new EventSource.Map((MapDataId)i, TriggerType.Default, 0, 0));
                        sw.WriteLine($"        EventChain: {npc.Chain?.Id}");
                        foreach (var e in npc.Chain.Events)
                        {
                            if (e.Event is TextEvent textEvent)
                            {
                                var textSource = tf.Format(textEvent.ToId());
                                var text = string.Join(", ", textSource.GetBlocks().Select(x => x.Text));
                                sw.WriteLine($"        {e} = {text}");
                            }
                            else
                            {
                                sw.Write("        ");
                                sw.WriteLine(e.ToString());
                            }
                        }
                    }
                }
            }
        }

        static void PartyCharacterSheets(IAssetManager assets, ITextFormatter tf, string baseDir)
        {
            using var sw = Open(baseDir, PartyCharacterPath);
            foreach (var charId in Ids<Base.PartyMember>())
                DumpCharacterSheet(charId, assets.LoadSheet(charId), sw, assets, tf);
        }

        static void NpcCharacterSheets(IAssetManager assets, ITextFormatter tf, string baseDir)
        {
            using var sw = Open(baseDir, NpcCharacterPath);
            foreach (var charId in Ids<Base.Npc>())
                DumpCharacterSheet(charId, assets.LoadSheet(charId), sw, assets, tf);
        }

        static void MonsterCharacterSheets(IAssetManager assets, ITextFormatter tf, string baseDir)
        {
            using var sw = Open(baseDir, MonsterCharacterPath);
            foreach (var charId in Ids<Base.Monster>())
                DumpCharacterSheet(charId, assets.LoadSheet(charId), sw, assets, tf);
        }

        static void DumpCharacterSheet(CharacterId id, CharacterSheet c, StreamWriter sw, IAssetManager assets, ITextFormatter tf)
        {
            if (c == null || string.IsNullOrEmpty(c.GermanName) && c.PortraitId.IsNone)
                return;

            sw.WriteLine($"{id.Id:D3} {id} ({c.EnglishName}, {c.GermanName}, {c.FrenchName})");
            sw.WriteLine($"    Type:{c.Type} Gender:{c.Gender} Races:{c.Race} Class:{c.PlayerClass} Age:{c.Age} Level:{c.Level}");
            sw.WriteLine($"    Languages:{c.Languages} Sprite:{c.SpriteId} Portrait:{c.PortraitId}");
            if (c.Inventory?.Slots != null)
            {
                sw.WriteLine($"    Inventory: (Gold:{c.Inventory.Gold.Amount / 10.0}, Rations:{c.Inventory.Rations.Amount})");
                sw.WriteLine($"             Head: {c.Inventory.Head}");
                sw.WriteLine($"             Neck: {c.Inventory.Neck}");
                sw.WriteLine($"            Chest: {c.Inventory.Chest}");
                sw.WriteLine($"            LHand: {c.Inventory.LeftHand}");
                sw.WriteLine($"            RHand: {c.Inventory.RightHand}");
                sw.WriteLine($"             Tail: {c.Inventory.Tail}");
                sw.WriteLine($"          LFinger: {c.Inventory.LeftFinger}");
                sw.WriteLine($"          RFinger: {c.Inventory.RightFinger}");
                sw.WriteLine($"             Feet: {c.Inventory.Feet}");
                sw.WriteLine("             Pack:");
                foreach (var item in c.Inventory.Slots.Where(x => x?.Item != null))
                    sw.WriteLine($"                 {item}");
            }

            sw.WriteLine($"    {c.Attributes}");
            sw.WriteLine($"    {c.Skills}");
            sw.WriteLine($"    {c.Combat}");
            if (c.Magic.SpellStrengths.Any())
            {
                sw.WriteLine($"    Magic: (SP:{c.Magic.SpellPoints}/{c.Magic.SpellPointsMax}) Classes: {c.Magic.SpellClasses}");
                foreach(var spell in c.Magic.SpellStrengths)
                    sw.WriteLine($"        {spell.Key} {spell.Value.Item2} ({(spell.Value.Item1 ? "Learnt": "Unknown")})");
            }

            sw.WriteLine($"    WordSetId:{c.WordSetId}");

            var eventSet = assets.LoadEventSet(c.EventSetId);
            sw.WriteLine($"    Event Set {c.EventSetId}: {(eventSet == null ? "Not Found" : $"{eventSet.Chains.Count} chains")}");
            if (eventSet != null)
            {
                sw.WriteLine("    Chain Offsets: " +
                             string.Join(", ", eventSet.Chains.Select((x, i) => $"{i}:{x.Id}")));
                foreach (var e in eventSet.Events)
                {
                    if (e.Event is TextEvent textEvent)
                    {
                        var textSource = tf.Format(textEvent.ToId());
                        var text = string.Join(", ", textSource.GetBlocks().Select(x => x.Text));
                        sw.WriteLine($"        {e} = {text}");
                    }
                    else
                        sw.WriteLine("        " + e);
                }
            }

            if (c.Unknown6 != 0) sw.WriteLine($"    Unknown06:{c.Unknown6}");
            if (c.Unknown7 != 0) sw.WriteLine($"    Unknown07:{c.Unknown7}");
            if (c.Unknown11 != 0) sw.WriteLine($"    Unknown11:{c.Unknown11}");
            if (c.Unknown12 != 0) sw.WriteLine($"    Unknown12:{c.Unknown12}");
            if (c.Unknown13 != 0) sw.WriteLine($"    Unknown13:{c.Unknown13}");
            if (c.Unknown14 != 0) sw.WriteLine($"    Unknown14:{c.Unknown14}");
            if (c.Unknown15 != 0) sw.WriteLine($"    Unknown15:{c.Unknown15}");
            if (c.Unknown16 != 0) sw.WriteLine($"    Unknown16:{c.Unknown16}");
            if (c.Unknown1C != 0) sw.WriteLine($"    Unknown1C:{c.Unknown1C}");
            if (c.Unknown20 != 0) sw.WriteLine($"    Unknown20:{c.Unknown20}");
            if (c.Unknown22 != 0) sw.WriteLine($"    Unknown22:{c.Unknown22}");
            if (c.Unknown24 != 0) sw.WriteLine($"    Unknown24:{c.Unknown24}");
            if (c.Unknown26 != 0) sw.WriteLine($"    Unknown26:{c.Unknown26}");
            if (c.Unknown28 != 0) sw.WriteLine($"    Unknown28:{c.Unknown28}");
            if (c.Unknown2E != 0) sw.WriteLine($"    Unknown2E:{c.Unknown2E}");
            if (c.Unknown30 != 0) sw.WriteLine($"    Unknown30:{c.Unknown30}");
            if (c.Unknown36 != 0) sw.WriteLine($"    Unknown36:{c.Unknown36}");
            if (c.Unknown38 != 0) sw.WriteLine($"    Unknown38:{c.Unknown38}");
            if (c.Unknown3E != 0) sw.WriteLine($"    Unknown3E:{c.Unknown3E}");
            if (c.Unknown40 != 0) sw.WriteLine($"    Unknown40:{c.Unknown40}");
            if (c.Unknown46 != 0) sw.WriteLine($"    Unknown46:{c.Unknown46}");
            if (c.Unknown48 != 0) sw.WriteLine($"    Unknown48:{c.Unknown48}");
            if (c.Unknown4E != 0) sw.WriteLine($"    Unknown4E:{c.Unknown4E}");
            if (c.Unknown50 != 0) sw.WriteLine($"    Unknown50:{c.Unknown50}");
            if (c.Unknown56 != 0) sw.WriteLine($"    Unknown56:{c.Unknown56}");
            if (c.Unknown58 != 0) sw.WriteLine($"    Unknown58:{c.Unknown58}");
            if (c.Unknown5E != 0) sw.WriteLine($"    Unknown5E:{c.Unknown5E}");
            if (c.Unknown60 != 0) sw.WriteLine($"    Unknown60:{c.Unknown60}");
            if (c.Unknown66 != 0) sw.WriteLine($"    Unknown66:{c.Unknown66}");
            if (c.Unknown68 != 0) sw.WriteLine($"    Unknown68:{c.Unknown68}");
            if (c.Unknown6C != 0) sw.WriteLine($"    Unknown6C:{c.Unknown6C}");
            if (c.Unknown7E != 0) sw.WriteLine($"    Unknown7E:{c.Unknown7E}");
            if (c.Unknown80 != 0) sw.WriteLine($"    Unknown80:{c.Unknown80}");
            if (c.Unknown86 != 0) sw.WriteLine($"    Unknown86:{c.Unknown86}");
            if (c.Unknown88 != 0) sw.WriteLine($"    Unknown88:{c.Unknown88}");
            if (c.Unknown8E != 0) sw.WriteLine($"    Unknown8E:{c.Unknown8E}");
            if (c.Unknown90 != 0) sw.WriteLine($"    Unknown90:{c.Unknown90}");
            if (c.UnusedBlock.Any(x => x != 0))
            {
                for (int i = 0; i < c.UnusedBlock.Length; i++)
                    sw.WriteLine($" UnusedBlock.{i}:{c.UnusedBlock[i]}");
            }

            if (c.UnknownCE != 0) sw.WriteLine($"    UnknownCE:{c.UnknownCE}");
            if (c.UnknownD6 != 0) sw.WriteLine($"    UnknownD6:{c.UnknownD6}");
            if (c.UnknownDA != 0) sw.WriteLine($"    UnknownDA:{c.UnknownDA}");
            if (c.UnknownDC != 0) sw.WriteLine($"    UnknownDC:{c.UnknownDC}");
            if (c.UnknownDE != 0) sw.WriteLine($"    UnknownDE:{c.UnknownDE}");
            if (c.UnknownE2 != 0) sw.WriteLine($"    UnknownE2:{c.UnknownE2}");
            if (c.UnknownE4 != 0) sw.WriteLine($"    UnknownE4:{c.UnknownE4}");
            if (c.UnknownE6 != 0) sw.WriteLine($"    UnknownE6:{c.UnknownE6}");
            if (c.UnknownEA != 0) sw.WriteLine($"    UnknownEA:{c.UnknownEA}");
            if (c.UnknownFA != 0) sw.WriteLine($"    UnknownFA:{c.UnknownFA}");
            if (c.UnknownFC != 0) sw.WriteLine($"    UnknownFC:{c.UnknownFC}");
        }

        static void Chests(IAssetManager assets, string baseDir)
        {
            using var sw = Open(baseDir, ChestPath);
            var chests = 
                Ids<Base.Chest>()
                    .ToDictionary(x => x, assets.LoadInventory);
            foreach (var chest in chests.Where(x => x.Value != null))
            {
                sw.WriteLine($"Chest {chest.Key.Id} {chest.Key}: ({chest.Value.Gold.Amount / 10.0} gold, {chest.Value.Rations} rations)");
                foreach (var x in chest.Value.Slots.Where(x => x.Item != null))
                    sw.WriteLine($"    {x.Amount}x{x.Item} Charges:{x.Charges} Enchantment:{x.Enchantment} Flags:{x.Flags}");
            }
        }

        static void Merchants(IAssetManager assets, string baseDir)
        {
            using var sw = Open(baseDir, MerchantPath);
            var merchants = 
                Ids<Base.Merchant>()
                    .ToDictionary(x => x, assets.LoadInventory);
            foreach (var merchant in merchants.Where(x => x.Value != null))
            {
                sw.WriteLine($"Merchant {merchant.Key.Id} {merchant.Key}");
                foreach(var x in merchant.Value.Slots.Where(x => x.Item != null))
                    sw.WriteLine($"    {x.Amount}x{x.Item} Charges:{x.Charges} Enchantment:{x.Enchantment} Flags:{x.Flags}");
            }
        }

        static void ItemData(IAssetManager assets, string baseDir)
        {
            using var sw = Open(baseDir, ItemDataPath);
            var items = new List<ItemData>();
            foreach (ItemId itemId in Ids<Base.Item>())
            {
                sw.Write(itemId.Id.ToString(CultureInfo.InvariantCulture).PadLeft(3)); 
                sw.Write(' ');
                sw.Write(itemId.ToString().PadRight(20));

                var data = assets.LoadItem(itemId);
                if (data == null)
                    sw.Write("null");
                else
                {
                    items.Add(data);
                    sw.Write($"Gfx:{data.Icon} ({data.IconAnim} frames) ");
                    sw.Write($"Type:{data.TypeId} Slot:{data.SlotType} ");
                    sw.Write($"F:{data.Flags} A:{data.Activate} ");
                    sw.Write($"{data.Class} ");
                    sw.Write($"${data.Value / 10}.{data.Value % 10:D2} ");
                    sw.Write($"{data.Weight}g ($/kg ratio: {(float)data.Value*100/data.Weight:F2})");
                }

                sw.WriteLine();
            }

            sw.WriteLine();
            sw.WriteLine("Details:");

            ItemType lastType = ItemType.Armor;
            foreach (var data in items.OrderBy(x => x.TypeId).ThenBy(x => x.Id))
            {
                if(lastType != data.TypeId)
                    sw.WriteLine();

                sw.WriteLine(data.ToString());
                lastType = data.TypeId;
            }
        }

        static void PrintChain(StreamWriter sw, EventFormatter formatter, IEventNode e, int indent)
        {
            do
            {
                sw.Write($"{e.Id:000}");
                sw.Write("".PadRight(indent * 4));
                if(e is IBranchNode branch)
                {
                    sw.WriteLine($"if (!{formatter.GetText(e)}) {{");
                    if (branch.NextIfFalse != null)
                        PrintChain(sw, formatter, branch.NextIfFalse, indent + 1);
                    sw.WriteLine("}".PadLeft(4 + indent * 4));
                    sw.WriteLine("else...".PadLeft(10 + indent * 4));
                }
                else sw.WriteLine(formatter.GetText(e));
                e = e.Next;
            } while (e != null);
        }

        static void PrintEvent(StreamWriter sw, EventFormatter formatter, IEventNode e, int? chainId)
        {
            if(chainId.HasValue)
            {
                sw.Write('C');
                sw.Write(chainId.Value.ToString(CultureInfo.InvariantCulture).PadRight(3));
            }
            else
                sw.Write("    ");

            sw.WriteLine(formatter.GetText(e));
        }

        static void DumpMapEvents(StreamWriter sw, IAssetManager assets, MapId mapId, IMapData map)
        {
            var formatter = new EventFormatter(assets, mapId.ToMapText());
            sw.WriteLine();
            sw.WriteLine($"Map {mapId.Id} {mapId}:");
            foreach (var e in map.Events)
            {
                var chainId = map.Chains.Select((x, i) => x.FirstEvent == e ? i : (int?) null).FirstOrDefault(x => x != null);
                PrintEvent(sw, formatter, e, chainId);
            }

            /*
            var rootNodes = new HashSet<(bool, TriggerType, int)>();
            foreach (var zone in map.Zones)
                rootNodes.Add((zone.Global, zone.Trigger, zone.EventNumber));

            var sorted =
                    rootNodes
                        .OrderByDescending(x => x.Item1)
                        .ThenBy(x => x.Item2)
                        .ThenBy(x => x.Item3)
                ;

            foreach (var (global, trigger, number) in sorted)
            {
                var e = map.Events[number];
                sw.WriteLine($"{(global ? "Global" : "Local")} {trigger}:");
                PrintChain(sw, formatter, e, 1);
            }*/
        }

        static void MapEvents(IAssetManager assets, string baseDir)
        {
            using var sw = Open(baseDir, MapEventPath);
            foreach (var mapId in Ids<Base.Map>())
            {
                IMapData map = assets.LoadMap(mapId);
                if (map != null)
                    DumpMapEvents(sw, assets, mapId, map);
            }
        }

        static void EventSets(IAssetManager assets, string baseDir)
        {
            using var sw = Open(baseDir, EventSetPath);
            foreach (var eventSetId in Ids<Base.EventSet>())
            {
                sw.WriteLine();
                sw.WriteLine($"{eventSetId.Id} {eventSetId}:");
                var set = assets.LoadEventSet(eventSetId);
                if (set == null)
                    continue;

                var formatter = new EventFormatter(assets, ((EventSetId)eventSetId).ToEventText());
                foreach (var e in set.Events)
                {
                    var chainId = set.Chains.Select((x, i) => x.FirstEvent == e ? i : (int?) null).FirstOrDefault(x => x != null);
                    PrintEvent(sw, formatter, e, chainId);
                }
            }
        }

        static void Spells(IAssetManager assets, string baseDir)
        {
            using var sw = Open(baseDir, SpellInfoPath);
            foreach (var spellId in Ids<Base.Spell>())
            {
                var spell = assets.LoadSpell(spellId);
                var name = assets.LoadString(spell.Name);
                //int classNumber = (int)spellId / SpellData.MaxSpellsPerClass;
                //int offsetInClass = (int)spellId % SpellData.MaxSpellsPerClass;
                sw.Write($"Spell{spellId.Id:D3} {spell.Class}_{spell.OffsetInClass} ");
                sw.Write($"({spellId}) ".PadRight(24));
                sw.Write($"\"{name}\" ".PadRight(24));
                sw.Write($"{spell.Cost} ".PadLeft(4));
                sw.Write($"Lvl:{spell.LevelRequirement} ");
                sw.Write($"Env:{spell.Environments} ");
                sw.Write($"Target:{spell.Targets} ");
                sw.WriteLine();
            }
        }

        static void MonsterGroups(IAssetManager assets, string baseDir)
        {
            using var sw = Open(baseDir, MonsterGroupPath);
            foreach (var groupId in Ids<Base.MonsterGroup>())
            {
                var group = assets.LoadMonsterGroup(groupId);
                if (group == null)
                    continue;

                var counts =
                    group.Grid
                        .Where(x => !x.IsNone)
                        .GroupBy(x => x)
                        .OrderBy(x => x.Key)
                        .ToDictionary(x => x.Key, x => x.Count());
                sw.Write($"{groupId.Id}: ");
                var countString = string.Join(" ", counts.Select(x => $"{x.Value}x{x.Key}"));
                sw.WriteLine(countString);
            }
        }
    }
}

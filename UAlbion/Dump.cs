﻿using SerdesNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Core.Veldrid;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Map;
using UAlbion.Formats.MapEvents;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Text;

namespace UAlbion
{
    static class Dump
    {
        public static void CoreSprites(IAssetManager assets, string baseDir)
        {
            var dir = $@"{baseDir}\data\exported\MAIN.EXE";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Dump all core sprites
            var factory = new VeldridCoreFactory();
            var palette = assets.LoadPalette(PaletteId.Inventory);
            for (int i = 0; i < 86; i++)
            {
                var name = $"{i}_{(CoreSpriteId)i}";
                var coreSprite = assets.LoadTexture((CoreSpriteId)i);
                var multiTexture = factory.CreateMultiTexture(name, new DummyPaletteManager(palette));
                multiTexture.AddTexture(1, coreSprite, 0, 0, null, false);
                multiTexture.SavePng(1, 0, $@"{dir}\{name}.bmp");
            }
        }

        public static void ThreeDMapAndLabInfo(IAssetManager assets, string baseDir)
        {
            using var sw = File.CreateText($@"{baseDir}\re\3DInfo.txt");
            // Dump map and lab data
            for (int i = 100; i < 400; i++)
            {
                if (!(assets.LoadMap((MapDataId) i) is MapData3D map))
                    continue;

                sw.WriteLine(
                    $"{i} {(MapDataId) i} {map.Width}x{map.Height} L{(int?) map.LabDataId} P{(int) map.PaletteId}:{map.PaletteId}");
                var floors = map.Floors.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                sw.WriteLine("    Floors: " + string.Join(" ", floors.Select(x => $"{x.Item1}:{x.Item2}")));
                var ceilings = map.Ceilings.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                sw.WriteLine("    Ceilings: " + string.Join(" ", ceilings.Select(x => $"{x.Item1}:{x.Item2}")));
                var contents = map.Contents.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                sw.WriteLine("    Contents: " + string.Join(" ", contents.Select(x => $"{x.Item1}:{x.Item2}")));

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
                        sw.Write(definition.TextureNumber);
                        sw.Write(" ");
                    }

                    sw.WriteLine();
                }
            }

            for (int i = 0; i < 300; i++)
            {
                var l = assets.LoadLabyrinthData((LabyrinthDataId) i);
                if (l == null)
                    continue;

                sw.WriteLine($"L{i}");
                for (int j = 0; j < l.FloorAndCeilings.Count; j++)
                {
                    var fc = l.FloorAndCeilings[j];
                    sw.WriteLine($"    F/C {j}: {fc.TextureNumber} {fc.AnimationCount}");
                }

                for (int j = 0; j < l.Walls.Count; j++)
                {
                    var w = l.Walls[j];
                    sw.WriteLine($"    W {j}: {w.TextureNumber} {w.AnimationFrames} P{w.TransparentColour}");
                }

                for (int j = 0; j < l.ObjectGroups.Count; j++)
                {
                    var o = l.ObjectGroups[j];
                    sw.WriteLine(
                        $"    Obj {j}: {o.AutoGraphicsId} [{string.Join(", ", o.SubObjects.Select(x => x.ObjectInfoNumber.ToString()))}]");
                }

                for (int j = 0; j < l.Objects.Count; j++)
                {
                    var o = l.Objects[j];
                    sw.WriteLine(
                        $"    Extra {j}: {o.TextureNumber} {o.AnimationFrames} {o.Width}x{o.Height} M:{o.MapWidth}x{o.MapHeight}");
                }
            }
        }

        public static void MapData(IAssetManager assets, ITextFormatter tf, string baseDir)
        {
            using var sw = File.CreateText($@"{baseDir}\re\MapInfo.txt");
            for (int i = 100; i < 400; i++)
            {
                IMapData map = assets.LoadMap((MapDataId)i);
                if (map == null)
                    continue;

                sw.Write($"{i} {(MapDataId)i} {map.MapType} ");
                sw.Write($"{map.Width}x{map.Height} ");
                sw.Write($"Palette:{map.PaletteId} ({(int)map.PaletteId}) ");
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
                    sw.Write($"Sound?:{map3d.Sound} ");
                    lab = assets.LoadLabyrinthData(map3d.LabDataId);
                }

                sw.Write($"Song:{map.SongId} ({(int?)map.SongId}) ");
                sw.WriteLine($"CombatBackground:{map.CombatBackgroundId} ({(int)map.CombatBackgroundId})");

                for(int j = 0; j < map.Npcs.Count; j++)
                {
                    if (!map.Npcs.TryGetValue(j, out var npc))
                        continue;

                    var wp = npc.Waypoints.FirstOrDefault();
                    var idText = npc.Id.ToString().PadLeft(15);

                    sw.Write($"    Npc{j:D3}: {idText} ({(int?)npc.Id:D3}) ");
                    sw.Write($"X:{wp.X:D3} Y:{wp.Y:D3} ");
                    sw.Write($"U8:{npc.Unk8} ");
                    sw.Write($"U9:{npc.Unk9} ");
                    sw.Write($"F{(int)npc.Flags:X2}:({npc.Flags}) ");
                    sw.Write($"M:{npc.Movement} ");
                    sw.Write($"S:{npc.Sound} ({(int?)npc.Sound}) ");
                    switch (map.MapType)
                    {
                        case MapType.TwoD: sw.Write($"O:{(LargeNpcId)npc.ObjectNumber} ({npc.ObjectNumber}) "); break;
                        case MapType.TwoDOutdoors: sw.Write($"O:{(SmallNpcId)npc.ObjectNumber} ({npc.ObjectNumber}) "); break;
                        case MapType.ThreeD:
                            if (lab != null)
                            {
                                var objectData = lab.ObjectGroups[npc.ObjectNumber - 1];
                                foreach (var subObject in objectData.SubObjects)
                                {
                                    var def = lab.Objects[subObject.ObjectInfoNumber];
                                    sw.Write($"O:{def.TextureNumber} ");
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
                            if (e.Event is BaseTextEvent textEvent)
                            {
                                var textSource = tf.Format(textEvent.ToId());
                                var text = string.Join(", ", textSource.Get().Select(x => x.Text));
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

        public static void CharacterSheets(IAssetManager assets, ITextFormatter tf, string baseDir)
        {
            {
                using var sw = File.CreateText($@"{baseDir}\re\PartyCharacters.txt");
                foreach (PartyCharacterId charId in Enum.GetValues(typeof(PartyCharacterId)))
                    DumpCharacterSheet(charId, assets.LoadPartyMember(charId), sw, assets, tf);
            }

            {
                using var sw = File.CreateText($@"{baseDir}\re\NpcCharacters.txt");
                foreach (NpcCharacterId charId in Enum.GetValues(typeof(NpcCharacterId)))
                    DumpCharacterSheet(charId, assets.LoadNpc(charId), sw, assets, tf);
            }

            {
                using var sw = File.CreateText($@"{baseDir}\re\MonsterCharacters.txt");
                foreach (MonsterCharacterId charId in Enum.GetValues(typeof(MonsterCharacterId)))
                    DumpCharacterSheet(charId, assets.LoadMonster(charId), sw, assets, tf);
            }
        }

        static void DumpCharacterSheet<T>(T id, CharacterSheet c, StreamWriter sw, IAssetManager assets, ITextFormatter tf) where T : Enum
        {
            if (c == null || c.GermanName == "" && c.PortraitId == 0)
                return;

            sw.WriteLine($"{Convert.ToInt32(id):D3} {id} ({c.EnglishName}, {c.GermanName}, {c.FrenchName})");
            sw.WriteLine($"    Type:{c.Type} Gender:{c.Gender} Race:{c.Race} Class:{c.Class} Age:{c.Age} Level:{c.Level}");
            sw.WriteLine($"    Languages:{c.Languages} Sprite:{c.SpriteId} Portrait:{(int?)c.PortraitId}");
            if (c.Inventory.Slots != null)
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
            sw.WriteLine($"    Event Set {c.EventSetId}: {(eventSet == null ? "Not Found" : $"{eventSet.Chains.Count()} chains")}");
            if (eventSet != null)
            {
                sw.WriteLine("    Chain Offsets: " + string.Join(", ", eventSet.Chains.Select((x, i) => $"{i}:{x.Id}")));
                foreach (var e in eventSet.Events)
                {
                    if (e.Event is BaseTextEvent textEvent)
                    {
                        var textSource = tf.Format(textEvent.ToId());
                        var text = string.Join(", ", textSource.Get().Select(x => x.Text));
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
            if(c.UnknownBlock6C.Any(x => x != 0)) sw.WriteLine($"    Unknown6C:{AnnotatedFormatWriter.ConvertToHexString(c.UnknownBlock6C)}");
            if (c.Unknown7E != 0) sw.WriteLine($"    Unknown7E:{c.Unknown7E}");
            if (c.Unknown80 != 0) sw.WriteLine($"    Unknown80:{c.Unknown80}");
            if (c.Unknown86 != 0) sw.WriteLine($"    Unknown86:{c.Unknown86}");
            if (c.Unknown88 != 0) sw.WriteLine($"    Unknown88:{c.Unknown88}");
            if (c.Unknown8E != 0) sw.WriteLine($"    Unknown8E:{c.Unknown8E}");
            if (c.Unknown90 != 0) sw.WriteLine($"    Unknown90:{c.Unknown90}");
            if(c.UnknownBlock96.Any(x => x != 0)) sw.WriteLine($"    Unknown96:{AnnotatedFormatWriter.ConvertToHexString(c.UnknownBlock96)}");
            if (c.UnknownCE != 0) sw.WriteLine($"    UnknownCE:{c.UnknownCE}");
            if (c.UnknownD6 != 0) sw.WriteLine($"    UnknownD6:{c.UnknownD6}");
            if(c.UnknownBlockDA.Any(x => x != 0)) sw.WriteLine($"    UnknownDA:{AnnotatedFormatWriter.ConvertToHexString(c.UnknownBlockDA)}");
            if (c.UnknownFA != 0) sw.WriteLine($"    UnknownFA:{c.UnknownFA}");
            if (c.UnknownFC != 0) sw.WriteLine($"    UnknownFC:{c.UnknownFC}");
        }

        public static void Chests(IAssetManager assets, string baseDir)
        {
            {
                using var sw = File.CreateText($@"{baseDir}\re\ChestInfo.txt");
                var chests = Enum.GetValues(typeof(ChestId)).Cast<ChestId>().ToDictionary(x => x, assets.LoadChest);
                foreach (var chest in chests.Where(x => x.Value != null))
                {
                    sw.WriteLine($"Chest {(int)chest.Key} {chest.Key}: ({chest.Value.Gold.Amount/10.0} gold, {chest.Value.Rations} rations)");
                    foreach(var x in chest.Value.Slots.Where(x => x.Item != null))
                        sw.WriteLine($"    {x.Amount}x{x.Item} Charges:{x.Charges} Enchantment:{x.Enchantment} Flags:{x.Flags}");
                }
            }

            {
                using var sw = File.CreateText($@"{baseDir}\re\MerchantInfo.txt");
                var merchants = Enum.GetValues(typeof(MerchantId)).Cast<MerchantId>().ToDictionary(x => x, assets.LoadMerchant);
                foreach (var merchant in merchants.Where(x => x.Value != null))
                {
                    sw.WriteLine($"Merchant {(int)merchant.Key} {merchant.Key}: ({merchant.Value.Gold.Amount/10.0} gold, {merchant.Value.Rations} rations)");
                    foreach(var x in merchant.Value.Slots.Where(x => x.Item != null))
                        sw.WriteLine($"    {x.Amount}x{x.Item} Charges:{x.Charges} Enchantment:{x.Enchantment} Flags:{x.Flags}");
                }
            }
        }

        public static void ItemData(IAssetManager assets, string baseDir)
        {
            using var sw = File.CreateText($@"{baseDir}\re\ItemInfo.txt");
            var items = new List<ItemData>();
            foreach (ItemId itemId in Enum.GetValues(typeof(ItemId)))
            {
                sw.Write(((int)itemId).ToString().PadLeft(3)); 
                sw.Write(' ');
                sw.Write(itemId.ToString().PadRight(20));

                var data = assets.LoadItem(itemId);
                if (data == null)
                    sw.Write("null");
                else
                {
                    items.Add(data);
                    sw.Write($"Gfx:{(ushort)data.Icon:D3} {data.IconAnim} frames ");
                    sw.Write($"Type:{data.TypeId} Slot:{data.SlotType} ");
                    sw.Write($"F:{data.Flags} A:{data.Activate} ");
                    sw.Write($"{data.Class}");
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
                    if (branch.NextEventWhenFalse != null)
                        PrintChain(sw, formatter, branch.NextEventWhenFalse, indent + 1);
                    sw.WriteLine("}".PadLeft(4 + indent * 4));
                    sw.WriteLine("else...".PadLeft(10 + indent * 4));
                }
                else sw.WriteLine(formatter.GetText(e));
                e = e.NextEvent;
            } while (e != null);
        }

        static void PrintEvent(StreamWriter sw, EventFormatter formatter, IEventNode e, int? chainId)
        {
            if(chainId.HasValue)
            {
                sw.Write('C');
                sw.Write(chainId.Value.ToString().PadRight(3));
            }
            else
                sw.Write("    ");

            sw.WriteLine(formatter.GetText(e));
        }

        static void DumpMapEvents(StreamWriter sw, IAssetManager assets, MapDataId mapId, IMapData map)
        {
            var formatter = new EventFormatter(assets, AssetType.MapText, (ushort)mapId);
            sw.WriteLine();
            sw.WriteLine($"Map {(int)mapId} {mapId}:");
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

        public static void MapEvents(IAssetManager assets, string baseDir)
        {
            using var sw = File.CreateText($@"{baseDir}\re\AllMapEvents.txt");
            foreach (var mapId in Enum.GetValues(typeof(MapDataId)).Cast<MapDataId>())
            {
                IMapData map = assets.LoadMap(mapId);
                if (map != null)
                    DumpMapEvents(sw, assets, mapId, map);
            }
        }

        public static void EventSets(AssetManager assets, string baseDir)
        {
            using var sw = File.CreateText($@"{baseDir}\re\AllEventSets.txt");
            foreach (var eventSetId in Enum.GetValues(typeof(EventSetId)).Cast<EventSetId>())
            {
                sw.WriteLine();
                sw.WriteLine($"EventSet{(int)eventSetId}:");
                var set = assets.LoadEventSet(eventSetId);
                if (set == null)
                    continue;

                var formatter = new EventFormatter(assets, AssetType.EventText, (ushort)eventSetId);
                foreach (var e in set.Events)
                {
                    var chainId = set.Chains.Select((x, i) => x.FirstEvent == e ? i : (int?) null).FirstOrDefault(x => x != null);
                    PrintEvent(sw, formatter, e, chainId);
                }
            }
        }

        public static void Spells(AssetManager assets, ITextFormatter tf, string baseDir)
        {
            using var sw = File.CreateText($@"{baseDir}\re\Spells.txt");
            foreach (var spellId in Enum.GetValues(typeof(SpellId)).Cast<SpellId>())
            {
                var spell = assets.LoadSpell(spellId);
                var systemTextId = (SystemTextId)((int)spellId + (int)SpellData.SystemTextOffset);
                var systemText = assets.LoadString(systemTextId, GameLanguage.English);
                int classNumber = (int)spellId / SpellData.MaxSpellsPerClass;
                int offsetInClass = (int)spellId % SpellData.MaxSpellsPerClass;
                sw.Write($"Spell{(int)spellId:D3} {classNumber}_{offsetInClass}");
                sw.Write($"({spellId}) ".PadRight(24));
                sw.Write($"\"{systemText}\" ".PadRight(24));
                sw.Write($"{spell.Cost} ".PadLeft(4));
                sw.Write($"Lvl:{spell.LevelRequirement} ");
                sw.Write($"Env:{spell.Environment} ");
                sw.Write($"Target:{spell.Targets} ");
                sw.WriteLine();
            }
        }
    }

    class EventFormatter
    {
        readonly IAssetManager _assets;
        readonly AssetType _textType;
        readonly ushort _context;

        public EventFormatter(IAssetManager assets, AssetType textType, ushort context)
        {
            _assets = assets;
            _textType = textType;
            _context = context;
        }

        public string GetText(IEventNode e)
        {
            var nodeText = e.ToString();
            if (e.Event is BaseTextEvent textEvent)
            {
                var text = _assets.LoadString(
                    new StringId(_textType, _context, textEvent.TextId),
                    GameLanguage.English);

                return $"{nodeText} \"{text}\"";
            }

            return nodeText;
        }
    }
}

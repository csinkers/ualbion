using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UAlbion.Core.Textures;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game;
using UAlbion.Game.Events;
using TextEvent = UAlbion.Formats.MapEvents.TextEvent;

namespace UAlbion
{
    static class Dump
    {
        public static void CoreSprites(IAssetManager assets, string baseDir)
        {
            var dir = $@"{baseDir}\data\exported\MAIN.EXE";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            // Dump all core sprites
            var palette = assets.LoadPalette(PaletteId.Main3D);
            for (int i = 0; i < 86; i++)
            {
                var name = $"{i}_{(CoreSpriteId)i}";
                var coreSprite = assets.LoadTexture((CoreSpriteId)i);
                var multiTexture = new MultiTexture(name, palette.GetCompletePalette());
                multiTexture.AddTexture(1, coreSprite, 0, 0, null, false);
                multiTexture.SavePng(1, 0, $@"{dir}\{name}.bmp");
            }
        }

        public static void MapAndLabData(IAssetManager assets, string baseDir)
        {
            using var sw = File.CreateText($@"{baseDir}\re\3DInfo.txt");
            // Dump map and lab data 
            for (int i = 100; i < 400; i++)
            {
                var map = assets.LoadMap3D((MapDataId) i);
                if (map == null)
                    continue;

                sw.WriteLine(
                    $"{i} {(MapDataId) i} {map.Width}x{map.Height} L{(int?) map.LabDataId} P{(int) map.PaletteId}:{map.PaletteId}");
                var floors = map.Floors.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                sw.WriteLine("    Floors: " + string.Join(" ", floors.Select(x => $"{x.Item1}:{x.Item2}")));
                var ceilings = map.Ceilings.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                sw.WriteLine("    Ceilings: " + string.Join(" ", ceilings.Select(x => $"{x.Item1}:{x.Item2}")));
                var contents = map.Contents.GroupBy(x => x).Select(x => (x.Key, x.Count())).OrderBy(x => x.Item1);
                sw.WriteLine("    Contents: " + string.Join(" ", contents.Select(x => $"{x.Item1}:{x.Item2}")));
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

        public static void CharacterSheets(IAssetManager assets)
        {
            var chars = new List<CharacterSheet>();
            foreach (PartyCharacterId charId in Enum.GetValues(typeof(PartyCharacterId)))
                chars.Add(assets.LoadCharacter(AssetType.PartyMember, charId));
            foreach (NpcCharacterId charId in Enum.GetValues(typeof(NpcCharacterId)))
                chars.Add(assets.LoadCharacter(AssetType.Npc, charId));
            foreach (MonsterCharacterId charId in Enum.GetValues(typeof(MonsterCharacterId)))
                chars.Add(assets.LoadCharacter(AssetType.Monster, charId));

            chars = chars.Where(x => x != null && (x.GermanName != "" || x.PortraitId != 0)).ToList();
            foreach (var c in chars)
            {
                
            }
        }

        public static void Chests(IAssetManager assets)
        {
            var chests = Enum.GetValues(typeof(ChestId)).Cast<ChestId>().ToDictionary(x => x, assets.LoadChest);
            var merchants = Enum.GetValues(typeof(MerchantId)).Cast<MerchantId>().ToDictionary(x => x, assets.LoadMerchant);
            foreach (var chest in chests)
            {
            }
        }

        public static void ItemData(IAssetManager assets, string baseDir)
        {
            using var sw = File.CreateText($@"{baseDir}\re\ItemInfo.txt");
            foreach (ItemId itemId in Enum.GetValues(typeof(ItemId)))
            {
                sw.Write($"{(int)itemId} {itemId} ");
                var data = assets.LoadItem(itemId);
                if (data == null)
                    sw.Write("null");
                else
                {
                    sw.Write($"Gfx:{(ushort)data.Icon} {data.IconAnim} frames ");
                    sw.Write($"Type:{data.TypeId} Slot:{data.SlotType} ");
                    sw.Write($"F:{data.Flags} A:{data.Activate}");
                }

                sw.WriteLine();
            }
        }

        static void PrintChain(EventFormatter formatter, MapEvent e, int indent)
        {
            do
            {
                Console.Write($"{e.Id:000}");
                Console.Write("".PadRight(indent * 4));
                if(e is QueryEvent query)
                {
                    Console.WriteLine($"if (!{formatter.GetText(e)}) {{");
                    if (query.FalseEvent != null)
                        PrintChain(formatter, query.FalseEvent, indent + 1);
                    Console.WriteLine("}".PadLeft(4 + indent * 4));
                    Console.WriteLine("else...".PadLeft(10 + indent * 4));
                }
                else Console.WriteLine(formatter.GetText(e));
                e = e.NextEvent;
            } while (e != null);
        }

        public static void MapEvents(IAssetManager assets, string baseDir, MapDataId mapId)
        {
            var map = assets.LoadMap2D(mapId);
            if (map == null) // Just handle 2D for now
                return;

            var formatter = new EventFormatter(assets, mapId);
            var rootNodes = new HashSet<(bool, MapEventZone.TriggerType, int)>();
            foreach(var zone in map.Zones)
                rootNodes.Add((zone.Global, zone.Trigger, zone.EventNumber));

            var sorted =
                    rootNodes
                        .OrderByDescending(x => x.Item1)
                        .ThenBy(x => x.Item2)
                        .ThenBy(x => x.Item3)
                ;

            foreach(var (global, trigger, number) in sorted)
            {
                var e = map.Events[number];
                Console.WriteLine($"{(global ? "Global" : "Local")} {trigger}:");
                PrintChain(formatter, e, 1);
            }

            Console.ReadLine();
        }
    }

    class EventFormatter
    {
        readonly IAssetManager _assets;
        readonly MapDataId _mapContext;

        public EventFormatter(IAssetManager assets, MapDataId mapContext)
        {
            _assets = assets;
            _mapContext = mapContext;
        }

        public string GetText(MapEvent e)
        {
            if(e is TextEvent textEvent) // Same as npc text event?
            {
                var text = _assets.LoadString(
                    new StringId(AssetType.MapText, (int)_mapContext, textEvent.TextId), 
                    GameLanguage.English);

                return $"text Portrait:{textEvent.PortraitId} \"{text}\"";
            }
            else return e.ToString();
        }

    }
}

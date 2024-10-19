﻿using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using static BuildTestingMaps.Constants;

namespace BuildTestingMaps;

class Test3DMap
{
    const byte MapWidth = 64;
    const byte MapHeight = 64;
    //  static int Pos(int x, int y) => y * MapWidth + x;
    public static Dictionary<AssetId, object> Build(MapId mapId, TestLab lab1)
    {
        ArgumentNullException.ThrowIfNull(lab1);

        var assets = new Dictionary<AssetId, object>();
        var builder = new MapBuilder3D(mapId, Palette1Id, lab1, MapWidth, MapHeight);
        // int nextScriptId = 1;
        builder.Draw(map =>
        {
            map.Flags |= MapFlags.Unk8000;
            map.RestMode = RestMode.NoResting;
            map.LightingMode = MapLightingMode.AlwaysDark;

            Array.Fill(map.Floors, (byte)lab1.BlankFloorOffset);
            /*for (int i = 0; i < map.Contents.Length; i++)
            {
                var y = i / map.Width;
                var x = i % map.Width;
                map.Contents[i] =
                    x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1
                        ? Lab1.SolidWallOffset
                        : (byte)0;
            } */

            /*
            ushort n = 0;
            void Add(int x, int y, string name, Func<Func<string, int>, string> scriptBuilder)
            {
                for (var index = 0; index < name.Length; index++)
                {
                    var c = name[index];
                    map.Floors[Pos(x + index, y)] = Lab1.FloorIndexForChar(c);
                }

                builder!.SetChain(n, scriptBuilder);
                map.AddZone((byte)x, (byte)y, TriggerTypes.Manipulate, n);
                n++;
            }

            string Script(Func<Func<string, int>, string> scriptBuilder)
            {
                var text = scriptBuilder(builder!.AddMapText);
                var script = ScriptLoader.Parse(ApiUtil.SplitLines(text));
                var scriptId = new ScriptId(nextScriptId++);
                assets![scriptId] = script;
                return "do_script " + scriptId.Id;
            } //*/
/*
            // var waypoints = BuildPatrolPath(18, 6);
            map.Npcs[0] = new MapNpc
            {
                Id = (MonsterGroupId)UAlbion.Base.MonsterGroup.TwoSkrinn1OneKrondir1,
                Type = NpcType.Monster,
                Movement = NpcMovement.Stationary,
                Waypoints = new NpcWaypoint[] { new(16, 16) },
                SpriteOrGroup = new AssetId(AssetType.ObjectGroup, Lab1.TwoMonstersObjGroupOffset)
            };
*/
        });

        var (finalMap, mapText) = builder.Build();
        assets.Add(finalMap.Id, finalMap);
        assets.Add(finalMap.Id.ToMapText(), mapText);
        return assets;
    }
}

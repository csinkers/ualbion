﻿using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents
{
    [Event("npc_move")] // USED IN SCRIPT
    public class NpcMoveEvent : Event
    {
        public NpcMoveEvent(int npcId, int x, int y) { NpcId = npcId; X = x; Y = y; }
        [EventPart("npcId ")] public int NpcId { get; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}

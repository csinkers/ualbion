using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.ScriptEvents;

[Event("npc_jump", "Teleport the given NPC to the given position.")] // USED IN SCRIPT
public class NpcJumpEvent : Event, INpcEvent
{
    public NpcJumpEvent(byte npcNum, int? x, int? y) { NpcNum = npcNum; X = x; Y = y; }
    [EventPart("npcNum ")] public byte NpcNum { get; }
    [EventPart("x", true)] public int? X { get; }
    [EventPart("y", true)] public int? Y { get; }
}
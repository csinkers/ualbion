using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.ScriptEvents;

[Event("npc_on")] // USED IN SCRIPT
public class NpcOnEvent : Event, INpcEvent
{
    public NpcOnEvent(byte npcNum) { NpcNum = npcNum; }
    [EventPart("npcNum")] public byte NpcNum { get; }
}
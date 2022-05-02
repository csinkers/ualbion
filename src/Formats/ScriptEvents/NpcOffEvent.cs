using UAlbion.Api.Eventing;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.ScriptEvents;

[Event("npc_off")] // USED IN SCRIPT
public class NpcOffEvent : Event, INpcEvent
{
    public NpcOffEvent(byte npcNum) { NpcNum = npcNum; }
    [EventPart("npcNum")] public byte NpcNum { get; }
}
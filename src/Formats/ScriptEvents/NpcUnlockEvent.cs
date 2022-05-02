using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.ScriptEvents;

[Event("npc_unlock")] // USED IN SCRIPT
public class NpcUnlockEvent : Event, INpcEvent
{
    public NpcUnlockEvent(byte npcNum) { NpcNum = npcNum; }
    [EventPart("npcNum")] public byte NpcNum { get; }
}
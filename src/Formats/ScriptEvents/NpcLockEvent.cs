using UAlbion.Api;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.ScriptEvents;

[Event("npc_lock")] // USED IN SCRIPT
public class NpcLockEvent : Event, INpcEvent
{
    public NpcLockEvent(byte npcNum) { NpcNum = npcNum; }
    [EventPart("npcNum")] public byte NpcNum { get; }
}
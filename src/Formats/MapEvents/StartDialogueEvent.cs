using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("start_dialogue",
    "Initiates a conversation using the given npc id",
    "talk")]
public class StartDialogueEvent : MapEvent
{
    StartDialogueEvent() { }
    public StartDialogueEvent(NpcSheetId npcId) => NpcId = npcId;

    [EventPart("npc_id")]
    public NpcSheetId NpcId { get; private set; }

    public static StartDialogueEvent Serdes(StartDialogueEvent e, AssetMapping mapping, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new StartDialogueEvent();
        s.UInt8("Pad1", 1);
        s.UInt8("Pad2", 0);
        s.UInt8("Pad3", 0);
        s.UInt8("Pad4", 0);
        s.UInt8("Pad5", 0);
        e.NpcId = NpcSheetId.SerdesU8(nameof(NpcId), e.NpcId, mapping, s);
        s.UInt8("Pad7", 0);
        s.UInt16("Pad8", 0);
        return e;
    }

    public override MapEventType EventType => MapEventType.StartDialogue;
}
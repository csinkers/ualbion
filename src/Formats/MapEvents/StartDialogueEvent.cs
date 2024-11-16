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
    /*
    Good test conversations:
    111 Frill - Ask about item (FineIskaiDagger)
    123 Fasiir - Learn spells
    127 Rejira - Merchant, Heal, Cure, RemoveCurse
    144 Zirr - SleepInRoom, OrderFood
    146 Snird - Merchant, RepairItem
    156 Garris - Unk2 ???, AskAbout(TharnossPermit)
    159 Ferina - LearnCloseCombat
    173 Torko - AskOpinion (2x)
    176 Roves - ScrollMerchant
    224 Unk - Unk9 ?? (also 241_Riko)
    285 Konny - AskToJoin
    314 Drannagh - RestoreItemEnergy, LearnSpells
    981 Tom - UnkE
    984 Sira2 - AskToLeave, PartySleeps, Unk2D, Unk17
    986 Harriet - DropItem(GodesssAmulet)
    */

    StartDialogueEvent() { }
    public StartDialogueEvent(NpcSheetId npcId) => NpcId = npcId;

    [EventPart("npc_id")]
    public NpcSheetId NpcId { get; private set; }

    public static StartDialogueEvent Serdes(StartDialogueEvent e, AssetMapping mapping, ISerdes s)
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
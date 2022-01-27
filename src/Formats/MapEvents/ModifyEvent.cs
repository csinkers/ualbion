using System;
using System.ComponentModel;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents;

public abstract class ModifyEvent : MapEvent
{
    public static ModifyEvent BaseSerdes(ModifyEvent genericEvent, AssetMapping mapping, AssetId eventSource, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        var subType = s.EnumU8("SubType", genericEvent?.SubType ?? ModifyType.Unk2);
        return subType switch
        {
            ModifyType.Switch => SwitchEvent.Serdes((SwitchEvent)genericEvent, mapping, s), // 0
            ModifyType.DisableEventChain => DisableEventChainEvent.Serdes((DisableEventChainEvent)genericEvent, eventSource, s), // 1
            ModifyType.Unk2 => ModifyUnk2Event.Serdes((ModifyUnk2Event)genericEvent, s), // 2
            ModifyType.NpcActive => NpcDisabledEvent.Serdes((NpcDisabledEvent)genericEvent, s), // 4
            ModifyType.AddPartyMember => AddPartyMemberEvent.Serdes((AddPartyMemberEvent)genericEvent, mapping, s), // 5
            ModifyType.InventoryItem => AddRemoveInventoryItemEvent.Serdes((AddRemoveInventoryItemEvent)genericEvent, mapping, s), // 6
            ModifyType.MapLighting => SetMapLightingEvent.Serdes((SetMapLightingEvent)genericEvent, s), // B
            ModifyType.PartyGold => ChangePartyGoldEvent.Serdes((ChangePartyGoldEvent)genericEvent, s), // F
            ModifyType.PartyRations => ChangePartyRationsEvent.Serdes((ChangePartyRationsEvent)genericEvent, s), // 10
            ModifyType.Time => ChangeTimeEvent.Serdes((ChangeTimeEvent)genericEvent, s), // 12
            ModifyType.Leader => SetPartyLeaderEvent.Serdes((SetPartyLeaderEvent)genericEvent, mapping, s), // 1A
            ModifyType.Ticker => TickerEvent.Serdes((TickerEvent)genericEvent, mapping, s), // 1C
            _ => throw new InvalidEnumArgumentException(nameof(subType), (int)subType, typeof(ModifyType))
        };
    }

    public enum ModifyType : byte
    {
        Switch = 0,
        DisableEventChain = 1,
        Unk2 = 2,
        Unk3 = 3,
        NpcActive = 4,
        AddPartyMember = 5,
        InventoryItem = 6,
        Unk7 = 7,
        Unk8 = 8,
        Unk9 = 9,
        UnkA = 0xA,
        MapLighting = 0xB,
        UnkC = 0xC,
        UnkD = 0xD,
        UnkE = 0xE,
        PartyGold = 0xF,
        PartyRations = 0x10,
        Unk11 = 0x11,
        Time = 0x12,
        Unk13 = 0x13,
        Unk14 = 0x14,
        Unk15 = 0x15,
        Unk16 = 0x16,
        Unk17 = 0x17,
        Unk18 = 0x18,
        Unk19 = 0x19,
        Leader = 0x1A,
        Unk1B = 0x1B,
        Ticker = 0x1C
    }

    public abstract ModifyType SubType { get; }
    public override MapEventType EventType => MapEventType.Modify;
}
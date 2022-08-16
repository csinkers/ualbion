using System;
using System.ComponentModel;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents;

public abstract class ModifyEvent : MapEvent
{
    public static ModifyEvent BaseSerdes(ModifyEvent genericEvent, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        var subType = s.EnumU8("SubType", genericEvent?.SubType ?? ModifyType.DoorOpen);
        return subType switch
        {
            ModifyType.Switch         =>          SwitchEvent.Serdes(         (SwitchEvent)genericEvent, mapping, s), // 0
            ModifyType.EventChainOff  =>   EventChainOffEvent.Serdes(  (EventChainOffEvent)genericEvent, mapping, s), // 1
            ModifyType.DoorOpen       =>        DoorOpenEvent.Serdes(       (DoorOpenEvent)genericEvent, mapping, s), // 2
            ModifyType.ChestOpen      =>       SetChestOpenEvent.Serdes(      (SetChestOpenEvent)genericEvent, mapping, s), // 3
            ModifyType.NpcOff         =>    ModifyNpcOffEvent.Serdes(   (ModifyNpcOffEvent)genericEvent, mapping, s), // 4
            ModifyType.AddPartyMember =>  AddPartyMemberEvent.Serdes( (AddPartyMemberEvent)genericEvent, mapping, s), // 5
            ModifyType.ItemCount      => ModifyItemCountEvent.Serdes((ModifyItemCountEvent)genericEvent, mapping, s), // 6
            ModifyType.WordKnown      =>       WordKnownEvent.Serdes(      (WordKnownEvent)genericEvent, mapping, s), // 8
            ModifyType.MapLighting    =>     MapLightingEvent.Serdes(    (MapLightingEvent)genericEvent, s),          // B
            ModifyType.PartyGold      =>      ModifyGoldEvent.Serdes(     (ModifyGoldEvent)genericEvent, s),          // F
            ModifyType.PartyRations   =>   ModifyRationsEvent.Serdes(  (ModifyRationsEvent)genericEvent, s),          // 10
            ModifyType.TimeHours      =>    ModifyHoursEvent.Serdes(     (ModifyHoursEvent)genericEvent, s),          // 12
            ModifyType.Leader         =>  SetPartyLeaderEvent.Serdes( (SetPartyLeaderEvent)genericEvent, mapping, s), // 1A
            ModifyType.TimeDays       =>      ModifyDaysEvent.Serdes(     (ModifyDaysEvent)genericEvent, s),          // 1B
            ModifyType.Ticker         =>          TickerEvent.Serdes(         (TickerEvent)genericEvent, mapping, s), // 1C
            ModifyType.TimeMTicks     =>  ModifyMTicksEvent.Serdes(     (ModifyMTicksEvent)genericEvent, s),          // 1E
            _ => throw new InvalidEnumArgumentException(nameof(subType), (int)subType, typeof(ModifyType))
        };
    }

    public enum ModifyType : byte
    {
        Switch = 0,
        EventChainOff = 1,
        DoorOpen = 2,
        ChestOpen = 3,
        NpcOff = 4,
        AddPartyMember = 5,
        ItemCount = 6,
        Unused7 = 7,
        WordKnown = 8,
        Unused9 = 9,
        UnusedA = 0xA,
        MapLighting = 0xB,
        UnusedC = 0xC,
        UnusedD = 0xD,
        UnusedE = 0xE,
        PartyGold = 0xF,
        PartyRations = 0x10,
        Unused11 = 0x11,
        TimeHours = 0x12, // Only supports SetAmount + AddAmount
        Unused13 = 0x13,
        Unused14 = 0x14,
        Unused15 = 0x15,
        Unused16 = 0x16,
        Unused17 = 0x17,
        Unused18 = 0x18,
        Unused19 = 0x19,
        Leader = 0x1A,
        TimeDays = 0x1B, // Only supports SetAmount (day of month [0..29]) + AddAmount
        Ticker = 0x1C,
        Unused1D = 0x1D,
        TimeMTicks = 0x1E // Only supports SetAmount (mtick of hour [0..47]) + AddAmount
    }

    public abstract ModifyType SubType { get; }
    public override MapEventType EventType => MapEventType.Modify;
}

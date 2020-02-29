using System;
using UAlbion.Api;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public abstract class ModifyEvent : Event, IMapEvent
    {
        public static ModifyEvent Serdes(ModifyEvent genericEvent, ISerializer s)
        {
            var subType = s.EnumU8("SubType", genericEvent?.SubType ?? ModifyType.Unk2);
            switch (subType)
            {
                case ModifyType.SetTemporarySwitch:     return SetTemporarySwitchEvent.Serdes((SetTemporarySwitchEvent)genericEvent, s);
                case ModifyType.DisableEventChain:      return DisableEventChainEvent.Serdes((DisableEventChainEvent)genericEvent, s);
                case ModifyType.SetNpcActive:           return SetNpcActiveEvent.Serdes((SetNpcActiveEvent)genericEvent, s);
                case ModifyType.AddPartyMember:         return AddPartyMemberEvent.Serdes((AddPartyMemberEvent)genericEvent, s);
                case ModifyType.AddRemoveInventoryItem: return AddRemoveInventoryItemEvent.Serdes((AddRemoveInventoryItemEvent)genericEvent, s);
                case ModifyType.SetMapLighting:         return SetMapLightingEvent.Serdes((SetMapLightingEvent)genericEvent, s);
                case ModifyType.ChangePartyGold:        return ChangePartyGoldEvent.Serdes((ChangePartyGoldEvent)genericEvent, s);
                case ModifyType.ChangePartyRations:     return ChangePartyRationsEvent.Serdes((ChangePartyRationsEvent)genericEvent, s);
                case ModifyType.ChangeTime:             return ChangeTimeEvent.Serdes((ChangeTimeEvent)genericEvent, s);
                case ModifyType.SetPartyLeader:         return SetPartyLeaderEvent.Serdes((SetPartyLeaderEvent)genericEvent, s);
                case ModifyType.SetTicker:              return SetTickerEvent.Serdes((SetTickerEvent)genericEvent, s);
                case ModifyType.Unk2:                   return DummyModifyEvent.Serdes((DummyModifyEvent)genericEvent, s);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public enum ModifyType : byte
        {
            SetTemporarySwitch = 0,
            DisableEventChain = 1,
            Unk2 = 2,
            SetNpcActive = 4,
            AddPartyMember = 5,
            AddRemoveInventoryItem = 6,
            SetMapLighting = 0xB,
            ChangePartyGold = 0xF,
            ChangePartyRations = 0x10,
            ChangeTime = 0x12,
            SetPartyLeader = 0x1A,
            SetTicker = 0x1C
        }

        public abstract ModifyType SubType { get; }
        public MapEventType EventType => MapEventType.Modify;
    }
}

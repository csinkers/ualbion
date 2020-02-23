using System;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public abstract class ModifyEvent : IMapEvent
    {
        public static ModifyEvent Serdes(ModifyEvent genericEvent, ISerializer s)
        {
            var subType = s.EnumU8("SubType", genericEvent?.SubType ?? ModifyType.Unk2);
            switch (subType)
            {
                case ModifyType.SetTemporarySwitch:     return SetTemporarySwitchEvent.Translate((SetTemporarySwitchEvent)genericEvent, s);
                case ModifyType.DisableEventChain:      return DisableEventChainEvent.Translate((DisableEventChainEvent)genericEvent, s);
                case ModifyType.SetNpcActive:           return SetNpcActiveEvent.Translate((SetNpcActiveEvent)genericEvent, s);
                case ModifyType.AddPartyMember:         return AddPartyMemberEvent.Translate((AddPartyMemberEvent)genericEvent, s);
                case ModifyType.AddRemoveInventoryItem: return AddRemoveInventoryItemEvent.Translate((AddRemoveInventoryItemEvent)genericEvent, s);
                case ModifyType.SetMapLighting:         return SetMapLightingEvent.Translate((SetMapLightingEvent)genericEvent, s);
                case ModifyType.ChangePartyGold:        return ChangePartyGoldEvent.Translate((ChangePartyGoldEvent)genericEvent, s);
                case ModifyType.ChangePartyRations:     return ChangePartyRationsEvent.Translate((ChangePartyRationsEvent)genericEvent, s);
                case ModifyType.ChangeTime:             return ChangeTimeEvent.Translate((ChangeTimeEvent)genericEvent, s);
                case ModifyType.SetPartyLeader:         return SetPartyLeaderEvent.Translate((SetPartyLeaderEvent)genericEvent, s);
                case ModifyType.SetTicker:              return SetTickerEvent.Translate((SetTickerEvent)genericEvent, s);
                case ModifyType.Unk2:                   return DummyModifyEvent.Translate((DummyModifyEvent)genericEvent, s);
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

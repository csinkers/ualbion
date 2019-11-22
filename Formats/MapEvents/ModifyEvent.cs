using System;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public abstract class ModifyEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            var subType = (ModifyType) br.ReadByte();
            switch (subType)
            {
                case ModifyType.SetTemporarySwitch:     return SetTemporarySwitchEvent.Load(br, id, type, subType);
                case ModifyType.DisableEventChain:      return DisableEventChainEvent.Load(br, id, type, subType);
                case ModifyType.SetNpcActive:           return SetNpcActiveEvent.Load(br, id, type, subType);
                case ModifyType.AddPartyMember:         return AddPartyMemberEvent.Load(br, id, type, subType);
                case ModifyType.AddRemoveInventoryItem: return AddRemoveInventoryItemEvent.Load(br, id, type, subType);
                case ModifyType.SetMapLighting:         return SetMapLightingEvent.Load(br, id, type, subType);
                case ModifyType.ChangePartyGold:        return ChangePartyGoldEvent.Load(br, id, type, subType);
                case ModifyType.ChangePartyRations:     return ChangePartyRationsEvent.Load(br, id, type, subType);
                case ModifyType.ChangeTime:             return ChangeTimeEvent.Load(br, id, type, subType);
                case ModifyType.SetPartyLeader:         return SetPartyLeaderEvent.Load(br, id, type, subType);
                case ModifyType.SetTicker:              return SetTickerEvent.Load(br, id, type, subType);
                case ModifyType.Unk2:                   return DummyModifyEvent.Load(br, id, type, subType);
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

        public ModifyType SubType { get; protected set; }
    }
}

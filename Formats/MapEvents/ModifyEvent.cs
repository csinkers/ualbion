using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public abstract class ModifyEvent : MapEvent
    {
        public static MapEvent Load(BinaryReader br, int id, EventType type)
        {
            var subType = (ModifyType) br.ReadByte();
            switch (subType)
            {
                case ModifyType.SetTemporarySwitch:     return new SetTemporarySwitchEvent(br, id, type, subType);
                case ModifyType.DisableEventChain:      return new DisableEventChainEvent(br, id, type, subType);
                case ModifyType.SetNpcActive:           return new SetNpcActiveEvent(br, id, type, subType);
                case ModifyType.AddPartyMember:         return new AddPartyMemberEvent(br, id, type, subType);
                case ModifyType.AddRemoveInventoryItem: return new AddRemoveInventoryItemEvent(br, id, type, subType);
                case ModifyType.SetMapLighting:         return new SetMapLightingEvent(br, id, type, subType);
                case ModifyType.ChangePartyGold:        return new ChangePartyGoldEvent(br, id, type, subType);
                case ModifyType.ChangePartyRations:     return new ChangePartyRationsEvent(br, id, type, subType);
                case ModifyType.ChangeTime:             return new ChangeTimeEvent(br, id, type, subType);
                case ModifyType.SetPartyLeader:         return new SetPartyLeaderEvent(br, id, type, subType);
                case ModifyType.SetTicker:              return new SetTickerEvent(br, id, type, subType);
                case ModifyType.Unk2:                   return new DummyModifyEvent(br, id, type, subType);
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

        public ModifyType SubType { get; }

        protected ModifyEvent(int id, EventType type, ModifyType subType) : base(id, type)
        {
            SubType = subType;
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SetTemporarySwitchEvent : ModifyEvent
    {
        public SetTemporarySwitchEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            SwitchValue = br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            SwitchId = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public byte SwitchValue { get; }
        public ushort SwitchId { get; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk8 { get; set; }
    }

    public class DisableEventChainEvent : ModifyEvent
    {
        public DisableEventChainEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Unk2 = br.ReadByte(); // 2
            Debug.Assert(Unk2 == 1 || Unk2 == 0 || Unk2 == 2); // Usually 1
            ChainNumber = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            Unk6 = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public byte Unk2 { get; }
        public byte ChainNumber { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; }
        public ushort Unk8 { get; set; }
    }

    public class SetNpcActiveEvent : ModifyEvent
    {
        public SetNpcActiveEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            IsActive = br.ReadByte(); // 2
            NpcIp = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            Unk6 = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public byte IsActive { get; }
        public byte NpcIp { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; }
        public ushort Unk8 { get; set; }
    }

    public class AddPartyMemberEvent : ModifyEvent
    {
        public AddPartyMemberEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Unk2 = br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            PartyMemberId = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public byte Unk2 { get; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort PartyMemberId { get; }
        public ushort Unk8 { get; set; }
    }

    public class AddRemoveInventoryItemEvent : ModifyEvent
    {
        public AddRemoveInventoryItemEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Operation = (QuantityChangeOperation)br.ReadByte(); // 2
            Amount = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            ItemId = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public QuantityChangeOperation Operation { get; }
        public byte Amount { get; set; }
        public ushort ItemId { get; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk8 { get; set; }
    }

    public class SetMapLightingEvent : ModifyEvent
    {
        public enum LightingLevel
        {
            Normal = 0,
            NeedTorch = 1,
            FadeFromBlack = 2
        }

        public SetMapLightingEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Unk2 = br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            LightLevel = (LightingLevel)br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public byte Unk2 { get; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public LightingLevel LightLevel { get; }
        public ushort Unk8 { get; set; }
    }

    public class ChangePartyGoldEvent : ModifyEvent
    {
        public ChangePartyGoldEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Operation = (QuantityChangeOperation)br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            Amount = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public QuantityChangeOperation Operation { get; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Amount { get; }
        public ushort Unk8 { get; set; }
    }

    public class ChangePartyRationsEvent : ModifyEvent
    {
        public ChangePartyRationsEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Operation = (QuantityChangeOperation)br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            Amount = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public QuantityChangeOperation Operation { get; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Amount { get; }
        public ushort Unk8 { get; set; }
    }

    public class ChangeTimeEvent : ModifyEvent
    {
        public ChangeTimeEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Operation = (QuantityChangeOperation)br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            Amount = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public QuantityChangeOperation Operation { get; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Amount { get; }
        public ushort Unk8 { get; set; }
    }

    public class SetPartyLeaderEvent : ModifyEvent
    {
        public SetPartyLeaderEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Unk2 = br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            PartyMemberId = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public byte Unk2 { get; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort PartyMemberId { get; }
        public ushort Unk8 { get; set; }
    }

    public class SetTickerEvent : ModifyEvent
    {
        public SetTickerEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Operation = (QuantityChangeOperation)br.ReadByte(); // 2
            Amount = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            TickerId = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public QuantityChangeOperation Operation { get; }
        public byte Amount { get; set; }
        public ushort TickerId { get; }

        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk8 { get; set; }
    }

    public class DummyModifyEvent : ModifyEvent
    {
        public DummyModifyEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Unk2 = br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            Unk6 = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public byte Unk2 { get; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; }
        public ushort Unk8 { get; set; }
    }

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

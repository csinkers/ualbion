using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class QueryEvent : MapEvent
    {
        public enum QueryType : byte
        {
            TemporarySwitch = 0x0,
            HasPartyMember = 0x5,
            InventoryHasItem = 0x6,
            UsedItemId = 0x7,
            PreviousActionResult = 0x9,
            IsScriptDebugModeActive = 0xA,
            IsNpcActive = 0xE,
            HasEnoughGold = 0xF,
            RandomChance = 0x11,
            ChosenVerb = 0x14,
            IsPartyMemberConscious = 0x15,
            IsPartyMemberLeader = 0x1A,
            Ticker = 0x1C,
            CurrentMapId = 0x1D,
            PromptPlayer = 0x1F,
            TriggerType = 0x20,
            EventAlreadyUsed = 0x22,
            IsDemoVersion = 0x23,
            PromptPlayerNumeric = 0x2B
        }

        public enum VerbType : byte
        {
            Examine = 1,
            Manipulate = 2,
            Speak = 3,
            UseItem = 4,
        }

        public QueryEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            SubType = (QueryType)br.ReadByte(); // 1
            Unk2 = br.ReadByte(); // 2
            Unk3 = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            Argument = br.ReadUInt16(); // 6
            FalseEvent = br.ReadUInt16(); // 8
        }

        public byte Unk2 { get; } // method to use for check?
        public byte Unk3 { get; }
        public byte Unk4 { get; }
        public byte Unk5 { get; }

        public QueryType SubType { get; }
        public ushort FalseEvent { get; }
        public ushort Argument { get; }
    }
}
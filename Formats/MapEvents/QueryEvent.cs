using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class QueryVerbEvent : QueryEvent
    {
        public enum VerbType : byte
        {
            Examine = 1,
            Manipulate = 2,
            Speak = 3,
            UseItem = 4,
        }
        public QueryVerbEvent(int id, EventType type) : base(id, type) { }
        public VerbType Verb => (VerbType) Argument;
    }

    public class QueryItemEvent : QueryEvent
    {
        public QueryItemEvent(int id, EventType type) : base(id, type) { }
        public ItemId ItemId => (ItemId) Argument;
    }

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

        public QueryEvent(int id, EventType type) : base(id, type) { }

        public static QueryEvent Load(BinaryReader br, int id, EventType type) 
        {
            var subType = (QueryType)br.ReadByte(); // 1
            QueryEvent e;
            switch (subType)
            {
                case QueryType.InventoryHasItem:
                case QueryType.UsedItemId:
                    e = new QueryItemEvent(id, type);
                    break;

                case QueryType.ChosenVerb:
                    e = new QueryVerbEvent(id, type);
                    break;

                case QueryType.PreviousActionResult:
                case QueryType.Ticker:
                case QueryType.CurrentMapId:
                case QueryType.PromptPlayer:
                case QueryType.TriggerType:
                    e = new QueryEvent(id, type);
                    break;

                default:
                    e = new QueryEvent(id, type);
                    break;
            }

            e.SubType = subType;
            e.Unk2 = br.ReadByte(); // 2
            e.Unk3 = br.ReadByte(); // 3
            e.Unk4 = br.ReadByte(); // 4
            e.Unk5 = br.ReadByte(); // 5
            e.Argument = br.ReadUInt16(); // 6
            e.FalseEventId = br.ReadUInt16(); // 8
            if (e.FalseEventId == 0xffff) e.FalseEventId = null;
            return e;
        }

        public byte Unk2 { get; protected set;  } // method to use for check?
        public byte Unk3 { get; protected set;  }
        public byte Unk4 { get; protected set;  }
        public byte Unk5 { get; protected set;  }

        public QueryType SubType { get; protected set;  }
        public ushort? FalseEventId { get; protected set;  }
        public ushort Argument { get; protected set;  }
        public MapEvent FalseEvent { get; set; }

        public override string ToString() => $"Query {SubType} {Argument} (method {Unk2})";
    }
}
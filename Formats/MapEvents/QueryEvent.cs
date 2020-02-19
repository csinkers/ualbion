using System.Diagnostics;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class QueryEvent : IEvent
    {
        public enum QueryOperation
        {
            Unk0,
            Unk1,
            Unk2,
            Equals,
        }

        public static BranchNode Load(BinaryReader br, int id, MapEventType type)
        {
            var subType = (QueryType)br.ReadByte(); // 1
            switch (subType)
            {
                case QueryType.InventoryHasItem:
                case QueryType.UsedItemId:
                    return QueryItemEvent.Load(br, id, subType);

                case QueryType.ChosenVerb:
                    return QueryVerbEvent.Load(br, id);

                case QueryType.PreviousActionResult:
                case QueryType.Ticker:
                case QueryType.CurrentMapId:
                case QueryType.PromptPlayer:
                case QueryType.TriggerType:
                default:
                    break;
            }

            var e = new QueryEvent
            {
                SubType = subType,
                Unk2 = br.ReadByte(), // 2
                Unk3 = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                Argument = br.ReadUInt16(), // 6
            };
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);

            ushort? falseEventId = br.ReadUInt16(); // 8
            if (falseEventId == 0xffff)
                falseEventId = null;
            return new BranchNode(id, e, falseEventId);
        }

        public byte Unk2 { get; protected set; } // method to use for check? 0,1,2,3,4,5
        public byte Unk3 { get; protected set; } // immediate value?
        protected byte Unk4 { get; set; }
        protected byte Unk5 { get; set; }

        public QueryType SubType { get; protected set; }
        public ushort Argument { get; protected set; }

        public override string ToString() => $"query {SubType} {Argument} ({Unk2} {Unk3})";
    }
}

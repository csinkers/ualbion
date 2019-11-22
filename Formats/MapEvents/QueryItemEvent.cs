using System.IO;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class QueryItemEvent : QueryEvent
    {
        public static BranchNode Load(BinaryReader br, int id, QueryType subType)
        {
            var e = new QueryItemEvent
            {
                SubType = subType,
                Unk2 = br.ReadByte(), // 2
                Unk3 = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                Argument = br.ReadUInt16(), // 6
            };

            ushort? falseEventId = br.ReadUInt16(); // 8
            if (falseEventId == 0xffff)
                falseEventId = null;

            return new BranchNode(id, e, falseEventId);
        }
        public ItemId ItemId => (ItemId)Argument-1;
        public override string ToString() => $"query_item {SubType} {ItemId} (method {Unk2})";
    }
}

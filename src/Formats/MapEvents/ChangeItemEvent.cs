using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("change_item")]
    public class ChangeItemEvent : DataChangeEvent
    {
        ChangeItemEvent() { }
        public ChangeItemEvent(PartyMemberId partyMemberId, NumericOperation operation, ushort amount, ItemId itemId, byte unk3)
        {
            PartyMemberId = partyMemberId;
            Operation = operation;
            Amount = amount;
            ItemId = itemId;
            Unk3 = unk3;
        }

        public static ChangeItemEvent Serdes(ChangeItemEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new ChangeItemEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            int zeroed = s.UInt8(null, 0);
            e.PartyMemberId = PartyMemberId.SerdesU8(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
            e.ItemId = ItemId.SerdesU16(nameof(ItemId), e.ItemId, AssetType.Item, mapping, s);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            s.Assert(zeroed == 0, "ChangeEvent: Expected byte 4 to be 0");
            return e;
        }

        public override ChangeProperty ChangeProperty => ChangeProperty.Item;
        [EventPart("party_member")] public PartyMemberId PartyMemberId { get; private set; }
        [EventPart("op")] public NumericOperation Operation { get; private set; }
        [EventPart("amount")] public ushort Amount { get; private set; }
        [EventPart("item")] public ItemId ItemId { get; private set; }
        [EventPart("unk3", true, 0)] public byte Unk3 { get; private set; }
    }
}
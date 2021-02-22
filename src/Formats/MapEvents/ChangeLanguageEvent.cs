using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("change_language")]
    public class ChangeLanguageEvent : DataChangeEvent
    {
        ChangeLanguageEvent() { }
        public ChangeLanguageEvent(PartyMemberId partyMemberId, NumericOperation operation, PlayerLanguages language, byte unk3)
        {
            PartyMemberId = partyMemberId;
            Operation = operation;
            Language = language;
            Unk3 = unk3;
        }

        public static ChangeLanguageEvent Serdes(ChangeLanguageEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new ChangeLanguageEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            int zeroed = s.UInt8(null, 0);
            e.PartyMemberId = PartyMemberId.SerdesU8(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
            e.Language = s.EnumU8(nameof(Language), e.Language);
            s.UInt8(null, 0);
            zeroed += s.UInt16(null, 0);
            s.Assert(zeroed == 0, "ChangeEvent: Expected byte 4 to be 0");
            return e;
        }
        public override ChangeProperty ChangeProperty => ChangeProperty.Language;
        [EventPart("party_member")] public PartyMemberId PartyMemberId { get; private set; }
        [EventPart("op")] public NumericOperation Operation { get; private set; }
        [EventPart("language")] public PlayerLanguages Language { get; private set; }
        [EventPart("unk3", true, "0")] public byte Unk3 { get; private set; }
    }
}
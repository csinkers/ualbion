using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("set_party_leader")]
    public class SetPartyLeaderEvent : ModifyEvent
    {
        public static SetPartyLeaderEvent Serdes(SetPartyLeaderEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new SetPartyLeaderEvent();
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.PartyMemberId = PartyMemberId.SerdesU16(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        SetPartyLeaderEvent() { }
        public SetPartyLeaderEvent(PartyMemberId id)
        {
            PartyMemberId = id;
        }

        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        [EventPart("id")] public PartyMemberId PartyMemberId { get; private set; } // stored as ushort
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"set_party_leader {PartyMemberId} ({Unk2} {Unk3} {Unk4} {Unk5} {Unk8})";
        public override ModifyType SubType => ModifyType.SetPartyLeader;
    }
}

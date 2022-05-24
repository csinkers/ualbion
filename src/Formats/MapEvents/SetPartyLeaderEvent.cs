using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("set_party_leader")]
public class SetPartyLeaderEvent : ModifyEvent
{
    SetPartyLeaderEvent() { }
    public SetPartyLeaderEvent(PartyMemberId id, byte unk2, byte unk3)
    {
        PartyMemberId = id;
        Unk2 = unk2;
        Unk3 = unk3;
    }

    public static SetPartyLeaderEvent Serdes(SetPartyLeaderEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new SetPartyLeaderEvent();
        e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        int zeroed = s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        e.PartyMemberId = PartyMemberId.SerdesU16(nameof(PartyMemberId), e.PartyMemberId, mapping, s);
        zeroed += s.UInt16(null, 0);
        s.Assert(zeroed == 0, "Expected fields 4,5,8 to be 0");
        return e;
    }

    [EventPart("id")] public PartyMemberId PartyMemberId { get; private set; } // stored as ushort
    [EventPart("unk2", true, (byte)3)] public byte Unk2 { get; private set; }
    [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
    public override ModifyType SubType => ModifyType.Leader;
}
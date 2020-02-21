using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SetPartyLeaderEvent : ModifyEvent
    {
        public static SetPartyLeaderEvent Translate(SetPartyLeaderEvent e, ISerializer s)
        {
            e ??= new SetPartyLeaderEvent();
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.UInt16(nameof(PartyMemberId),
                () => (ushort)e.PartyMemberId,
                x => e.PartyMemberId = (PartyCharacterId)x);
            s.Dynamic(e, nameof(Unk8));
            return e;
        }

        public byte Unk2 { get; private set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public PartyCharacterId PartyMemberId { get; private set; } // stored as ushort
        public ushort Unk8 { get; set; }
        public override string ToString() => $"set_party_leader {PartyMemberId} ({Unk2} {Unk3} {Unk4} {Unk5} {Unk8})";
        public override ModifyType SubType => ModifyType.SetPartyLeader;
    }
}

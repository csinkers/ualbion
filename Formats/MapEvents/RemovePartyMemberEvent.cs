using System.Diagnostics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class RemovePartyMemberEvent : IMapEvent
    {
        public static RemovePartyMemberEvent Serdes(RemovePartyMemberEvent e, ISerializer s)
        {
            e ??= new RemovePartyMemberEvent();
            e.PartyMemberId = s.EnumU8(nameof(PartyMemberId), e.PartyMemberId);
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(Unk6));
            s.Dynamic(e, nameof(Unk8));
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk8 == 0);
            return e;
        }

        public RemovePartyMemberEvent(PartyCharacterId partyMemberId) { PartyMemberId = partyMemberId;}
        RemovePartyMemberEvent() { }

        public PartyCharacterId PartyMemberId { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"remove_party_member ({PartyMemberId} {Unk2} {Unk3} {Unk6})";
        public MapEventType EventType => MapEventType.RemovePartyMember;
    }
}

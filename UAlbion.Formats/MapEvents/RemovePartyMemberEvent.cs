using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class RemovePartyMemberEvent : MapEvent
    {
        public RemovePartyMemberEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.RemovePartyMember;
    }
}
using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class CreateTransportEvent : MapEvent
    {
        public CreateTransportEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            throw new NotImplementedException();
        }
    }
}
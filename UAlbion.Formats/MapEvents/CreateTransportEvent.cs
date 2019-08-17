using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class CreateTransportEvent : MapEvent
    {
        public CreateTransportEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.CreateTransport;
    }
}
using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class OffsetEvent : MapEvent
    {
        public OffsetEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Offset;
    }
}
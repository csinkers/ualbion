using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class DoorEvent : MapEvent
    {
        public DoorEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Door;
    }
}
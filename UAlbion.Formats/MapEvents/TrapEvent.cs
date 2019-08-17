using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class TrapEvent : MapEvent
    {
        public TrapEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Trap;
    }
}
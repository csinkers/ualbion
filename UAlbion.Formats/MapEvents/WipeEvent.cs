using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class WipeEvent : MapEvent
    {
        public WipeEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Wipe;
    }
}
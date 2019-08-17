using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class PauseEvent : MapEvent
    {
        public PauseEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Pause;
    }
}
using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SignalEvent : MapEvent
    {
        public SignalEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Signal;
    }
}
using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SpinnerEvent : MapEvent
    {
        public SpinnerEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Spinner;
    }
}
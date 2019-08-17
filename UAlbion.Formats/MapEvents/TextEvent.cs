using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class TextEvent : MapEvent
    {
        public TextEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Text;
    }
}
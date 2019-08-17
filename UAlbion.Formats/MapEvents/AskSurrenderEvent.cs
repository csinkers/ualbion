using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class AskSurrenderEvent : MapEvent
    {
        public AskSurrenderEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.AskSurrender;
    }
}
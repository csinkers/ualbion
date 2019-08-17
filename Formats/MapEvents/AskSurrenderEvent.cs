using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class AskSurrenderEvent : MapEvent
    {
        public AskSurrenderEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            throw new NotImplementedException();
        }
    }
}
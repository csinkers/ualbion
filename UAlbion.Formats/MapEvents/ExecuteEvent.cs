using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class ExecuteEvent : MapEvent
    {
        public ExecuteEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Execute;
    }
}
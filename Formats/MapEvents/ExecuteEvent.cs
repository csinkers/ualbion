using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class ExecuteEvent : MapEvent
    {
        public ExecuteEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            throw new NotImplementedException();
        }
    }
}
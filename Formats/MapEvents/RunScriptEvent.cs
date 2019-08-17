using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class RunScriptEvent : MapEvent
    {
        public RunScriptEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class RunScriptEvent : MapEvent
    {
        public RunScriptEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Script;
    }
}
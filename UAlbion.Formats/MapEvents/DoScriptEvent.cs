using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class DoScriptEvent : MapEvent
    {
        public DoScriptEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.DoScript;
    }
}
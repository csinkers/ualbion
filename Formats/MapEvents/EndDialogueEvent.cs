using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class EndDialogueEvent : MapEvent
    {
        public EndDialogueEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            throw new NotImplementedException();
        }
    }
}
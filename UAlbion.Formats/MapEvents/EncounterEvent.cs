using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class EncounterEvent : MapEvent
    {
        public EncounterEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Encounter;
    }
}
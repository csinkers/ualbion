using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class EncounterEvent : MapEvent
    {
        public EncounterEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            throw new NotImplementedException();
        }
    }
}
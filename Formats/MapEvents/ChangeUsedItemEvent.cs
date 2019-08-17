using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class ChangeUsedItemEvent : MapEvent
    {
        public ChangeUsedItemEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            throw new NotImplementedException();
        }
    }
}
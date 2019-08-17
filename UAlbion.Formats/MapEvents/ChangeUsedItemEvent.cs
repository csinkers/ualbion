using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class ChangeUsedItemEvent : MapEvent
    {
        public ChangeUsedItemEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.ChangeUsedItem;
    }
}
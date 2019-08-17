using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SimpleChestEvent : MapEvent
    {
        public SimpleChestEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.SimpleChest;
    }
}
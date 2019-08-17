using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class PlaceActionEvent : MapEvent
    {
        public PlaceActionEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.PlaceAction;
    }
}
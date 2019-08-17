using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class ActionEvent : MapEvent
    {
        public ActionEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Action;
    }
}
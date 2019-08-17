using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class ActionEvent : MapEvent
    {
        public ActionEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            throw new NotImplementedException();
        }
    }
}
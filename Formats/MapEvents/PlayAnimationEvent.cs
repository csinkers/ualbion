using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class PlayAnimationEvent : MapEvent
    {
        public PlayAnimationEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            throw new NotImplementedException();
        }
    }
}
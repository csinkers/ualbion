using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class PlayAnimationEvent : MapEvent
    {
        public PlayAnimationEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.PlayAnimation;
    }
}
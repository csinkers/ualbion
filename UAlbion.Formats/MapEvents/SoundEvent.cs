using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SoundEvent : MapEvent
    {
        public SoundEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.Sound;
    }
}
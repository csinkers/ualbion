using System;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class StartDialogueEvent : MapEvent
    {
        public StartDialogueEvent(BinaryReader br, int id)
        {
            throw new NotImplementedException();
        }

        public override EventType Type => EventType.StartDialogue;
    }
}
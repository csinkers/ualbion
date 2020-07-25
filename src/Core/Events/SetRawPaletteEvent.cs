using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class SetRawPaletteEvent : EngineEvent, IVerboseEvent
    {
        public string Name { get; }
        public uint[] Entries { get; }

        public SetRawPaletteEvent(string name, uint[] entries)
        {
            Name = name;
            Entries = entries;
        }
    }
}

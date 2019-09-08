using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("load_pal")]
    public class LoadPaletteEvent : GameEvent
    {
        public LoadPaletteEvent(int paletteId) { PaletteId = paletteId; }
        [EventPart("paletteId")] public int PaletteId { get; }
    }
}
using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("load_pal")]
    public class LoadPalEvent : GameEvent
    {
        public LoadPalEvent(int paletteId) { PaletteId = paletteId; }
        [EventPart("paletteId")] public int PaletteId { get; }
    }
}
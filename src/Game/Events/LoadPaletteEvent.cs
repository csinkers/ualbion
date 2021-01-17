using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("palette", "Set the current palette", "load_pal")]
    public class LoadPaletteEvent : GameEvent
    {
        public LoadPaletteEvent(PaletteId paletteId) { PaletteId = paletteId; }
        [EventPart("paletteId")] public PaletteId PaletteId { get; }
    }
}

using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("load_pal")]
    public class LoadPaletteEvent : GameEvent
    {
        public LoadPaletteEvent(PaletteId paletteId) { PaletteId = paletteId; }
        [EventPart("paletteId")] public PaletteId PaletteId { get; }
    }
}

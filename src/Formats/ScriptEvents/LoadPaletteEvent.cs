using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.ScriptEvents
{
    [Event("load_pal", "Set the current palette", "palette")] // USED IN SCRIPT (as load_paL)
    public class LoadPaletteEvent : Event
    {
        public LoadPaletteEvent(PaletteId paletteId) { PaletteId = paletteId; }
        [EventPart("paletteId")] public PaletteId PaletteId { get; }
    }
}

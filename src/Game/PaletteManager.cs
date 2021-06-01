using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.ScriptEvents;

namespace UAlbion.Game
{
    public class PaletteManager : ServiceComponent<IPaletteManager>, IPaletteManager
    {
        public IPalette Palette { get; private set; }
        public IReadOnlyTexture<uint> PaletteTexture { get; private set; }
        public int Version { get; private set; }
        public int Frame { get; private set; }

        public PaletteManager()
        {
            On<SlowClockEvent>(e => OnTick(e.Delta));
            On<LoadPaletteEvent>(e => SetPalette(e.PaletteId));
            On<LoadRawPaletteEvent>(e =>
            {
                Palette = null;
                GeneratePalette(PaletteId.None, e.Name, e.Entries);
            });
        }

        protected override void Subscribed()
        {
            base.Subscribed();
            if (PaletteTexture == null)
                SetPalette(Base.Palette.Toronto2D);
        }

        void OnTick(int frames)
        {
            if (Palette == null || !Palette.IsAnimated)
                return;

            Frame += frames;
            while (Frame >= Palette.GetCompletePalette().Count)
                Frame -= Palette.GetCompletePalette().Count;

            GeneratePalette(PaletteId.FromUInt32(Palette.Id), Palette.Name, Palette.GetPaletteAtTime(Frame));
        }

        void SetPalette(PaletteId paletteId)
        {
            var palette = Resolve<IAssetManager>().LoadPalette(paletteId);
            if (palette == null)
            {
                Error($"Palette ID {paletteId} could not be loaded!");
                return;
            }

            Palette = palette;
            while (Frame >= Palette.GetCompletePalette().Count)
                Frame -= Palette.GetCompletePalette().Count;

            GeneratePalette(paletteId, Palette.Name, Palette.GetPaletteAtTime(Frame));
        }

        void GeneratePalette(PaletteId id, string name, uint[] rawPalette)
        {
            PaletteTexture = new PaletteTexture(id, name, rawPalette);
            Version++;
        }
    }
}

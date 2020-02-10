using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class PaletteManager : Component, IPaletteManager
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<PaletteManager, SlowClockEvent>((x, e) => x.OnTick(e.Delta)),
            H<PaletteManager, LoadPaletteEvent>((x, e) => x.SetPalette(e.PaletteId))
        );

        public IPalette Palette { get; private set; }
        public PaletteTexture PaletteTexture { get; private set; }
        public int PaletteFrame { get; private set; }

        public PaletteManager() : base(Handlers) { }

        public override void Subscribed()
        {
            SetPalette(PaletteId.Toronto2D);
            base.Subscribed();
        }

        void OnTick(int frames)
        {
            PaletteFrame += frames;
            while (PaletteFrame >= Palette.GetCompletePalette().Count)
                PaletteFrame -= Palette.GetCompletePalette().Count;

            if (Palette.IsAnimated)
                GeneratePalette();
        }

        void SetPalette(PaletteId paletteId)
        {
            var palette = Resolve<IAssetManager>().LoadPalette(paletteId);
            if (palette == null)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Palette ID {paletteId} could not be loaded!"));
                return;
            }

            Palette = palette;
            while (PaletteFrame >= Palette.GetCompletePalette().Count)
                PaletteFrame -= Palette.GetCompletePalette().Count;

            GeneratePalette();
        }

        void GeneratePalette()
        {
            PaletteTexture = new PaletteTexture(Palette.Name, Palette.GetPaletteAtTime(PaletteFrame));
        }
    }
}

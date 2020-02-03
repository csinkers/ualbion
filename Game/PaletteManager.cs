using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class PaletteManager : Component, IPaletteManager
    {
        const int TicksPerPaletteChange = 8;
        static readonly HandlerSet Handlers = new HandlerSet(
            H<PaletteManager, UpdateEvent>((x, e) => x.OnTick(e.Frames)),
            H<PaletteManager, LoadPaletteEvent>((x, e) => x.SetPalette(e.PaletteId))
        );

        int _ticks;

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
            _ticks += frames;
            while (_ticks >= TicksPerPaletteChange)
            {
                _ticks -= TicksPerPaletteChange;
                PaletteFrame++;
                if (PaletteFrame >= Palette.GetCompletePalette().Count)
                    PaletteFrame = 0;

                if (Palette.IsAnimated)
                    GeneratePalette();
            }
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
            if (PaletteFrame >= Palette.GetCompletePalette().Count)
                PaletteFrame = 0;

            GeneratePalette();
        }

        void GeneratePalette()
        {
            PaletteTexture = new PaletteTexture(Palette.Name, Palette.GetPaletteAtTime(PaletteFrame));
        }
    }
}

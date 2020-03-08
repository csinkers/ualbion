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
        public int Version { get; private set; }
        public int Frame { get; private set; }

        public PaletteManager() : base(Handlers) { }

        public override void Subscribed()
        {
            SetPalette(PaletteId.Toronto2D);
            base.Subscribed();
        }

        void OnTick(int frames)
        {
            if (!Palette.IsAnimated)
                return;

            Frame += frames;
            while (Frame >= Palette.GetCompletePalette().Count)
                Frame -= Palette.GetCompletePalette().Count;

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
            while (Frame >= Palette.GetCompletePalette().Count)
                Frame -= Palette.GetCompletePalette().Count;

            GeneratePalette();
        }

        void GeneratePalette()
        {
            var factory = Resolve<ICoreFactory>();
            PaletteTexture = factory.CreatePaletteTexture(Palette.Name, Palette.GetPaletteAtTime(Frame));
            Version++;
        }
    }
}

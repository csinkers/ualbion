using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class PaletteManager : Component, IPaletteManager
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<PaletteManager, UpdateEvent>((x, e) =>
            {
                x._ticks++;
                if(x._logicalPalette.IsAnimated)
                    x.GeneratePalette();
            }),
            H<PaletteManager, LoadPaletteEvent>((x, e) => x.SetPalette(e.PaletteId))
        );

        AlbionPalette _logicalPalette;
        public Palette Palette { get; private set; }
        int _ticks;

        public PaletteManager() : base(Handlers) { }

        protected override void Subscribed()
        {
            SetPalette(PaletteId.Toronto2D);
            base.Subscribed();
        }

        void SetPalette(PaletteId paletteId)
        {
            var palette = Resolve<IAssetManager>().LoadPalette(paletteId);
            if (palette == null)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Palette ID {paletteId} could not be loaded!"));
                return;
            }

            _logicalPalette = palette;
            GeneratePalette();
        }

        void GeneratePalette()
        {
            Palette = new Palette(_logicalPalette.Name, _logicalPalette.GetPaletteAtTime(_ticks));
        }
    }
}
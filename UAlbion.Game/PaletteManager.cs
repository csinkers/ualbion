using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Game.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class PaletteManager : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<PaletteManager, UpdateEvent>((x, e) =>
            {
                x._ticks++;
                if(x._palette.IsAnimated)
                    x.EmitPalette();
            }),
            new Handler<PaletteManager, SubscribedEvent>((x, e) => x.SetPalette(PaletteId.Main3D)),
            new Handler<PaletteManager, LoadPalEvent>((x, e) => x.SetPalette((PaletteId)e.PaletteId))
        };

        readonly Assets _assets;
        AlbionPalette _palette;
        int _ticks;

        public PaletteManager(Assets assets) : base(Handlers)
        {
            _assets = assets;
        }

        void SetPalette(PaletteId paletteId)
        {
            var palette = _assets.LoadPalette(paletteId);
            if (palette == null)
            {
                Raise(new LogEvent((int) LogEvent.Level.Error, $"Palette ID {paletteId} could not be loaded!"));
                return;
            }

            _palette = palette;
            EmitPalette();
        }

        void EmitPalette()
        {
            Exchange.Raise(new SetRawPaletteEvent(_palette.Name, _palette.GetPaletteAtTime(_ticks)), this);
        }
    }
}
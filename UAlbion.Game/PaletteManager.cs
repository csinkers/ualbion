using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Parsers;
using UAlbion.Game.AssetIds;

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
                    x._scene.SetPalette(x._palette.Name, x._palette.GetPaletteAtTime(x._ticks));
            }),
            new Handler<PaletteManager, LoadPalEvent>((x, e) =>
            {
                var palette = x._assets.LoadPalette((PaletteId) e.PaletteId);
                if(palette != null)
                    x.SetPalette(palette);
            })
        };

        readonly Scene _scene;
        readonly Assets _assets;
        AlbionPalette _palette;
        int _ticks;

        public PaletteManager(Scene scene, Assets assets) : base(Handlers)
        {
            _scene = scene;
            _assets = assets;
        }

        public void SetPalette(AlbionPalette palette)
        {
            _palette = palette ?? throw new ArgumentNullException(nameof(palette));
            _scene.SetPalette(_palette.Name, _palette.GetPaletteAtTime(_ticks));
        }
    }
}
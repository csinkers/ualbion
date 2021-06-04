using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class PaletteManager : ServiceComponent<IPaletteManager>, IPaletteManager
    {
        IPalette _nightPalette;
        public IPalette Palette { get; private set; }
        public IReadOnlyTexture<uint> PaletteTexture { get; private set; }
        public int Version { get; private set; }
        public int Frame { get; private set; }
        public float PaletteBlend => TryResolve<IGameState>()?.PaletteBlend ?? 0;

        public PaletteManager()
        {
            On<SlowClockEvent>(e => OnTick(e.Delta));
            On<LoadPaletteEvent>(e => SetPalette(e.PaletteId));
            On<LoadRawPaletteEvent>(e =>
            {
                Palette = null;
                GeneratePalette(PaletteId.None, e.Name, e.Entries, null);
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
            //while (Frame >= Palette.GetCompletePalette().Count)
            //    Frame -= Palette.GetCompletePalette().Count;

            GeneratePalette(
                PaletteId.FromUInt32(Palette.Id),
                Palette.Name,
                Palette.GetPaletteAtTime(Frame),
                _nightPalette?.GetPaletteAtTime(Frame));
        }

        void SetPalette(PaletteId paletteId)
        {
            var assets = Resolve<IAssetManager>();
            var palette = assets.LoadPalette(paletteId);
            if (palette == null)
            {
                Error($"Palette ID {paletteId} could not be loaded!");
                return;
            }

            Palette = palette;
            _nightPalette = NightPalettes.TryGetValue(paletteId, out var nightPaletteId) 
                ? assets.LoadPalette(nightPaletteId) 
                : null;

            //while (Frame >= Palette.GetCompletePalette().Count)
            //    Frame -= Palette.GetCompletePalette().Count;

            GeneratePalette(paletteId, Palette.Name, Palette.GetPaletteAtTime(Frame), _nightPalette?.GetPaletteAtTime(Frame));
        }

        void GeneratePalette(PaletteId id, string name, uint[] dayPalette, uint[] nightPalette)
        {
            var blendedPalette = dayPalette;
            var state = TryResolve<IGameState>();
            if (state != null && nightPalette != null)
            {
                blendedPalette = new uint[256];
                for (int i = 0; i < 256; i++)
                {
                    var (dr, dg, db, da) = ApiUtil.UnpackColor(dayPalette[i]);
                    var (nr, ng, nb, na) = ApiUtil.UnpackColor(nightPalette[i]);
                    var br = (byte)ApiUtil.Lerp(dr, nr, state.PaletteBlend);
                    var bg = (byte)ApiUtil.Lerp(dg, ng, state.PaletteBlend);
                    var bb = (byte)ApiUtil.Lerp(db, nb, state.PaletteBlend);
                    var ba = (byte)ApiUtil.Lerp(da, na, state.PaletteBlend);
                    blendedPalette[i] = ApiUtil.PackColor(br, bg, bb, ba);
                }
            }

            PaletteTexture = new PaletteTexture(id, name, blendedPalette);
            Version++;
            Raise(new PaletteChangedEvent());
        }
    }
}

using System;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.State;

namespace UAlbion.Game;

public class PaletteManager : ServiceComponent<IPaletteManager>, IPaletteManager
{
    public IPalette Day { get; private set; }
    public IPalette Night { get; private set; }
    public int Frame { get; private set; }
    public int Version { get; private set; }
    public float Blend
    {
        get
        {
            var state = TryResolve<IGameState>();
            if (state == null || Night == null)
                return 0;

            return MathF.Cos((float)state.Time.TimeOfDay.TotalDays * MathF.PI * 2 * 60) * 0.5f + 0.5f;
        }
    }

    public PaletteManager()
    {
        On<LoadPaletteEvent>(e => SetPalette(e.PaletteId));
        On<SlowClockEvent>(e => Frame += e.Delta);
        On<LoadRawPaletteEvent>(e =>
        {
            Day = new AlbionPalette(0, "Raw", e.Entries);
            Night = null;
            Version++;
        });
    }

    protected override void Subscribed()
    {
        base.Subscribed();
        if (Day == null)
            SetPalette(Base.Palette.Common);
    }

    void SetPalette(PaletteId paletteId)
    {
        var assets = Resolve<IAssetManager>();
        var day = assets.LoadPalette(paletteId);
        if (day == null)
        {
            Error($"Palette ID {paletteId} could not be loaded!");
            return;
        }

        Day = day;
        Night = NightPalettes.TryGetValue(paletteId, out var nightPaletteId)
            ? assets.LoadPalette(nightPaletteId)
            : null;
        Version++;
    }
}
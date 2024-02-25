using System;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.State;

namespace UAlbion.Game;

public class PaletteManager : GameServiceComponent<IPaletteManager>, IPaletteManager
{
    public IPalette Day { get; private set; }
    public IPalette Night { get; private set; }
    public int Frame { get; private set; }
    public float Blend
    {
        get
        {
            var state = TryResolve<IGameState>();
            if (state == null || Night == null)
                return 0;

            var daysElapsed = (float)state.Time.TimeOfDay.TotalDays; // how far through the day we are (0..1)
            return MathF.Cos(daysElapsed * MathF.PI * 2) * 0.5f + 0.5f;
        }
    }

    public PaletteManager()
    {
        On<LoadPaletteEvent>(e => SetPalette(e.PaletteId));
        On<SlowClockEvent>(_ => Frame++);
        On<LoadRawPaletteEvent>(e =>
        {
            Day = new AlbionPalette(0, "Raw", e.Entries);
            Night = null;
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
        var day = Assets.LoadPalette(paletteId);
        if (day == null)
        {
            Error($"Palette ID {paletteId} could not be loaded!");
            return;
        }

        Day = day;
        Night = NightPalettes.TryGetValue(paletteId, out var nightPaletteId)
            ? Assets.LoadPalette(nightPaletteId)
            : null;

        if (Night != null)
        {
            ApiUtil.Assert(Day.AnimatedEntries.SequenceEqual(Night.AnimatedEntries),
                "Expected day and night palettes to have identical animated entries!" +
                $" Day palette {Day.Id} had entries [ {string.Join(", ", Day.AnimatedEntries)} ] and " +
                $"Night palette {Night.Id} had entries [ {string.Join(", ", Night.AnimatedEntries)} ]");
        }
    }
}

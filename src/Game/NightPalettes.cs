using System.Collections.Generic;
using System.Threading;
using UAlbion.Formats.Ids;

namespace UAlbion.Game;

public static class NightPalettes // TODO: Load from config / asset instead
{
    static readonly Lock SyncRoot = new();
    static readonly Dictionary<PaletteId, PaletteId> Mapping = [];

    public static bool TryGetValue(PaletteId dayPaletteId, out PaletteId nightPaletteId)
    {
        lock (SyncRoot)
            return Mapping.TryGetValue(dayPaletteId, out nightPaletteId);
    }

    public static void Map(PaletteId dayPaletteId, PaletteId nightPaletteId)
    {
        lock (SyncRoot)
            Mapping[dayPaletteId] = nightPaletteId;
    }

    static NightPalettes()
    { // Add base palettes
        Map(Base.Palette.JirinaarDay, Base.Palette.JirinaarNight);
        Map(Base.Palette.BelovenoDay, Base.Palette.BelovenoNight);
        Map(Base.Palette.DesertDay, Base.Palette.DesertNight);
        Map(Base.Palette.FirstIslandDay, Base.Palette.OutdoorsNight);
        Map(Base.Palette.SecondIslandDay, Base.Palette.OutdoorsNight);
        Map(Base.Palette.DesertCombat, Base.Palette.DesertNightCombat);
        Map(Base.Palette.ForestCombat, Base.Palette.ForestNightCombat);
        Map(Base.Palette.TownCombat, Base.Palette.TownNightCombat);
    }
}
using System;

namespace UAlbion.Formats.MapEvents
{
    [Flags]
    public enum EventScopes : byte
    {
        Rel = 1, // Relative positioning (vs. absolute)
        Temp = 2, // Temporary lifetime (i.e. doesn't survive map reload)
    }
}

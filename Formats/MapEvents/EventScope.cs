using System;

namespace UAlbion.Formats.MapEvents
{
    [Flags]
    public enum EventScope : byte
    {
        RelativePositioning = 1,
        Temporary = 2,
    }
}
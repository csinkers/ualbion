using System;

namespace UAlbion.Formats.MapEvents;

[Flags]
public enum ChangeIconLayers : byte
{
    None = 0,
    Underlay = 1,
    Overlay = 2,
}
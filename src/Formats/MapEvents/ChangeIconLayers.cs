using System;

namespace UAlbion.Formats.MapEvents;

[Flags]
public enum ChangeIconLayers : byte
{
    Underlay = 1,
    Overlay = 2,
}
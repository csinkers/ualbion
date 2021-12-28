using System;

namespace UAlbion;

[Flags]
public enum DumpFormats
{
    Json = 1 << 0, // Machine & human-readable, good for inter-operation and modding
    Text = 1 << 1, // Dense ad-hoc text format, good for an overview.
    Png  = 1 << 2, // True-colour PNG with colours resolved at palette tick 0
    // Tiled = 1 << 4, // Dump maps and tilemaps in Tiled tsx and tmx format
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UAlbion.Api.Visual;

public interface IPalette
{
    uint Id { get; }
    string Name { get; }
    bool IsAnimated { get; }
    [JsonIgnore] IReadOnlyTexture<uint> Texture { get; } // Width=256 Height=Cycle length (LCM of all cycles)
    IEnumerable<(byte, int)> AnimatedEntries { get; }
}
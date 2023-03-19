using UAlbion.Api.Visual;

namespace UiTest;

public class SimplePalette : IPalette
{
    public SimplePalette(uint id, string name, IReadOnlyTexture<uint> texture)
    {
        Id = id;
        Name = name;
        Texture = texture;
    }

    public uint Id { get; }
    public string Name { get; }
    public bool IsAnimated => false;
    public IReadOnlyTexture<uint> Texture { get; }
    public IEnumerable<(byte, int)> AnimatedEntries => Enumerable.Empty<(byte, int)>();
}
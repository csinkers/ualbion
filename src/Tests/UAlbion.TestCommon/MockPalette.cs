using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Visual;

namespace UAlbion.TestCommon;

public class MockPalette : IPalette
{
    readonly uint[] _entries = Enumerable.Range(0, 256).Select(x =>
        (uint)x |
        (uint)x << 8 |
        (uint)x << 16 |
        (uint)(x == 0 ? 0 : 0xff) << 24
    ).ToArray();

    public MockPalette()
    {
        var texture = new SimpleTexture<uint>(null, 256, 1);
        texture.AddRegion(0, 0, 256, 1);
        var span = texture.GetMutableLayerBuffer(0).Buffer;
        _entries.AsSpan().CopyTo(span);
        Texture = texture;
    }

    public uint Id => 0;
    public string Name => "Mock";
    public bool IsAnimated => false;
    public IReadOnlyTexture<uint> Texture { get; }
    public IEnumerable<(byte, int)> AnimatedEntries => Array.Empty<(byte, int)>();
}
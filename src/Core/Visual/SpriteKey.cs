using System;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

public readonly struct SpriteKey : IEquatable<SpriteKey>, IComparable<SpriteKey>, IBatchKey
{
    public SpriteKey(ITexture texture, SpriteSampler sampler, DrawLayer renderOrder, SpriteKeyFlags flags, Rectangle? scissorRegion = null)
    {
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        Sampler = sampler;
        RenderOrder = renderOrder;
        Flags = 
            (flags & ~(SpriteKeyFlags.UseArrayTexture | SpriteKeyFlags.UsePalette)) | 
            (texture.ArrayLayers > 1 ? SpriteKeyFlags.UseArrayTexture : 0) |
            (texture is IReadOnlyTexture<byte> ? SpriteKeyFlags.UsePalette : 0);

        ScissorRegion = scissorRegion;
    }

    public ITexture Texture { get; }
    public SpriteSampler Sampler { get; }
    public DrawLayer RenderOrder { get; }
    public SpriteKeyFlags Flags { get; }
    public Rectangle? ScissorRegion { get; } // UI coordinates

    public bool Equals(SpriteKey other) => 
        Equals(Texture, other.Texture) && 
        Equals(Sampler, other.Sampler) && 
        RenderOrder == other.RenderOrder && 
        Flags == other.Flags &&
        ScissorRegion == other.ScissorRegion;

    public override bool Equals(object obj) => obj is SpriteKey other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Texture, Sampler, RenderOrder, (int)Flags, ScissorRegion);
    public override string ToString() => $"{Texture.Name} Flags:{Flags}";
    public static bool operator ==(SpriteKey a, SpriteKey b) => Equals(a, b);
    public static bool operator !=(SpriteKey a, SpriteKey b) => !(a == b);
    public static bool operator >(SpriteKey a, SpriteKey b) => a.CompareTo(b) > 0;
    public static bool operator <(SpriteKey a, SpriteKey b) => a.CompareTo(b) < 0;
    public static bool operator >=(SpriteKey a, SpriteKey b) => a.CompareTo(b) <= 0;
    public static bool operator <=(SpriteKey a, SpriteKey b) => a.CompareTo(b) >= 0;

    public int CompareTo(SpriteKey other)
    {
        var samplerComparison = Sampler.CompareTo(other.Sampler);
        if (samplerComparison != 0) return samplerComparison;
        var orderComparison = RenderOrder.CompareTo(other.RenderOrder);
        if (orderComparison != 0) return orderComparison;
        var flagsComparison = Flags.CompareTo(other.Flags);
        if (flagsComparison != 0) return flagsComparison;
        return Nullable.Compare(ScissorRegion, other.ScissorRegion);
    }
}
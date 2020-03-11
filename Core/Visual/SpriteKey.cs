using System;
using UAlbion.Api;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public struct SpriteKey : IEquatable<SpriteKey>
    {
        public SpriteKey(ITexture texture, DrawLayer renderOrder, SpriteKeyFlags flags)
        {
            Texture = texture;
            RenderOrder = renderOrder;
            Flags = flags;
        }

        public ITexture Texture { get; }
        public DrawLayer RenderOrder { get; }
        public SpriteKeyFlags Flags { get; }

        public bool Equals(SpriteKey other) => Equals(Texture, other.Texture) && RenderOrder == other.RenderOrder && Flags == other.Flags;
        public override bool Equals(object obj) => obj is SpriteKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Texture, RenderOrder, (int)Flags);
        public static bool operator ==(SpriteKey a, SpriteKey b) => Equals(a, b);
        public static bool operator !=(SpriteKey a, SpriteKey b) => !(a == b);
    }
}

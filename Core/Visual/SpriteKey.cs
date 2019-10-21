using System;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public struct SpriteKey : IEquatable<SpriteKey>
    {
        public SpriteKey(ITexture texture, int renderOrder, SpriteFlags flags)
        {
            Texture = texture;
            RenderOrder = renderOrder;
            Flags = flags & (SpriteFlags)SpriteFlagMask.SpriteKey;
        }
        public ITexture Texture { get; }
        public int RenderOrder { get; }
        public SpriteFlags Flags { get; }

        public bool Equals(SpriteKey other) =>
            Equals(Texture, other.Texture) && 
            RenderOrder == other.RenderOrder && 
            Flags == other.Flags;

        public override bool Equals(object obj) => obj is SpriteKey other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Texture != null ? Texture.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RenderOrder;
                hashCode = (hashCode * 397) ^ Flags.GetHashCode();
                return hashCode;
            }
        }
    }
}
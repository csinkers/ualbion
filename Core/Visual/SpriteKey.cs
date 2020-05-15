using System;
using UAlbion.Api;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public struct SpriteKey : IEquatable<SpriteKey>
    {
        public SpriteKey(ITexture texture, DrawLayer renderOrder, SpriteKeyFlags flags, Rectangle? scissorRegion = null)
        {
            Texture = texture;
            RenderOrder = renderOrder;
            Flags = flags;
            ScissorRegion = scissorRegion;
        }

        public ITexture Texture { get; }
        public DrawLayer RenderOrder { get; }
        public SpriteKeyFlags Flags { get; }
        public Rectangle? ScissorRegion { get; } // UI coordinates

        public bool Equals(SpriteKey other) => 
            Equals(Texture, other.Texture) && 
            RenderOrder == other.RenderOrder && 
            Flags == other.Flags &&
            ScissorRegion == other.ScissorRegion;

        public override bool Equals(object obj) => obj is SpriteKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Texture, RenderOrder, (int)Flags, ScissorRegion);
        public static bool operator ==(SpriteKey a, SpriteKey b) => Equals(a, b);
        public static bool operator !=(SpriteKey a, SpriteKey b) => !(a == b);
    }
}

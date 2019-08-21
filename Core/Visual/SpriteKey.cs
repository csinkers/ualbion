using System;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public struct SpriteKey : IEquatable<SpriteKey>
    {
        public SpriteKey(ITexture texture, int renderOrder) { Texture = texture; RenderOrder = renderOrder; }
        public ITexture Texture { get; }
        public int RenderOrder { get; }
        public bool Equals(SpriteKey other) => Equals(Texture, other.Texture) && RenderOrder == other.RenderOrder;
        public override bool Equals(object obj) => obj is SpriteKey other && Equals(other);
        public override int GetHashCode() { unchecked { return ((Texture != null ? Texture.GetHashCode() : 0) * 397) ^ RenderOrder; } }
    }
}
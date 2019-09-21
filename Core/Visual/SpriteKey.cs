using System;
using UAlbion.Core.Textures;

namespace UAlbion.Core.Visual
{
    public struct SpriteKey : IEquatable<SpriteKey>
    {
        public SpriteKey(ITexture texture, int renderOrder, bool depthTested)
        {
            Texture = texture;
            RenderOrder = renderOrder;
            DepthTested = depthTested;
        }
        public ITexture Texture { get; }
        public int RenderOrder { get; }
        public bool DepthTested { get; }

        public bool Equals(SpriteKey other) =>
            Equals(Texture, other.Texture) && 
            RenderOrder == other.RenderOrder && 
            DepthTested == other.DepthTested;

        public override bool Equals(object obj) => obj is SpriteKey other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Texture != null ? Texture.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RenderOrder;
                hashCode = (hashCode * 397) ^ DepthTested.GetHashCode();
                return hashCode;
            }
        }
    }
}
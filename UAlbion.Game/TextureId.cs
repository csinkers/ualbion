using System;
using UAlbion.Core.Objects;

namespace UAlbion.Game
{
    public struct TextureId : ITextureId, IEquatable<TextureId>
    {
        public TextureId(AssetType assetType, int assetId, int frame)
        {
            AssetType = assetType;
            AssetId = assetId;
            Frame = frame;
        }

        public AssetType AssetType { get; }
        public int AssetId { get; }
        public int Frame { get; set; }

        public bool Equals(TextureId other) { return AssetType == other.AssetType && AssetId == other.AssetId && Frame == other.Frame; }
        public override bool Equals(object obj) { return obj is TextureId other && Equals(other); }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) AssetType;
                hashCode = (hashCode * 397) ^ AssetId;
                hashCode = (hashCode * 397) ^ Frame;
                return hashCode;
            }
        }
    }
} 
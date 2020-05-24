using System;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public struct AssetKey : IEquatable<AssetKey>
    {
        public AssetKey(AssetId id, GameLanguage language = GameLanguage.English)
        {
            AssetId = id;
            Language = language;
        }

        public AssetKey(AssetType type, ushort id = 0, GameLanguage language = GameLanguage.English)
        {
            AssetId = new AssetId(type, id);
            Language = language;
        }

        public AssetId AssetId { get; }
        public AssetType Type => AssetId.Type;
        public int Id => AssetId.Id;
        public GameLanguage Language { get; }

        public bool Equals(AssetKey other) => AssetId.Equals(other.AssetId) && Language == other.Language;
        public override bool Equals(object obj) => obj is AssetKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(AssetId, Language);
    }
}

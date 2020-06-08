using System;

namespace UAlbion.Formats.AssetIds
{
    public struct AssetKey : IEquatable<AssetKey>
    {
        public AssetKey(AssetType type, ushort id = 0, GameLanguage language = GameLanguage.English)
        {
            AssetId = new AssetId(type, id);
            Language = language;
        }

        public static implicit operator AssetKey(AssetId id) => new AssetKey(id.Type, id.Id);

        public AssetId AssetId { get; }
        public AssetType Type => AssetId.Type;
        public int Id => AssetId.Id;
        public GameLanguage Language { get; }

        public override string ToString() => $"{AssetId}:{Language}";
        public static bool operator ==(AssetKey x, AssetKey y) => x.Equals(y);
        public static bool operator !=(AssetKey x, AssetKey y) => !(x == y);
        public bool Equals(AssetKey other) => AssetId.Equals(other.AssetId) && Language == other.Language;
        public override bool Equals(object obj) => obj is AssetKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Language;
                return hashCode;
            }
        } 
    }
}

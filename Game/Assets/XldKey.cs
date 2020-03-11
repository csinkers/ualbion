using System;
using UAlbion.Formats;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    struct XldKey : IEquatable<XldKey>
    {
        public XldKey(AssetKey key)
        {
            Type = key.Type;
            Number = key.Id / 100;
            Language = key.Language;
        }

        public AssetType Type { get; }
        public int Number { get; }
        public GameLanguage Language { get; }

        public bool Equals(XldKey other) => Type == other.Type && Number == other.Number && Language == other.Language;
        public override bool Equals(object obj) => obj is XldKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Type, Number, (int)Language);
    }
}

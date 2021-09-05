using System;
using System.Globalization;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    public struct InventoryId : IEquatable<InventoryId>
    {
        public InventoryId(AssetId id)
        {
            Type = id.Type switch
            {
                AssetType.Chest => InventoryType.Chest,
                AssetType.Merchant => InventoryType.Merchant,
                AssetType.PartyMember => InventoryType.Player,
                AssetType.Monster => InventoryType.Monster,
                _ => throw new ArgumentOutOfRangeException($"Tried to convert an asset of type {id.Type} (value {id.Id}) to an inventory id")
            };
            Id = (ushort)id.Id;
        }

        public InventoryId(InventoryType type, ushort id)
        {
            Type = type;
            Id = id;
        }

        public InventoryType Type { get; }
        public ushort Id { get; }

        public AssetId ToAssetId() =>
            Type switch
            {
                InventoryType.Player => new AssetId(AssetType.PartyMember, Id),
                InventoryType.Chest => new AssetId(AssetType.Chest, Id),
                InventoryType.Merchant => new AssetId(AssetType.Merchant, Id),
                InventoryType.Monster => new AssetId(AssetType.Monster, Id),
                _ => AssetId.None,
            };

        public override string ToString() => Type switch
        {
            InventoryType.CombatLoot => "CombatLoot",
            _ => ToAssetId().ToString()
        };

        public static InventoryId Parse(string s)
        {
            if (s == null)
                throw new FormatException($"Tried to parse an empty InventoryId");

            if (s.Equals("CombatLoot", StringComparison.OrdinalIgnoreCase))
                return new InventoryId(InventoryType.CombatLoot, 0);

            var assetId = AssetId.Parse(s);
            return new InventoryId(assetId);
        }

        public static explicit operator int(InventoryId id) => ToInt32(id);
        public static int ToInt32(InventoryId id) => (int)id.Type << 16 | id.Id;
        public static explicit operator InventoryId(int id) => ToInventoryId(id);
        public static InventoryId ToInventoryId(int id)
            => new(
                (InventoryType)((id & 0x7fff0000) >> 16),
                (ushort)(id & 0xffff));

        public static explicit operator InventoryId(AssetId id) => ToInventoryId(id);
        public static explicit operator InventoryId(PartyMemberId id) => ToInventoryId(id);
        public static explicit operator InventoryId(ChestId id) => ToInventoryId(id);
        public static explicit operator InventoryId(MerchantId id) => ToInventoryId(id);
        public static explicit operator AssetId(InventoryId id) => ToAssetId(id);
        public static InventoryId ToInventoryId(AssetId id) => new(id);
        public static AssetId ToAssetId(InventoryId id) => new(id.Type switch
        {
            InventoryType.Chest => AssetType.Chest,
            InventoryType.Merchant => AssetType.Merchant,
            InventoryType.Player => AssetType.PartyMember,
            InventoryType.Monster => AssetType.Monster,
            _ => throw new ArgumentOutOfRangeException($"Cannot convert inventory id of type {id.Type} to an AssetId")
        }, id.Id);

        public static bool operator ==(InventoryId x, InventoryId y) => x.Equals(y);
        public static bool operator !=(InventoryId x, InventoryId y) => !(x == y);
        public bool Equals(InventoryId other) => Type == other.Type && Id == other.Id;
        public override bool Equals(object obj) => obj is InventoryId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }
}

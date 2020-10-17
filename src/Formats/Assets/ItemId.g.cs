// Note: This file was automatically generated using Tools/GenerateEnums.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various types.
using System;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(ToStringJsonConverter))]
    public struct ItemId : IEquatable<ItemId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public ItemId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type >= AssetType.Gold && type <= AssetType.Item))
                throw new ArgumentOutOfRangeException($"Tried to construct a ItemId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a ItemId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public ItemId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type >= AssetType.Gold && Type <= AssetType.Item))
                throw new ArgumentOutOfRangeException($"Tried to construct a ItemId with a type of {Type}");
        }
        public ItemId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type >= AssetType.Gold && Type <= AssetType.Item))
                throw new ArgumentOutOfRangeException($"Tried to construct a ItemId with a type of {Type}");
        }

        public static ItemId From<T>(T id) where T : unmanaged, Enum => (ItemId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static ItemId FromDisk(AssetType type, int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            
            if (!(type == AssetType.None || type >= AssetType.Gold && type <= AssetType.Item))
                throw new ArgumentOutOfRangeException($"Tried to construct a ItemId with a type of {type}");

            var (enumType, enumValue) = mapping.IdToEnum(new ItemId(type, disk));
            return (ItemId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static ItemId SerdesU8(string name, ItemId id, AssetType type, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(type, diskValue, mapping);
        }

        public static ItemId SerdesU16(string name, ItemId id, AssetType type, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(type, diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static ItemId None => new ItemId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static ItemId Parse(string s)
        {
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            // if (s == null || !s.Contains(":"))
            //     throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            // var parts = s.Split(':');
            // //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            // var type = AssetTypeExtensions.FromShort(parts[0]);
            // var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            // return new ItemId(type, id);
        }

        public static implicit operator AssetId(ItemId id) => new AssetId(id._value);
        public static implicit operator ItemId(AssetId id) => new ItemId((uint)id);
        public static explicit operator uint(ItemId id) => id._value;
        public static explicit operator int(ItemId id) => unchecked((int)id._value);
        public static explicit operator ItemId(int id) => new ItemId(id);
        public static implicit operator ItemId(UAlbion.Base.Item id) => ItemId.From(id);

        public static ItemId ToItemId(int id) => new ItemId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(ItemId x, ItemId y) => x.Equals(y);
        public static bool operator !=(ItemId x, ItemId y) => !(x == y);
        public static bool operator ==(ItemId x, AssetId y) => x.Equals(y);
        public static bool operator !=(ItemId x, AssetId y) => !(x == y);
        public bool Equals(ItemId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
        public readonly ItemNameId ToItemName() => new ItemNameId(AssetType.ItemName, Id);
    }
}
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
    public struct ItemNameId : IEquatable<ItemNameId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public ItemNameId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.ItemName))
                throw new ArgumentOutOfRangeException($"Tried to construct a ItemNameId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a ItemNameId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public ItemNameId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.ItemName))
                throw new ArgumentOutOfRangeException($"Tried to construct a ItemNameId with a type of {Type}");
        }
        public ItemNameId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.ItemName))
                throw new ArgumentOutOfRangeException($"Tried to construct a ItemNameId with a type of {Type}");
        }

        public static ItemNameId From<T>(T id) where T : unmanaged, Enum => (ItemNameId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static ItemNameId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new ItemNameId(AssetType.ItemName, disk));
            return (ItemNameId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static ItemNameId SerdesU8(string name, ItemNameId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static ItemNameId SerdesU16(string name, ItemNameId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static ItemNameId None => new ItemNameId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static ItemNameId Parse(string s)
        {
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            // if (s == null || !s.Contains(":"))
            //     throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            // var parts = s.Split(':');
            // //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            // var type = AssetTypeExtensions.FromShort(parts[0]);
            // var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            // return new ItemNameId(type, id);
        }

        public static implicit operator AssetId(ItemNameId id) => new AssetId(id._value);
        public static implicit operator ItemNameId(AssetId id) => new ItemNameId((uint)id);
        public static explicit operator uint(ItemNameId id) => id._value;
        public static explicit operator int(ItemNameId id) => unchecked((int)id._value);
        public static explicit operator ItemNameId(int id) => new ItemNameId(id);
        public static implicit operator TextId(ItemNameId id) => new TextId(id._value);
        public static explicit operator ItemNameId(TextId id) => new ItemNameId((uint)id);
        public static implicit operator ItemNameId(UAlbion.Base.ItemName id) => ItemNameId.From(id);

        public static ItemNameId ToItemNameId(int id) => new ItemNameId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(ItemNameId x, ItemNameId y) => x.Equals(y);
        public static bool operator !=(ItemNameId x, ItemNameId y) => !(x == y);
        public static bool operator ==(ItemNameId x, AssetId y) => x.Equals(y);
        public static bool operator !=(ItemNameId x, AssetId y) => !(x == y);
        public bool Equals(ItemNameId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }
}
// Note: This file was automatically generated using Tools/CodeGenerator.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various types.
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(ToStringJsonConverter<ItemId>))]
    [TypeConverter(typeof(ItemIdConverter))]
    public readonly struct ItemId : IEquatable<ItemId>, IEquatable<AssetId>, IComparable, IAssetId
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

        ItemId(uint id) 
        {
            _value = id;
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
            id = FromDisk(type, diskValue, mapping);
            if (s.IsCommenting()) s.Comment(id.ToString());
            return id;
        }

        public static ItemId SerdesU16(string name, ItemId id, AssetType type, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            id = FromDisk(type, diskValue, mapping);
            if (s.IsCommenting()) s.Comment(id.ToString());
            return id;
        }

        public static ItemId SerdesU16BE(string name, ItemId id, AssetType type, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16BE(name, diskValue);
            id = FromDisk(type, diskValue, mapping);
            if (s.IsCommenting()) s.Comment(id.ToString());
            return id;
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static ItemId None => new ItemId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
        public static AssetType[] ValidTypes = { AssetType.Item, AssetType.Gold, AssetType.Rations };
        public static ItemId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

        public static implicit operator AssetId(ItemId id) => AssetId.FromUInt32(id._value);
        public static implicit operator ItemId(AssetId id) => new ItemId(id.ToUInt32());
        public static implicit operator ItemId(UAlbion.Base.Item id) => ItemId.From(id);

        public readonly int ToInt32() => unchecked((int)_value);
        public readonly uint ToUInt32() => _value;
        public static ItemId FromInt32(int id) => new ItemId(unchecked((uint)id));
        public static ItemId FromUInt32(uint id) => new ItemId(id);
        public static bool operator ==(ItemId x, ItemId y) => x.Equals(y);
        public static bool operator !=(ItemId x, ItemId y) => !(x == y);
        public static bool operator ==(ItemId x, AssetId y) => x.Equals(y);
        public static bool operator !=(ItemId x, AssetId y) => !(x == y);
        public static bool operator <(ItemId x, ItemId y) => x.CompareTo(y) == -1;
        public static bool operator >(ItemId x, ItemId y) => x.CompareTo(y) == 1;
        public static bool operator <=(ItemId x, ItemId y) => x.CompareTo(y) != 1;
        public static bool operator >=(ItemId x, ItemId y) => x.CompareTo(y) != -1;
        public bool Equals(ItemId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == other.ToUInt32();
        public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
        public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
        public override int GetHashCode() => unchecked((int)_value);
        public readonly TextId ToName() => new TextId(AssetType.ItemName, Id);
    }

    public class ItemIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? ItemId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}
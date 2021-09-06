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
    [JsonConverter(typeof(ToStringJsonConverter<TilesetGraphicsId>))]
    [TypeConverter(typeof(TilesetGraphicsIdConverter))]
    public readonly struct TilesetGraphicsId : IEquatable<TilesetGraphicsId>, IEquatable<AssetId>, IComparable, IAssetId
    {
        readonly uint _value;
        public TilesetGraphicsId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.TilesetGraphics))
                throw new ArgumentOutOfRangeException($"Tried to construct a TilesetGraphicsId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a TilesetGraphicsId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        TilesetGraphicsId(uint id) 
        {
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.TilesetGraphics))
                throw new ArgumentOutOfRangeException($"Tried to construct a TilesetGraphicsId with a type of {Type}");
        }

        public static TilesetGraphicsId From<T>(T id) where T : unmanaged, Enum => (TilesetGraphicsId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static TilesetGraphicsId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new TilesetGraphicsId(AssetType.TilesetGraphics, disk));
            return (TilesetGraphicsId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static TilesetGraphicsId SerdesU8(string name, TilesetGraphicsId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            id = FromDisk(diskValue, mapping);
            if (s.IsCommenting()) s.Comment(id.ToString());
            return id;
        }

        public static TilesetGraphicsId SerdesU16(string name, TilesetGraphicsId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            id = FromDisk(diskValue, mapping);
            if (s.IsCommenting()) s.Comment(id.ToString());
            return id;
        }

        public static TilesetGraphicsId SerdesU16BE(string name, TilesetGraphicsId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16BE(name, diskValue);
            id = FromDisk(diskValue, mapping);
            if (s.IsCommenting()) s.Comment(id.ToString());
            return id;
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static TilesetGraphicsId None => new TilesetGraphicsId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
        public static AssetType[] ValidTypes = { AssetType.TilesetGraphics };
        public static TilesetGraphicsId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

        public static implicit operator AssetId(TilesetGraphicsId id) => AssetId.FromUInt32(id._value);
        public static implicit operator TilesetGraphicsId(AssetId id) => new TilesetGraphicsId(id.ToUInt32());
        public static implicit operator SpriteId(TilesetGraphicsId id) => SpriteId.FromUInt32(id._value);
        public static explicit operator TilesetGraphicsId(SpriteId id) => new TilesetGraphicsId(id.ToUInt32());
        public static implicit operator TilesetGraphicsId(UAlbion.Base.TilesetGraphics id) => TilesetGraphicsId.From(id);

        public readonly int ToInt32() => unchecked((int)_value);
        public readonly uint ToUInt32() => _value;
        public static TilesetGraphicsId FromInt32(int id) => new TilesetGraphicsId(unchecked((uint)id));
        public static TilesetGraphicsId FromUInt32(uint id) => new TilesetGraphicsId(id);
        public static bool operator ==(TilesetGraphicsId x, TilesetGraphicsId y) => x.Equals(y);
        public static bool operator !=(TilesetGraphicsId x, TilesetGraphicsId y) => !(x == y);
        public static bool operator ==(TilesetGraphicsId x, AssetId y) => x.Equals(y);
        public static bool operator !=(TilesetGraphicsId x, AssetId y) => !(x == y);
        public static bool operator <(TilesetGraphicsId x, TilesetGraphicsId y) => x.CompareTo(y) == -1;
        public static bool operator >(TilesetGraphicsId x, TilesetGraphicsId y) => x.CompareTo(y) == 1;
        public static bool operator <=(TilesetGraphicsId x, TilesetGraphicsId y) => x.CompareTo(y) != 1;
        public static bool operator >=(TilesetGraphicsId x, TilesetGraphicsId y) => x.CompareTo(y) != -1;
        public bool Equals(TilesetGraphicsId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == other.ToUInt32();
        public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
        public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
        public override int GetHashCode() => unchecked((int)_value);
    }

    public class TilesetGraphicsIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? TilesetGraphicsId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}
// Note: This file was automatically generated using Tools/GenerateEnums.
// No changes should be made to this file by hand. Instead, the relevant json
// files should be modified and then GenerateEnums should be used to regenerate
// the various types.
using System;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(ToStringJsonConverter))]
    [TypeConverter(typeof(SpecialIdConverter))]
    public readonly struct SpecialId : IEquatable<SpecialId>, IEquatable<AssetId>, IComparable, ITextureId
    {
        readonly uint _value;
        public SpecialId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Special))
                throw new ArgumentOutOfRangeException($"Tried to construct a SpecialId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a SpecialId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        SpecialId(uint id) 
        {
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Special))
                throw new ArgumentOutOfRangeException($"Tried to construct a SpecialId with a type of {Type}");
        }

        public static SpecialId From<T>(T id) where T : unmanaged, Enum => (SpecialId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static SpecialId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new SpecialId(AssetType.Special, disk));
            return (SpecialId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static SpecialId SerdesU8(string name, SpecialId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static SpecialId SerdesU16(string name, SpecialId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static SpecialId SerdesU16BE(string name, SpecialId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16BE(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static SpecialId None => new SpecialId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        static AssetType[] _validTypes = { AssetType.Special };
        public static SpecialId Parse(string s) => AssetMapping.Global.Parse(s, _validTypes);

        public static implicit operator AssetId(SpecialId id) => AssetId.FromUInt32(id._value);
        public static implicit operator SpecialId(AssetId id) => new SpecialId(id.ToUInt32());
        public static implicit operator TextId(SpecialId id) => TextId.FromUInt32(id._value);
        public static explicit operator SpecialId(TextId id) => new SpecialId(id.ToUInt32());
        public static implicit operator SpecialId(UAlbion.Base.Special id) => SpecialId.From(id);

        public readonly int ToInt32() => unchecked((int)_value);
        public readonly uint ToUInt32() => _value;
        public static SpecialId FromInt32(int id) => new SpecialId(unchecked((uint)id));
        public static SpecialId FromUInt32(uint id) => new SpecialId(id);
        public static bool operator ==(SpecialId x, SpecialId y) => x.Equals(y);
        public static bool operator !=(SpecialId x, SpecialId y) => !(x == y);
        public static bool operator ==(SpecialId x, AssetId y) => x.Equals(y);
        public static bool operator !=(SpecialId x, AssetId y) => !(x == y);
        public static bool operator <(SpecialId x, SpecialId y) => x.CompareTo(y) == -1;
        public static bool operator >(SpecialId x, SpecialId y) => x.CompareTo(y) == 1;
        public static bool operator <=(SpecialId x, SpecialId y) => x.CompareTo(y) != 1;
        public static bool operator >=(SpecialId x, SpecialId y) => x.CompareTo(y) != -1;
        public bool Equals(SpecialId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == other.ToUInt32();
        public override bool Equals(object obj) => obj is ITextureId other && other.ToUInt32() == _value;
        public int CompareTo(object obj) => (obj is ITextureId other) ? _value.CompareTo(other.ToUInt32()) : -1;
        public override int GetHashCode() => unchecked((int)_value);
    }

    public class SpecialIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? SpecialId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}
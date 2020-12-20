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
    [TypeConverter(typeof(SpellIdConverter))]
    public readonly struct SpellId : IEquatable<SpellId>, IEquatable<AssetId>, IComparable, ITextureId
    {
        readonly uint _value;
        public SpellId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Spell))
                throw new ArgumentOutOfRangeException($"Tried to construct a SpellId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a SpellId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        SpellId(uint id) 
        {
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Spell))
                throw new ArgumentOutOfRangeException($"Tried to construct a SpellId with a type of {Type}");
        }

        public static SpellId From<T>(T id) where T : unmanaged, Enum => (SpellId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static SpellId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new SpellId(AssetType.Spell, disk));
            return (SpellId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static SpellId SerdesU8(string name, SpellId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static SpellId SerdesU16(string name, SpellId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static SpellId SerdesU16BE(string name, SpellId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16BE(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static SpellId None => new SpellId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        static AssetType[] _validTypes = { AssetType.Spell };
        public static SpellId Parse(string s) => AssetMapping.Global.Parse(s, _validTypes);

        public static implicit operator AssetId(SpellId id) => AssetId.FromUInt32(id._value);
        public static implicit operator SpellId(AssetId id) => new SpellId(id.ToUInt32());
        public static implicit operator SpellId(UAlbion.Base.Spell id) => SpellId.From(id);

        public readonly int ToInt32() => unchecked((int)_value);
        public readonly uint ToUInt32() => _value;
        public static SpellId FromInt32(int id) => new SpellId(unchecked((uint)id));
        public static SpellId FromUInt32(uint id) => new SpellId(id);
        public static bool operator ==(SpellId x, SpellId y) => x.Equals(y);
        public static bool operator !=(SpellId x, SpellId y) => !(x == y);
        public static bool operator ==(SpellId x, AssetId y) => x.Equals(y);
        public static bool operator !=(SpellId x, AssetId y) => !(x == y);
        public static bool operator <(SpellId x, SpellId y) => x.CompareTo(y) == -1;
        public static bool operator >(SpellId x, SpellId y) => x.CompareTo(y) == 1;
        public static bool operator <=(SpellId x, SpellId y) => x.CompareTo(y) != 1;
        public static bool operator >=(SpellId x, SpellId y) => x.CompareTo(y) != -1;
        public bool Equals(SpellId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == other.ToUInt32();
        public override bool Equals(object obj) => obj is ITextureId other && other.ToUInt32() == _value;
        public int CompareTo(object obj) => (obj is ITextureId other) ? _value.CompareTo(other.ToUInt32()) : -1;
        public override int GetHashCode() => unchecked((int)_value);
    }

    public class SpellIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? SpellId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}
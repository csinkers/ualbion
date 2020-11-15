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
    [TypeConverter(typeof(PaletteIdConverter))]
    public struct PaletteId : IEquatable<PaletteId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public PaletteId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Palette))
                throw new ArgumentOutOfRangeException($"Tried to construct a PaletteId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a PaletteId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public PaletteId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Palette))
                throw new ArgumentOutOfRangeException($"Tried to construct a PaletteId with a type of {Type}");
        }
        public PaletteId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.Palette))
                throw new ArgumentOutOfRangeException($"Tried to construct a PaletteId with a type of {Type}");
        }

        public static PaletteId From<T>(T id) where T : unmanaged, Enum => (PaletteId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static PaletteId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new PaletteId(AssetType.Palette, disk));
            return (PaletteId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static PaletteId SerdesU8(string name, PaletteId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static PaletteId SerdesU16(string name, PaletteId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static PaletteId None => new PaletteId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        static AssetType[] _validTypes = { AssetType.Palette };
        public static PaletteId Parse(string s) => AssetMapping.Global.Parse(s, _validTypes);

        public static implicit operator AssetId(PaletteId id) => new AssetId(id._value);
        public static implicit operator PaletteId(AssetId id) => new PaletteId((uint)id);
        public static explicit operator uint(PaletteId id) => id._value;
        public static explicit operator int(PaletteId id) => unchecked((int)id._value);
        public static explicit operator PaletteId(int id) => new PaletteId(id);
        public static implicit operator PaletteId(UAlbion.Base.Palette id) => PaletteId.From(id);

        public static PaletteId ToPaletteId(int id) => new PaletteId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(PaletteId x, PaletteId y) => x.Equals(y);
        public static bool operator !=(PaletteId x, PaletteId y) => !(x == y);
        public static bool operator ==(PaletteId x, AssetId y) => x.Equals(y);
        public static bool operator !=(PaletteId x, AssetId y) => !(x == y);
        public bool Equals(PaletteId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }

    public class PaletteIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? PaletteId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}
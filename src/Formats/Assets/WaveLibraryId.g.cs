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
    [TypeConverter(typeof(WaveLibraryIdConverter))]
    public struct WaveLibraryId : IEquatable<WaveLibraryId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public WaveLibraryId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.WaveLibrary))
                throw new ArgumentOutOfRangeException($"Tried to construct a WaveLibraryId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a WaveLibraryId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public WaveLibraryId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.WaveLibrary))
                throw new ArgumentOutOfRangeException($"Tried to construct a WaveLibraryId with a type of {Type}");
        }
        public WaveLibraryId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.WaveLibrary))
                throw new ArgumentOutOfRangeException($"Tried to construct a WaveLibraryId with a type of {Type}");
        }

        public static WaveLibraryId From<T>(T id) where T : unmanaged, Enum => (WaveLibraryId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static WaveLibraryId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new WaveLibraryId(AssetType.WaveLibrary, disk));
            return (WaveLibraryId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static WaveLibraryId SerdesU8(string name, WaveLibraryId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static WaveLibraryId SerdesU16(string name, WaveLibraryId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static WaveLibraryId None => new WaveLibraryId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        static AssetType[] _validTypes = { AssetType.WaveLibrary };
        public static WaveLibraryId Parse(string s) => AssetMapping.Global.Parse(s, _validTypes);

        public static implicit operator AssetId(WaveLibraryId id) => new AssetId(id._value);
        public static implicit operator WaveLibraryId(AssetId id) => new WaveLibraryId((uint)id);
        public static explicit operator uint(WaveLibraryId id) => id._value;
        public static explicit operator int(WaveLibraryId id) => unchecked((int)id._value);
        public static explicit operator WaveLibraryId(int id) => new WaveLibraryId(id);
        public static implicit operator WaveLibraryId(UAlbion.Base.WaveLibrary id) => WaveLibraryId.From(id);

        public static WaveLibraryId ToWaveLibraryId(int id) => new WaveLibraryId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(WaveLibraryId x, WaveLibraryId y) => x.Equals(y);
        public static bool operator !=(WaveLibraryId x, WaveLibraryId y) => !(x == y);
        public static bool operator ==(WaveLibraryId x, AssetId y) => x.Equals(y);
        public static bool operator !=(WaveLibraryId x, AssetId y) => !(x == y);
        public bool Equals(WaveLibraryId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }

    public class WaveLibraryIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? WaveLibraryId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}
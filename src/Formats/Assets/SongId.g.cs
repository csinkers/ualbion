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
    [TypeConverter(typeof(SongIdConverter))]
    public struct SongId : IEquatable<SongId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public SongId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Song))
                throw new ArgumentOutOfRangeException($"Tried to construct a SongId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a SongId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public SongId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Song))
                throw new ArgumentOutOfRangeException($"Tried to construct a SongId with a type of {Type}");
        }
        public SongId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.Song))
                throw new ArgumentOutOfRangeException($"Tried to construct a SongId with a type of {Type}");
        }

        public static SongId From<T>(T id) where T : unmanaged, Enum => (SongId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static SongId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new SongId(AssetType.Song, disk));
            return (SongId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static SongId SerdesU8(string name, SongId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static SongId SerdesU16(string name, SongId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static SongId None => new SongId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        static AssetType[] _validTypes = { AssetType.Song };
        public static SongId Parse(string s) => AssetMapping.Global.Parse(s, _validTypes);

        public static implicit operator AssetId(SongId id) => new AssetId(id._value);
        public static implicit operator SongId(AssetId id) => new SongId((uint)id);
        public static explicit operator uint(SongId id) => id._value;
        public static explicit operator int(SongId id) => unchecked((int)id._value);
        public static explicit operator SongId(int id) => new SongId(id);
        public static implicit operator SongId(UAlbion.Base.Song id) => SongId.From(id);

        public static SongId ToSongId(int id) => new SongId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(SongId x, SongId y) => x.Equals(y);
        public static bool operator !=(SongId x, SongId y) => !(x == y);
        public static bool operator ==(SongId x, AssetId y) => x.Equals(y);
        public static bool operator !=(SongId x, AssetId y) => !(x == y);
        public bool Equals(SongId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
        public readonly WaveLibraryId ToWaveLibrary() => new WaveLibraryId(AssetType.WaveLibrary, Id);
    }

    public class SongIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? SongId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}
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
    [TypeConverter(typeof(PictureIdConverter))]
    public readonly struct PictureId : IEquatable<PictureId>, IEquatable<AssetId>, IComparable, ITextureId
    {
        readonly uint _value;
        public PictureId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Picture))
                throw new ArgumentOutOfRangeException($"Tried to construct a PictureId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a PictureId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        PictureId(uint id) 
        {
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Picture))
                throw new ArgumentOutOfRangeException($"Tried to construct a PictureId with a type of {Type}");
        }

        public static PictureId From<T>(T id) where T : unmanaged, Enum => (PictureId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static PictureId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new PictureId(AssetType.Picture, disk));
            return (PictureId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static PictureId SerdesU8(string name, PictureId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            id = FromDisk(diskValue, mapping);
            if (s.IsCommenting()) s.Comment(id.ToString());
            return id;
        }

        public static PictureId SerdesU16(string name, PictureId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            id = FromDisk(diskValue, mapping);
            if (s.IsCommenting()) s.Comment(id.ToString());
            return id;
        }

        public static PictureId SerdesU16BE(string name, PictureId id, AssetMapping mapping, ISerializer s)
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
        public static PictureId None => new PictureId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        static AssetType[] _validTypes = { AssetType.Picture };
        public static PictureId Parse(string s) => AssetMapping.Global.Parse(s, _validTypes);

        public static implicit operator AssetId(PictureId id) => AssetId.FromUInt32(id._value);
        public static implicit operator PictureId(AssetId id) => new PictureId(id.ToUInt32());
        public static implicit operator SpriteId(PictureId id) => SpriteId.FromUInt32(id._value);
        public static explicit operator PictureId(SpriteId id) => new PictureId(id.ToUInt32());
        public static implicit operator PictureId(UAlbion.Base.Picture id) => PictureId.From(id);

        public readonly int ToInt32() => unchecked((int)_value);
        public readonly uint ToUInt32() => _value;
        public static PictureId FromInt32(int id) => new PictureId(unchecked((uint)id));
        public static PictureId FromUInt32(uint id) => new PictureId(id);
        public static bool operator ==(PictureId x, PictureId y) => x.Equals(y);
        public static bool operator !=(PictureId x, PictureId y) => !(x == y);
        public static bool operator ==(PictureId x, AssetId y) => x.Equals(y);
        public static bool operator !=(PictureId x, AssetId y) => !(x == y);
        public static bool operator <(PictureId x, PictureId y) => x.CompareTo(y) == -1;
        public static bool operator >(PictureId x, PictureId y) => x.CompareTo(y) == 1;
        public static bool operator <=(PictureId x, PictureId y) => x.CompareTo(y) != 1;
        public static bool operator >=(PictureId x, PictureId y) => x.CompareTo(y) != -1;
        public bool Equals(PictureId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == other.ToUInt32();
        public override bool Equals(object obj) => obj is ITextureId other && other.ToUInt32() == _value;
        public int CompareTo(object obj) => (obj is ITextureId other) ? _value.CompareTo(other.ToUInt32()) : -1;
        public override int GetHashCode() => unchecked((int)_value);
    }

    public class PictureIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? PictureId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}
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
    [JsonConverter(typeof(ToStringJsonConverter<EventSetId>))]
    [TypeConverter(typeof(EventSetIdConverter))]
    public readonly struct EventSetId : IEquatable<EventSetId>, IEquatable<AssetId>, IComparable, IAssetId
    {
        readonly uint _value;
        public EventSetId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.EventSet))
                throw new ArgumentOutOfRangeException($"Tried to construct a EventSetId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a EventSetId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        EventSetId(uint id) 
        {
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.EventSet))
                throw new ArgumentOutOfRangeException($"Tried to construct a EventSetId with a type of {Type}");
        }

        public static EventSetId From<T>(T id) where T : unmanaged, Enum => (EventSetId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static EventSetId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new EventSetId(AssetType.EventSet, disk));
            return (EventSetId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static EventSetId SerdesU8(string name, EventSetId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            id = FromDisk(diskValue, mapping);
            if (s.IsCommenting()) s.Comment(id.ToString());
            return id;
        }

        public static EventSetId SerdesU16(string name, EventSetId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            id = FromDisk(diskValue, mapping);
            if (s.IsCommenting()) s.Comment(id.ToString());
            return id;
        }

        public static EventSetId SerdesU16BE(string name, EventSetId id, AssetMapping mapping, ISerializer s)
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
        public static EventSetId None => new EventSetId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
        public static AssetType[] ValidTypes = { AssetType.EventSet };
        public static EventSetId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

        public static implicit operator AssetId(EventSetId id) => AssetId.FromUInt32(id._value);
        public static implicit operator EventSetId(AssetId id) => new EventSetId(id.ToUInt32());
        public static implicit operator EventSetId(UAlbion.Base.EventSet id) => EventSetId.From(id);

        public readonly int ToInt32() => unchecked((int)_value);
        public readonly uint ToUInt32() => _value;
        public static EventSetId FromInt32(int id) => new EventSetId(unchecked((uint)id));
        public static EventSetId FromUInt32(uint id) => new EventSetId(id);
        public static bool operator ==(EventSetId x, EventSetId y) => x.Equals(y);
        public static bool operator !=(EventSetId x, EventSetId y) => !(x == y);
        public static bool operator ==(EventSetId x, AssetId y) => x.Equals(y);
        public static bool operator !=(EventSetId x, AssetId y) => !(x == y);
        public static bool operator <(EventSetId x, EventSetId y) => x.CompareTo(y) == -1;
        public static bool operator >(EventSetId x, EventSetId y) => x.CompareTo(y) == 1;
        public static bool operator <=(EventSetId x, EventSetId y) => x.CompareTo(y) != 1;
        public static bool operator >=(EventSetId x, EventSetId y) => x.CompareTo(y) != -1;
        public bool Equals(EventSetId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == other.ToUInt32();
        public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
        public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
        public override int GetHashCode() => unchecked((int)_value);
        public readonly TextId ToEventText() => new TextId(AssetType.EventText, Id);
    }

    public class EventSetIdConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
            => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
            => value is string s ? EventSetId.Parse(s) : base.ConvertFrom(context, culture, value);

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
    }
}
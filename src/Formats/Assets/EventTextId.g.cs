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
    public struct EventTextId : IEquatable<EventTextId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public EventTextId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.EventText))
                throw new ArgumentOutOfRangeException($"Tried to construct a EventTextId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a EventTextId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public EventTextId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.EventText))
                throw new ArgumentOutOfRangeException($"Tried to construct a EventTextId with a type of {Type}");
        }
        public EventTextId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.EventText))
                throw new ArgumentOutOfRangeException($"Tried to construct a EventTextId with a type of {Type}");
        }

        public static EventTextId From<T>(T id) where T : unmanaged, Enum => (EventTextId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static EventTextId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new EventTextId(AssetType.EventText, disk));
            return (EventTextId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static EventTextId SerdesU8(string name, EventTextId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static EventTextId SerdesU16(string name, EventTextId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static EventTextId None => new EventTextId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static EventTextId Parse(string s)
        {
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            // if (s == null || !s.Contains(":"))
            //     throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            // var parts = s.Split(':');
            // //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            // var type = AssetTypeExtensions.FromShort(parts[0]);
            // var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            // return new EventTextId(type, id);
        }

        public static implicit operator AssetId(EventTextId id) => new AssetId(id._value);
        public static implicit operator EventTextId(AssetId id) => new EventTextId((uint)id);
        public static explicit operator uint(EventTextId id) => id._value;
        public static explicit operator int(EventTextId id) => unchecked((int)id._value);
        public static explicit operator EventTextId(int id) => new EventTextId(id);
        public static implicit operator TextId(EventTextId id) => new TextId(id._value);
        public static explicit operator EventTextId(TextId id) => new EventTextId((uint)id);
        public static implicit operator EventTextId(UAlbion.Base.EventText id) => EventTextId.From(id);

        public static EventTextId ToEventTextId(int id) => new EventTextId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(EventTextId x, EventTextId y) => x.Equals(y);
        public static bool operator !=(EventTextId x, EventTextId y) => !(x == y);
        public static bool operator ==(EventTextId x, AssetId y) => x.Equals(y);
        public static bool operator !=(EventTextId x, AssetId y) => !(x == y);
        public bool Equals(EventTextId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }
}
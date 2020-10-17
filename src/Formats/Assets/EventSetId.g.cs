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
    public struct EventSetId : IEquatable<EventSetId>, IEquatable<AssetId>, ITextureId
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

        public EventSetId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.EventSet))
                throw new ArgumentOutOfRangeException($"Tried to construct a EventSetId with a type of {Type}");
        }
        public EventSetId(int id)
        {
            _value = unchecked((uint)id);
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
            return FromDisk(diskValue, mapping);
        }

        public static EventSetId SerdesU16(string name, EventSetId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static EventSetId None => new EventSetId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static EventSetId Parse(string s)
        {
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            // if (s == null || !s.Contains(":"))
            //     throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            // var parts = s.Split(':');
            // //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            // var type = AssetTypeExtensions.FromShort(parts[0]);
            // var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            // return new EventSetId(type, id);
        }

        public static implicit operator AssetId(EventSetId id) => new AssetId(id._value);
        public static implicit operator EventSetId(AssetId id) => new EventSetId((uint)id);
        public static explicit operator uint(EventSetId id) => id._value;
        public static explicit operator int(EventSetId id) => unchecked((int)id._value);
        public static explicit operator EventSetId(int id) => new EventSetId(id);
        public static implicit operator EventSetId(UAlbion.Base.EventSet id) => EventSetId.From(id);

        public static EventSetId ToEventSetId(int id) => new EventSetId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(EventSetId x, EventSetId y) => x.Equals(y);
        public static bool operator !=(EventSetId x, EventSetId y) => !(x == y);
        public static bool operator ==(EventSetId x, AssetId y) => x.Equals(y);
        public static bool operator !=(EventSetId x, AssetId y) => !(x == y);
        public bool Equals(EventSetId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
        public readonly TextId ToEventText() => new TextId(AssetType.EventText, Id);
    }
}
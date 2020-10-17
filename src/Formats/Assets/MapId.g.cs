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
    public struct MapId : IEquatable<MapId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public MapId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Map))
                throw new ArgumentOutOfRangeException($"Tried to construct a MapId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a MapId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public MapId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Map))
                throw new ArgumentOutOfRangeException($"Tried to construct a MapId with a type of {Type}");
        }
        public MapId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.Map))
                throw new ArgumentOutOfRangeException($"Tried to construct a MapId with a type of {Type}");
        }

        public static MapId From<T>(T id) where T : unmanaged, Enum => (MapId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static MapId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new MapId(AssetType.Map, disk));
            return (MapId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static MapId SerdesU8(string name, MapId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static MapId SerdesU16(string name, MapId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static MapId None => new MapId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static MapId Parse(string s)
        {
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            // if (s == null || !s.Contains(":"))
            //     throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            // var parts = s.Split(':');
            // //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            // var type = AssetTypeExtensions.FromShort(parts[0]);
            // var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            // return new MapId(type, id);
        }

        public static implicit operator AssetId(MapId id) => new AssetId(id._value);
        public static implicit operator MapId(AssetId id) => new MapId((uint)id);
        public static explicit operator uint(MapId id) => id._value;
        public static explicit operator int(MapId id) => unchecked((int)id._value);
        public static explicit operator MapId(int id) => new MapId(id);
        public static implicit operator MapId(UAlbion.Base.Map id) => MapId.From(id);

        public static MapId ToMapId(int id) => new MapId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(MapId x, MapId y) => x.Equals(y);
        public static bool operator !=(MapId x, MapId y) => !(x == y);
        public static bool operator ==(MapId x, AssetId y) => x.Equals(y);
        public static bool operator !=(MapId x, AssetId y) => !(x == y);
        public bool Equals(MapId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
        public readonly TextId ToMapText() => new TextId(AssetType.MapText, Id);
    }
}
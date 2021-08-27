using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using SerdesNet;
using UAlbion.Api.Visual;

namespace UAlbion.Config
{
    // 8 bit base type (256)
    // 24 bit id (16M)
    // [JsonConverter(typeof(ToStringJsonConverter))]
    public readonly struct AssetId : IEquatable<AssetId>, IComparable, IAssetId
    {
        readonly uint _value;
        public AssetId(AssetType type, int id = 0) // Should only really be called by AssetMapping
        {
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a SpriteId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public static AssetId From<T>(T id) where T : unmanaged, Enum => AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static AssetId FromDisk(AssetType type, int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = mapping.IdToEnum(new AssetId(type, disk));
            return AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static AssetId SerdesU8(string name, AssetId id, AssetType type, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(type, diskValue, mapping);
        }

        public static AssetId SerdesU16(string name, AssetId id, AssetType type, AssetMapping mapping, ISerializer s)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            if (s == null) throw new ArgumentNullException(nameof(s));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(id);
            ushort diskValue = (ushort)mapping.EnumToId(enumType, enumValue).Id;

            diskValue = s.UInt16(name, diskValue);

            (enumType, enumValue) = mapping.IdToEnum(new AssetId(type, diskValue));
            return AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static AssetId None { get; } = new(AssetType.None);
        public static AssetId Gold { get; } = new(AssetType.Gold);
        public static AssetId Rations { get; } = new(AssetType.Rations);
        public bool IsNone => _value == 0;
        AssetId(uint id) => _value = id;
        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public override string ToString() => AssetMapping.Global.IdToName(this);
        public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
        public static AssetId Parse(string s) => AssetMapping.Global.Parse(s, null);
        public readonly int ToInt32() => unchecked((int)_value);
        public readonly uint ToUInt32() => _value;
        public static AssetId FromInt32(int id) => new(unchecked((uint)id));
        public static AssetId FromUInt32(uint id) => new(id);
        public static bool operator ==(AssetId x, AssetId y) => x.Equals(y);
        public static bool operator !=(AssetId x, AssetId y) => !(x == y);
        public static bool operator <(AssetId x, AssetId y) => x.CompareTo(y) == -1;
        public static bool operator >(AssetId x, AssetId y) => x.CompareTo(y) == 1;
        public static bool operator <=(AssetId x, AssetId y) => x.CompareTo(y) != 1;
        public static bool operator >=(AssetId x, AssetId y) => x.CompareTo(y) != -1;
        public bool Equals(AssetId other) => _value == other._value;
        public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
        public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
        public override int GetHashCode() => unchecked((int)_value);
        public static IEnumerable<AssetId> EnumerateAll(AssetType type) => AssetMapping.Global.EnumerateAssetsOfType(type);
    }
}
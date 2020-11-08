using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Config
{
    // 8 bit base type (256)
    // 24 bit id (16M)
    [JsonConverter(typeof(ToStringJsonConverter))]
    public struct AssetId : IEquatable<AssetId>, ITextureId
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

        public static AssetId None { get; } = new AssetId(AssetType.None);
        public static AssetId Gold { get; } = new AssetId(AssetType.Gold);
        public static AssetId Rations { get; } = new AssetId(AssetType.Rations);
        public bool IsNone => _value == 0;
        public AssetId(uint id) => _value = id;
        public AssetId(int id) => _value = unchecked((uint)id);
        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static AssetId Parse(string s)
        {
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            /*
            if (s == null || !s.Contains(":"))
                throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            var parts = s.Split(':');
            //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            var type = AssetTypeExtensions.FromShort(parts[0]);
            var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            return new AssetId(type, id); */
        }

        public static explicit operator uint(AssetId id) => id._value;
        public static explicit operator int(AssetId id) => unchecked((int)id._value);
        public static explicit operator AssetId(int id) => new AssetId(id);
        public static AssetId ToAssetId(int id) => new AssetId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(AssetId x, AssetId y) => x.Equals(y);
        public static bool operator !=(AssetId x, AssetId y) => !(x == y);
        public bool Equals(AssetId other) => _value == other._value;
        public override bool Equals(object obj) => obj is AssetId other && Equals(other);
        public override int GetHashCode() => (int)this;

        public static IEnumerable<AssetId> EnumerateAll(AssetType type) => AssetMapping.Global.EnumeratAssetsOfType(type);
    }
}
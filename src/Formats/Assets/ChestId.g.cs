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
    public struct ChestId : IEquatable<ChestId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public ChestId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Chest))
                throw new ArgumentOutOfRangeException($"Tried to construct a ChestId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a ChestId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public ChestId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Chest))
                throw new ArgumentOutOfRangeException($"Tried to construct a ChestId with a type of {Type}");
        }
        public ChestId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.Chest))
                throw new ArgumentOutOfRangeException($"Tried to construct a ChestId with a type of {Type}");
        }

        public static ChestId From<T>(T id) where T : unmanaged, Enum => (ChestId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static ChestId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new ChestId(AssetType.Chest, disk));
            return (ChestId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static ChestId SerdesU8(string name, ChestId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static ChestId SerdesU16(string name, ChestId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static ChestId None => new ChestId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static ChestId Parse(string s)
        {
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            // if (s == null || !s.Contains(":"))
            //     throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            // var parts = s.Split(':');
            // //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            // var type = AssetTypeExtensions.FromShort(parts[0]);
            // var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            // return new ChestId(type, id);
        }

        public static implicit operator AssetId(ChestId id) => new AssetId(id._value);
        public static implicit operator ChestId(AssetId id) => new ChestId((uint)id);
        public static explicit operator uint(ChestId id) => id._value;
        public static explicit operator int(ChestId id) => unchecked((int)id._value);
        public static explicit operator ChestId(int id) => new ChestId(id);
        public static implicit operator ChestId(UAlbion.Base.Chest id) => ChestId.From(id);

        public static ChestId ToChestId(int id) => new ChestId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(ChestId x, ChestId y) => x.Equals(y);
        public static bool operator !=(ChestId x, ChestId y) => !(x == y);
        public static bool operator ==(ChestId x, AssetId y) => x.Equals(y);
        public static bool operator !=(ChestId x, AssetId y) => !(x == y);
        public bool Equals(ChestId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }
}
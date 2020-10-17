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
    public struct WordId : IEquatable<WordId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public WordId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Word))
                throw new ArgumentOutOfRangeException($"Tried to construct a WordId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a WordId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public WordId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Word))
                throw new ArgumentOutOfRangeException($"Tried to construct a WordId with a type of {Type}");
        }
        public WordId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.Word))
                throw new ArgumentOutOfRangeException($"Tried to construct a WordId with a type of {Type}");
        }

        public static WordId From<T>(T id) where T : unmanaged, Enum => (WordId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static WordId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new WordId(AssetType.Word, disk));
            return (WordId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static WordId SerdesU8(string name, WordId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static WordId SerdesU16(string name, WordId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static WordId None => new WordId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static WordId Parse(string s)
        {
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            // if (s == null || !s.Contains(":"))
            //     throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            // var parts = s.Split(':');
            // //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            // var type = AssetTypeExtensions.FromShort(parts[0]);
            // var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            // return new WordId(type, id);
        }

        public static implicit operator AssetId(WordId id) => new AssetId(id._value);
        public static implicit operator WordId(AssetId id) => new WordId((uint)id);
        public static explicit operator uint(WordId id) => id._value;
        public static explicit operator int(WordId id) => unchecked((int)id._value);
        public static explicit operator WordId(int id) => new WordId(id);
        public static implicit operator TextId(WordId id) => new TextId(id._value);
        public static explicit operator WordId(TextId id) => new WordId((uint)id);
        public static implicit operator WordId(UAlbion.Base.Word id) => WordId.From(id);

        public static WordId ToWordId(int id) => new WordId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(WordId x, WordId y) => x.Equals(y);
        public static bool operator !=(WordId x, WordId y) => !(x == y);
        public static bool operator ==(WordId x, AssetId y) => x.Equals(y);
        public static bool operator !=(WordId x, AssetId y) => !(x == y);
        public bool Equals(WordId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }
}
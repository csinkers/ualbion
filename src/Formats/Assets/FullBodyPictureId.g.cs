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
    public struct FullBodyPictureId : IEquatable<FullBodyPictureId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public FullBodyPictureId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.FullBodyPicture))
                throw new ArgumentOutOfRangeException($"Tried to construct a FullBodyPictureId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a FullBodyPictureId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public FullBodyPictureId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.FullBodyPicture))
                throw new ArgumentOutOfRangeException($"Tried to construct a FullBodyPictureId with a type of {Type}");
        }
        public FullBodyPictureId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.FullBodyPicture))
                throw new ArgumentOutOfRangeException($"Tried to construct a FullBodyPictureId with a type of {Type}");
        }

        public static FullBodyPictureId From<T>(T id) where T : unmanaged, Enum => (FullBodyPictureId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static FullBodyPictureId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new FullBodyPictureId(AssetType.FullBodyPicture, disk));
            return (FullBodyPictureId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static FullBodyPictureId SerdesU8(string name, FullBodyPictureId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static FullBodyPictureId SerdesU16(string name, FullBodyPictureId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static FullBodyPictureId None => new FullBodyPictureId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static FullBodyPictureId Parse(string s)
        {
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            // if (s == null || !s.Contains(":"))
            //     throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            // var parts = s.Split(':');
            // //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            // var type = AssetTypeExtensions.FromShort(parts[0]);
            // var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            // return new FullBodyPictureId(type, id);
        }

        public static implicit operator AssetId(FullBodyPictureId id) => new AssetId(id._value);
        public static implicit operator FullBodyPictureId(AssetId id) => new FullBodyPictureId((uint)id);
        public static explicit operator uint(FullBodyPictureId id) => id._value;
        public static explicit operator int(FullBodyPictureId id) => unchecked((int)id._value);
        public static explicit operator FullBodyPictureId(int id) => new FullBodyPictureId(id);
        public static implicit operator SpriteId(FullBodyPictureId id) => new SpriteId(id._value);
        public static explicit operator FullBodyPictureId(SpriteId id) => new FullBodyPictureId((uint)id);
        public static implicit operator FullBodyPictureId(UAlbion.Base.FullBodyPicture id) => FullBodyPictureId.From(id);

        public static FullBodyPictureId ToFullBodyPictureId(int id) => new FullBodyPictureId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(FullBodyPictureId x, FullBodyPictureId y) => x.Equals(y);
        public static bool operator !=(FullBodyPictureId x, FullBodyPictureId y) => !(x == y);
        public static bool operator ==(FullBodyPictureId x, AssetId y) => x.Equals(y);
        public static bool operator !=(FullBodyPictureId x, AssetId y) => !(x == y);
        public bool Equals(FullBodyPictureId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }
}
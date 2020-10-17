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
    public struct VideoId : IEquatable<VideoId>, IEquatable<AssetId>, ITextureId
    {
        readonly uint _value;
        public VideoId(AssetType type, int id = 0)
        {
            if (!(type == AssetType.None || type == AssetType.Video))
                throw new ArgumentOutOfRangeException($"Tried to construct a VideoId with a type of {type}");
#if DEBUG
            if (id < 0 || id > 0xffffff)
                throw new ArgumentOutOfRangeException($"Tried to construct a VideoId with out of range id {id}");
#endif
            _value = (uint)type << 24 | (uint)id;
        }

        public VideoId(uint id) 
        { 
            _value = id;
            if (!(Type == AssetType.None || Type == AssetType.Video))
                throw new ArgumentOutOfRangeException($"Tried to construct a VideoId with a type of {Type}");
        }
        public VideoId(int id)
        {
            _value = unchecked((uint)id);
            if (!(Type == AssetType.None || Type == AssetType.Video))
                throw new ArgumentOutOfRangeException($"Tried to construct a VideoId with a type of {Type}");
        }

        public static VideoId From<T>(T id) where T : unmanaged, Enum => (VideoId)AssetMapping.Global.EnumToId(id);

        public int ToDisk(AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
            return mapping.EnumToId(enumType, enumValue).Id;
        }

        public static VideoId FromDisk(int disk, AssetMapping mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            

            var (enumType, enumValue) = mapping.IdToEnum(new VideoId(AssetType.Video, disk));
            return (VideoId)AssetMapping.Global.EnumToId(enumType, enumValue);
        }

        public static VideoId SerdesU8(string name, VideoId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            byte diskValue = (byte)id.ToDisk(mapping);
            diskValue = s.UInt8(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public static VideoId SerdesU16(string name, VideoId id, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            ushort diskValue = (ushort)id.ToDisk(mapping);
            diskValue = s.UInt16(name, diskValue);
            return FromDisk(diskValue, mapping);
        }

        public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
        public readonly int Id => (int)(_value & 0xffffff);
        public static VideoId None => new VideoId(AssetType.None);
        public bool IsNone => Type == AssetType.None;

        public override string ToString() => AssetMapping.Global.IdToName(this);
        public static VideoId Parse(string s)
        {
            throw new NotImplementedException(); // TODO: Add proper parsing of arbitrary asset enums
            // if (s == null || !s.Contains(":"))
            //     throw new FormatException($"Tried to parse an InventoryId without a : (\"{s}\")");
            // var parts = s.Split(':');
            // //var type = (AssetType)Enum.Parse(typeof(AssetType), parts[0]);
            // var type = AssetTypeExtensions.FromShort(parts[0]);
            // var id = ushort.Parse(parts[1], CultureInfo.InvariantCulture);
            // return new VideoId(type, id);
        }

        public static implicit operator AssetId(VideoId id) => new AssetId(id._value);
        public static implicit operator VideoId(AssetId id) => new VideoId((uint)id);
        public static explicit operator uint(VideoId id) => id._value;
        public static explicit operator int(VideoId id) => unchecked((int)id._value);
        public static explicit operator VideoId(int id) => new VideoId(id);
        public static implicit operator VideoId(UAlbion.Base.Video id) => VideoId.From(id);

        public static VideoId ToVideoId(int id) => new VideoId(id);
        public readonly int ToInt32() => (int)this;
        public readonly uint ToUInt32() => (uint)this;
        public static bool operator ==(VideoId x, VideoId y) => x.Equals(y);
        public static bool operator !=(VideoId x, VideoId y) => !(x == y);
        public static bool operator ==(VideoId x, AssetId y) => x.Equals(y);
        public static bool operator !=(VideoId x, AssetId y) => !(x == y);
        public bool Equals(VideoId other) => _value == other._value;
        public bool Equals(AssetId other) => _value == (uint)other;
        public override bool Equals(object obj) => obj is ITextureId other && Equals(other);
        public override int GetHashCode() => (int)this;
    }
}
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

namespace UAlbion.Formats.Ids;

[JsonConverter(typeof(ToStringJsonConverter<BlockListId>))]
[TypeConverter(typeof(BlockListIdConverter))]
public readonly struct BlockListId : IEquatable<BlockListId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public BlockListId(AssetType type, int id = 0)
    {
        if (!(type == AssetType.None || type == AssetType.BlockList))
            throw new ArgumentOutOfRangeException($"Tried to construct a BlockListId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a BlockListId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    public BlockListId(int id)
    {
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a BlockListId with out of range id {id}");
#endif
        _value = (uint)AssetType.BlockList << 24 | (uint)id;
    }

    BlockListId(uint id) 
    {
        _value = id;
        if (!(Type == AssetType.None || Type == AssetType.BlockList))
            throw new ArgumentOutOfRangeException($"Tried to construct a BlockListId with a type of {Type}");
    }

    public BlockListId(IAssetId id)
    {
        _value = id.ToUInt32();
        if (!(Type == AssetType.None || Type == AssetType.BlockList))
            throw new ArgumentOutOfRangeException($"Tried to construct a BlockListId with a type of {Type}");
    }

    public static BlockListId From<T>(T id) where T : unmanaged, Enum => (BlockListId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static BlockListId FromDisk(int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = mapping.IdToEnum(new BlockListId(AssetType.BlockList, disk));
        return (BlockListId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static BlockListId SerdesU8(string name, BlockListId id, AssetMapping mapping, ISerdes s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static BlockListId SerdesU16(string name, BlockListId id, AssetMapping mapping, ISerdes s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static BlockListId SerdesU16BE(string name, BlockListId id, AssetMapping mapping, ISerdes s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16BE(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
    public readonly int Id => (int)(_value & 0xffffff);
    public static BlockListId None => new BlockListId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString();
    public static AssetType[] ValidTypes = { AssetType.BlockList };
    public static BlockListId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(BlockListId id) => AssetId.FromUInt32(id._value);
    public static implicit operator BlockListId(AssetId id) => new BlockListId(id.ToUInt32());
    public static implicit operator BlockListId(UAlbion.Base.BlockList id) => BlockListId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static BlockListId FromInt32(int id) => new BlockListId(unchecked((uint)id));
    public static BlockListId FromUInt32(uint id) => new BlockListId(id);
    public static bool operator ==(BlockListId x, BlockListId y) => x.Equals(y);
    public static bool operator !=(BlockListId x, BlockListId y) => !(x == y);
    public static bool operator ==(BlockListId x, AssetId y) => x.Equals(y);
    public static bool operator !=(BlockListId x, AssetId y) => !(x == y);
    public static bool operator <(BlockListId x, BlockListId y) => x.CompareTo(y) == -1;
    public static bool operator >(BlockListId x, BlockListId y) => x.CompareTo(y) == 1;
    public static bool operator <=(BlockListId x, BlockListId y) => x.CompareTo(y) != 1;
    public static bool operator >=(BlockListId x, BlockListId y) => x.CompareTo(y) != -1;
    public bool Equals(BlockListId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
    public readonly TilesetId ToTileset() => new TilesetId(AssetType.Tileset, Id);
}

public class BlockListIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? BlockListId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
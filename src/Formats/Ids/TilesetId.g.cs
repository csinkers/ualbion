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

[JsonConverter(typeof(ToStringJsonConverter<TilesetId>))]
[TypeConverter(typeof(TilesetIdConverter))]
public readonly struct TilesetId : IEquatable<TilesetId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public TilesetId(AssetType type, int id = 0)
    {
        if (!(type == AssetType.None || type == AssetType.Tileset))
            throw new ArgumentOutOfRangeException($"Tried to construct a TilesetId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a TilesetId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    public TilesetId(int id)
    {
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a TilesetId with out of range id {id}");
#endif
        _value = (uint)AssetType.Tileset << 24 | (uint)id;
    }

    TilesetId(uint id) 
    {
        _value = id;
        if (!(Type == AssetType.None || Type == AssetType.Tileset))
            throw new ArgumentOutOfRangeException($"Tried to construct a TilesetId with a type of {Type}");
    }

    public TilesetId(IAssetId id)
    {
        _value = id.ToUInt32();
        if (!(Type == AssetType.None || Type == AssetType.Tileset))
            throw new ArgumentOutOfRangeException($"Tried to construct a TilesetId with a type of {Type}");
    }

    public static TilesetId From<T>(T id) where T : unmanaged, Enum => (TilesetId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static TilesetId FromDisk(int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = mapping.IdToEnum(new TilesetId(AssetType.Tileset, disk));
        return (TilesetId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static TilesetId SerdesU8(string name, TilesetId id, AssetMapping mapping, ISerdes s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static TilesetId SerdesU16(string name, TilesetId id, AssetMapping mapping, ISerdes s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static TilesetId SerdesU16BE(string name, TilesetId id, AssetMapping mapping, ISerdes s)
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
    public static TilesetId None => new TilesetId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString();
    public static AssetType[] ValidTypes = { AssetType.Tileset };
    public static TilesetId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(TilesetId id) => AssetId.FromUInt32(id._value);
    public static implicit operator TilesetId(AssetId id) => new TilesetId(id.ToUInt32());
    public static implicit operator TilesetId(UAlbion.Base.Tileset id) => TilesetId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static TilesetId FromInt32(int id) => new TilesetId(unchecked((uint)id));
    public static TilesetId FromUInt32(uint id) => new TilesetId(id);
    public static bool operator ==(TilesetId x, TilesetId y) => x.Equals(y);
    public static bool operator !=(TilesetId x, TilesetId y) => !(x == y);
    public static bool operator ==(TilesetId x, AssetId y) => x.Equals(y);
    public static bool operator !=(TilesetId x, AssetId y) => !(x == y);
    public static bool operator <(TilesetId x, TilesetId y) => x.CompareTo(y) == -1;
    public static bool operator >(TilesetId x, TilesetId y) => x.CompareTo(y) == 1;
    public static bool operator <=(TilesetId x, TilesetId y) => x.CompareTo(y) != 1;
    public static bool operator >=(TilesetId x, TilesetId y) => x.CompareTo(y) != -1;
    public bool Equals(TilesetId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
    public readonly BlockListId ToBlockList() => new BlockListId(AssetType.BlockList, Id);
    public readonly TilesetGfxId ToTilesetGfx() => new TilesetGfxId(AssetType.TilesetGfx, Id);
}

public class TilesetIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? TilesetId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
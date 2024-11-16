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

[JsonConverter(typeof(ToStringJsonConverter<TilesetGfxId>))]
[TypeConverter(typeof(TilesetGfxIdConverter))]
public readonly struct TilesetGfxId : IEquatable<TilesetGfxId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public TilesetGfxId(AssetType type, int id = 0)
    {
        if (!(type == AssetType.None || type == AssetType.TilesetGfx))
            throw new ArgumentOutOfRangeException($"Tried to construct a TilesetGfxId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a TilesetGfxId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    public TilesetGfxId(int id)
    {
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a TilesetGfxId with out of range id {id}");
#endif
        _value = (uint)AssetType.TilesetGfx << 24 | (uint)id;
    }

    TilesetGfxId(uint id) 
    {
        _value = id;
        if (!(Type == AssetType.None || Type == AssetType.TilesetGfx))
            throw new ArgumentOutOfRangeException($"Tried to construct a TilesetGfxId with a type of {Type}");
    }

    public TilesetGfxId(IAssetId id)
    {
        _value = id.ToUInt32();
        if (!(Type == AssetType.None || Type == AssetType.TilesetGfx))
            throw new ArgumentOutOfRangeException($"Tried to construct a TilesetGfxId with a type of {Type}");
    }

    public static TilesetGfxId From<T>(T id) where T : unmanaged, Enum => (TilesetGfxId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static TilesetGfxId FromDisk(int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = mapping.IdToEnum(new TilesetGfxId(AssetType.TilesetGfx, disk));
        return (TilesetGfxId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static TilesetGfxId SerdesU8(string name, TilesetGfxId id, AssetMapping mapping, ISerdes s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static TilesetGfxId SerdesU16(string name, TilesetGfxId id, AssetMapping mapping, ISerdes s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static TilesetGfxId SerdesU16BE(string name, TilesetGfxId id, AssetMapping mapping, ISerdes s)
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
    public static TilesetGfxId None => new TilesetGfxId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString();
    public static AssetType[] ValidTypes = { AssetType.TilesetGfx };
    public static TilesetGfxId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(TilesetGfxId id) => AssetId.FromUInt32(id._value);
    public static implicit operator TilesetGfxId(AssetId id) => new TilesetGfxId(id.ToUInt32());
    public static implicit operator SpriteId(TilesetGfxId id) => SpriteId.FromUInt32(id._value);
    public static explicit operator TilesetGfxId(SpriteId id) => new TilesetGfxId(id.ToUInt32());
    public static implicit operator TilesetGfxId(UAlbion.Base.TilesetGfx id) => TilesetGfxId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static TilesetGfxId FromInt32(int id) => new TilesetGfxId(unchecked((uint)id));
    public static TilesetGfxId FromUInt32(uint id) => new TilesetGfxId(id);
    public static bool operator ==(TilesetGfxId x, TilesetGfxId y) => x.Equals(y);
    public static bool operator !=(TilesetGfxId x, TilesetGfxId y) => !(x == y);
    public static bool operator ==(TilesetGfxId x, AssetId y) => x.Equals(y);
    public static bool operator !=(TilesetGfxId x, AssetId y) => !(x == y);
    public static bool operator <(TilesetGfxId x, TilesetGfxId y) => x.CompareTo(y) == -1;
    public static bool operator >(TilesetGfxId x, TilesetGfxId y) => x.CompareTo(y) == 1;
    public static bool operator <=(TilesetGfxId x, TilesetGfxId y) => x.CompareTo(y) != 1;
    public static bool operator >=(TilesetGfxId x, TilesetGfxId y) => x.CompareTo(y) != -1;
    public bool Equals(TilesetGfxId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
}

public class TilesetGfxIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? TilesetGfxId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
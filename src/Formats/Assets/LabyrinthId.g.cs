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

namespace UAlbion.Formats.Assets;

[JsonConverter(typeof(ToStringJsonConverter<LabyrinthId>))]
[TypeConverter(typeof(LabyrinthIdConverter))]
public readonly struct LabyrinthId : IEquatable<LabyrinthId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public LabyrinthId(AssetType type, int id = 0)
    {
        if (!(type == AssetType.None || type == AssetType.Labyrinth))
            throw new ArgumentOutOfRangeException($"Tried to construct a LabyrinthId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a LabyrinthId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    LabyrinthId(uint id) 
    {
        _value = id;
        if (!(Type == AssetType.None || Type == AssetType.Labyrinth))
            throw new ArgumentOutOfRangeException($"Tried to construct a LabyrinthId with a type of {Type}");
    }

    public static LabyrinthId From<T>(T id) where T : unmanaged, Enum => (LabyrinthId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static LabyrinthId FromDisk(int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = mapping.IdToEnum(new LabyrinthId(AssetType.Labyrinth, disk));
        return (LabyrinthId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static LabyrinthId SerdesU8(string name, LabyrinthId id, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public static LabyrinthId SerdesU16(string name, LabyrinthId id, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public static LabyrinthId SerdesU16BE(string name, LabyrinthId id, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16BE(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
    public readonly int Id => (int)(_value & 0xffffff);
    public static LabyrinthId None => new LabyrinthId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
    public static AssetType[] ValidTypes = { AssetType.Labyrinth };
    public static LabyrinthId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(LabyrinthId id) => AssetId.FromUInt32(id._value);
    public static implicit operator LabyrinthId(AssetId id) => new LabyrinthId(id.ToUInt32());
    public static implicit operator LabyrinthId(UAlbion.Base.Labyrinth id) => LabyrinthId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static LabyrinthId FromInt32(int id) => new LabyrinthId(unchecked((uint)id));
    public static LabyrinthId FromUInt32(uint id) => new LabyrinthId(id);
    public static bool operator ==(LabyrinthId x, LabyrinthId y) => x.Equals(y);
    public static bool operator !=(LabyrinthId x, LabyrinthId y) => !(x == y);
    public static bool operator ==(LabyrinthId x, AssetId y) => x.Equals(y);
    public static bool operator !=(LabyrinthId x, AssetId y) => !(x == y);
    public static bool operator <(LabyrinthId x, LabyrinthId y) => x.CompareTo(y) == -1;
    public static bool operator >(LabyrinthId x, LabyrinthId y) => x.CompareTo(y) == 1;
    public static bool operator <=(LabyrinthId x, LabyrinthId y) => x.CompareTo(y) != 1;
    public static bool operator >=(LabyrinthId x, LabyrinthId y) => x.CompareTo(y) != -1;
    public bool Equals(LabyrinthId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
}

public class LabyrinthIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? LabyrinthId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
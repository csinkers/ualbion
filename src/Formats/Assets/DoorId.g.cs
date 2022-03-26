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

[JsonConverter(typeof(ToStringJsonConverter<DoorId>))]
[TypeConverter(typeof(DoorIdConverter))]
public readonly struct DoorId : IEquatable<DoorId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public DoorId(AssetType type, int id = 0)
    {
        if (!(type == AssetType.None || type == AssetType.Door))
            throw new ArgumentOutOfRangeException($"Tried to construct a DoorId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a DoorId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    DoorId(uint id) 
    {
        _value = id;
        if (!(Type == AssetType.None || Type == AssetType.Door))
            throw new ArgumentOutOfRangeException($"Tried to construct a DoorId with a type of {Type}");
    }

    public static DoorId From<T>(T id) where T : unmanaged, Enum => (DoorId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static DoorId FromDisk(int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        

        var (enumType, enumValue) = mapping.IdToEnum(new DoorId(AssetType.Door, disk));
        return (DoorId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static DoorId SerdesU8(string name, DoorId id, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public static DoorId SerdesU16(string name, DoorId id, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public static DoorId SerdesU16BE(string name, DoorId id, AssetMapping mapping, ISerializer s)
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
    public static DoorId None => new DoorId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
    public static AssetType[] ValidTypes = { AssetType.Door };
    public static DoorId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(DoorId id) => AssetId.FromUInt32(id._value);
    public static implicit operator DoorId(AssetId id) => new DoorId(id.ToUInt32());
    public static implicit operator DoorId(UAlbion.Base.Door id) => DoorId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static DoorId FromInt32(int id) => new DoorId(unchecked((uint)id));
    public static DoorId FromUInt32(uint id) => new DoorId(id);
    public static bool operator ==(DoorId x, DoorId y) => x.Equals(y);
    public static bool operator !=(DoorId x, DoorId y) => !(x == y);
    public static bool operator ==(DoorId x, AssetId y) => x.Equals(y);
    public static bool operator !=(DoorId x, AssetId y) => !(x == y);
    public static bool operator <(DoorId x, DoorId y) => x.CompareTo(y) == -1;
    public static bool operator >(DoorId x, DoorId y) => x.CompareTo(y) == 1;
    public static bool operator <=(DoorId x, DoorId y) => x.CompareTo(y) != 1;
    public static bool operator >=(DoorId x, DoorId y) => x.CompareTo(y) != -1;
    public bool Equals(DoorId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
}

public class DoorIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? DoorId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
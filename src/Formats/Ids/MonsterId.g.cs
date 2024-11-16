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

[JsonConverter(typeof(ToStringJsonConverter<MonsterId>))]
[TypeConverter(typeof(MonsterIdConverter))]
public readonly struct MonsterId : IEquatable<MonsterId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public MonsterId(AssetType type, int id = 0)
    {
        if (!(type == AssetType.None || type == AssetType.MonsterSheet))
            throw new ArgumentOutOfRangeException($"Tried to construct a MonsterId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a MonsterId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    public MonsterId(int id)
    {
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a MonsterId with out of range id {id}");
#endif
        _value = (uint)AssetType.MonsterSheet << 24 | (uint)id;
    }

    MonsterId(uint id) 
    {
        _value = id;
        if (!(Type == AssetType.None || Type == AssetType.MonsterSheet))
            throw new ArgumentOutOfRangeException($"Tried to construct a MonsterId with a type of {Type}");
    }

    public MonsterId(IAssetId id)
    {
        _value = id.ToUInt32();
        if (!(Type == AssetType.None || Type == AssetType.MonsterSheet))
            throw new ArgumentOutOfRangeException($"Tried to construct a MonsterId with a type of {Type}");
    }

    public static MonsterId From<T>(T id) where T : unmanaged, Enum => (MonsterId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static MonsterId FromDisk(int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = mapping.IdToEnum(new MonsterId(AssetType.MonsterSheet, disk));
        return (MonsterId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static MonsterId SerdesU8(string name, MonsterId id, AssetMapping mapping, ISerdes s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static MonsterId SerdesU16(string name, MonsterId id, AssetMapping mapping, ISerdes s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static MonsterId SerdesU16BE(string name, MonsterId id, AssetMapping mapping, ISerdes s)
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
    public static MonsterId None => new MonsterId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString();
    public static AssetType[] ValidTypes = { AssetType.MonsterSheet };
    public static MonsterId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(MonsterId id) => AssetId.FromUInt32(id._value);
    public static implicit operator MonsterId(AssetId id) => new MonsterId(id.ToUInt32());
    public static implicit operator SheetId(MonsterId id) => SheetId.FromUInt32(id._value);
    public static explicit operator MonsterId(SheetId id) => new MonsterId(id.ToUInt32());
    public static implicit operator MonsterId(UAlbion.Base.MonsterSheet id) => MonsterId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static MonsterId FromInt32(int id) => new MonsterId(unchecked((uint)id));
    public static MonsterId FromUInt32(uint id) => new MonsterId(id);
    public static bool operator ==(MonsterId x, MonsterId y) => x.Equals(y);
    public static bool operator !=(MonsterId x, MonsterId y) => !(x == y);
    public static bool operator ==(MonsterId x, AssetId y) => x.Equals(y);
    public static bool operator !=(MonsterId x, AssetId y) => !(x == y);
    public static bool operator <(MonsterId x, MonsterId y) => x.CompareTo(y) == -1;
    public static bool operator >(MonsterId x, MonsterId y) => x.CompareTo(y) == 1;
    public static bool operator <=(MonsterId x, MonsterId y) => x.CompareTo(y) != 1;
    public static bool operator >=(MonsterId x, MonsterId y) => x.CompareTo(y) != -1;
    public bool Equals(MonsterId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
}

public class MonsterIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? MonsterId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
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

[JsonConverter(typeof(ToStringJsonConverter<SheetId>))]
[TypeConverter(typeof(SheetIdConverter))]
public readonly struct SheetId : IEquatable<SheetId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public SheetId(AssetType type, int id = 0)
    {
        if (!(type == AssetType.None || type >= AssetType.PartySheet && type <= AssetType.NpcSheet))
            throw new ArgumentOutOfRangeException($"Tried to construct a SheetId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a SheetId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    SheetId(uint id) 
    {
        _value = id;
        if (!(Type == AssetType.None || Type >= AssetType.PartySheet && Type <= AssetType.NpcSheet))
            throw new ArgumentOutOfRangeException($"Tried to construct a SheetId with a type of {Type}");
    }

    public static SheetId From<T>(T id) where T : unmanaged, Enum => (SheetId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static SheetId FromDisk(AssetType type, int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (!(type == AssetType.None || type >= AssetType.PartySheet && type <= AssetType.NpcSheet))
            throw new ArgumentOutOfRangeException($"Tried to construct a SheetId with a type of {type}");

        var (enumType, enumValue) = mapping.IdToEnum(new SheetId(type, disk));
        return (SheetId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static SheetId SerdesU8(string name, SheetId id, AssetType type, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(type, diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static SheetId SerdesU16(string name, SheetId id, AssetType type, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(type, diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static SheetId SerdesU16BE(string name, SheetId id, AssetType type, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16BE(name, diskValue);
        id = FromDisk(type, diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
    public readonly int Id => (int)(_value & 0xffffff);
    public static SheetId None => new SheetId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
    public static AssetType[] ValidTypes = { AssetType.MonsterSheet, AssetType.NpcSheet, AssetType.PartySheet };
    public static SheetId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(SheetId id) => AssetId.FromUInt32(id._value);
    public static implicit operator SheetId(AssetId id) => new SheetId(id.ToUInt32());
    public static implicit operator SheetId(UAlbion.Base.MonsterSheet id) => SheetId.From(id);
    public static implicit operator SheetId(UAlbion.Base.NpcSheet id) => SheetId.From(id);
    public static implicit operator SheetId(UAlbion.Base.PartySheet id) => SheetId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static SheetId FromInt32(int id) => new SheetId(unchecked((uint)id));
    public static SheetId FromUInt32(uint id) => new SheetId(id);
    public static bool operator ==(SheetId x, SheetId y) => x.Equals(y);
    public static bool operator !=(SheetId x, SheetId y) => !(x == y);
    public static bool operator ==(SheetId x, AssetId y) => x.Equals(y);
    public static bool operator !=(SheetId x, AssetId y) => !(x == y);
    public static bool operator <(SheetId x, SheetId y) => x.CompareTo(y) == -1;
    public static bool operator >(SheetId x, SheetId y) => x.CompareTo(y) == 1;
    public static bool operator <=(SheetId x, SheetId y) => x.CompareTo(y) != 1;
    public static bool operator >=(SheetId x, SheetId y) => x.CompareTo(y) != -1;
    public bool Equals(SheetId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
}

public class SheetIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? SheetId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
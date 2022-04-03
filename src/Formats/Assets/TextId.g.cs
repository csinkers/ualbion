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

[JsonConverter(typeof(ToStringJsonConverter<TextId>))]
[TypeConverter(typeof(TextIdConverter))]
public readonly struct TextId : IEquatable<TextId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public TextId(AssetType type, int id = 0)
    {
        if (!(type == AssetType.None || type >= AssetType.EventText && type <= AssetType.Word || type == AssetType.Special))
            throw new ArgumentOutOfRangeException($"Tried to construct a TextId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a TextId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    TextId(uint id) 
    {
        _value = id;
        if (!(Type == AssetType.None || Type >= AssetType.EventText && Type <= AssetType.Word || Type == AssetType.Special))
            throw new ArgumentOutOfRangeException($"Tried to construct a TextId with a type of {Type}");
    }

    public static TextId From<T>(T id) where T : unmanaged, Enum => (TextId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static TextId FromDisk(AssetType type, int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (!(type == AssetType.None || type >= AssetType.EventText && type <= AssetType.Word || type == AssetType.Special))
            throw new ArgumentOutOfRangeException($"Tried to construct a TextId with a type of {type}");

        var (enumType, enumValue) = mapping.IdToEnum(new TextId(type, disk));
        return (TextId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static TextId SerdesU8(string name, TextId id, AssetType type, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(type, diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public static TextId SerdesU16(string name, TextId id, AssetType type, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(type, diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public static TextId SerdesU16BE(string name, TextId id, AssetType type, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16BE(name, diskValue);
        id = FromDisk(type, diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public readonly AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
    public readonly int Id => (int)(_value & 0xffffff);
    public static TextId None => new TextId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
    public static AssetType[] ValidTypes = { AssetType.EventText, AssetType.ItemName, AssetType.MapText, AssetType.Special, AssetType.Text, AssetType.Word };
    public static TextId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(TextId id) => AssetId.FromUInt32(id._value);
    public static implicit operator TextId(AssetId id) => new TextId(id.ToUInt32());
    public static implicit operator TextId(UAlbion.Base.EventText id) => TextId.From(id);
    public static implicit operator TextId(UAlbion.Base.ItemName id) => TextId.From(id);
    public static implicit operator TextId(UAlbion.Base.MapText id) => TextId.From(id);
    public static implicit operator TextId(UAlbion.Base.Special id) => TextId.From(id);
    public static implicit operator TextId(UAlbion.Base.SystemText id) => TextId.From(id);
    public static implicit operator TextId(UAlbion.Base.UAlbionString id) => TextId.From(id);
    public static implicit operator TextId(UAlbion.Base.Word id) => TextId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static TextId FromInt32(int id) => new TextId(unchecked((uint)id));
    public static TextId FromUInt32(uint id) => new TextId(id);
    public static bool operator ==(TextId x, TextId y) => x.Equals(y);
    public static bool operator !=(TextId x, TextId y) => !(x == y);
    public static bool operator ==(TextId x, AssetId y) => x.Equals(y);
    public static bool operator !=(TextId x, AssetId y) => !(x == y);
    public static bool operator <(TextId x, TextId y) => x.CompareTo(y) == -1;
    public static bool operator >(TextId x, TextId y) => x.CompareTo(y) == 1;
    public static bool operator <=(TextId x, TextId y) => x.CompareTo(y) != 1;
    public static bool operator >=(TextId x, TextId y) => x.CompareTo(y) != -1;
    public bool Equals(TextId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
}

public class TextIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? TextId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
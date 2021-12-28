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

[JsonConverter(typeof(ToStringJsonConverter<MerchantId>))]
[TypeConverter(typeof(MerchantIdConverter))]
public readonly struct MerchantId : IEquatable<MerchantId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public MerchantId(AssetType type, int id = 0)
    {
        if (!(type == AssetType.None || type == AssetType.Merchant))
            throw new ArgumentOutOfRangeException($"Tried to construct a MerchantId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a MerchantId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    MerchantId(uint id) 
    {
        _value = id;
        if (!(Type == AssetType.None || Type == AssetType.Merchant))
            throw new ArgumentOutOfRangeException($"Tried to construct a MerchantId with a type of {Type}");
    }

    public static MerchantId From<T>(T id) where T : unmanaged, Enum => (MerchantId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static MerchantId FromDisk(int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        

        var (enumType, enumValue) = mapping.IdToEnum(new MerchantId(AssetType.Merchant, disk));
        return (MerchantId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static MerchantId SerdesU8(string name, MerchantId id, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public static MerchantId SerdesU16(string name, MerchantId id, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public static MerchantId SerdesU16BE(string name, MerchantId id, AssetMapping mapping, ISerializer s)
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
    public static MerchantId None => new MerchantId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
    public static AssetType[] ValidTypes = { AssetType.Merchant };
    public static MerchantId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(MerchantId id) => AssetId.FromUInt32(id._value);
    public static implicit operator MerchantId(AssetId id) => new MerchantId(id.ToUInt32());
        public static implicit operator MerchantId(UAlbion.Base.Merchant id) => MerchantId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static MerchantId FromInt32(int id) => new MerchantId(unchecked((uint)id));
    public static MerchantId FromUInt32(uint id) => new MerchantId(id);
    public static bool operator ==(MerchantId x, MerchantId y) => x.Equals(y);
    public static bool operator !=(MerchantId x, MerchantId y) => !(x == y);
    public static bool operator ==(MerchantId x, AssetId y) => x.Equals(y);
    public static bool operator !=(MerchantId x, AssetId y) => !(x == y);
    public static bool operator <(MerchantId x, MerchantId y) => x.CompareTo(y) == -1;
    public static bool operator >(MerchantId x, MerchantId y) => x.CompareTo(y) == 1;
    public static bool operator <=(MerchantId x, MerchantId y) => x.CompareTo(y) != 1;
    public static bool operator >=(MerchantId x, MerchantId y) => x.CompareTo(y) != -1;
    public bool Equals(MerchantId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
}

public class MerchantIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? MerchantId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
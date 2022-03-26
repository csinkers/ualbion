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

[JsonConverter(typeof(ToStringJsonConverter<PartyMemberId>))]
[TypeConverter(typeof(PartyMemberIdConverter))]
public readonly struct PartyMemberId : IEquatable<PartyMemberId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public PartyMemberId(AssetType type, int id = 0)
    {
        if (!(type == AssetType.None || type == AssetType.Party))
            throw new ArgumentOutOfRangeException($"Tried to construct a PartyMemberId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a PartyMemberId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    PartyMemberId(uint id) 
    {
        _value = id;
        if (!(Type == AssetType.None || Type == AssetType.Party))
            throw new ArgumentOutOfRangeException($"Tried to construct a PartyMemberId with a type of {Type}");
    }

    public static PartyMemberId From<T>(T id) where T : unmanaged, Enum => (PartyMemberId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static PartyMemberId FromDisk(int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        

        var (enumType, enumValue) = mapping.IdToEnum(new PartyMemberId(AssetType.Party, disk));
        return (PartyMemberId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static PartyMemberId SerdesU8(string name, PartyMemberId id, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public static PartyMemberId SerdesU16(string name, PartyMemberId id, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString());
        return id;
    }

    public static PartyMemberId SerdesU16BE(string name, PartyMemberId id, AssetMapping mapping, ISerializer s)
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
    public static PartyMemberId None => new PartyMemberId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
    public static AssetType[] ValidTypes = { AssetType.Party };
    public static PartyMemberId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(PartyMemberId id) => AssetId.FromUInt32(id._value);
    public static implicit operator PartyMemberId(AssetId id) => new PartyMemberId(id.ToUInt32());
    public static implicit operator CharacterId(PartyMemberId id) => CharacterId.FromUInt32(id._value);
    public static explicit operator PartyMemberId(CharacterId id) => new PartyMemberId(id.ToUInt32());
    public static implicit operator TargetId(PartyMemberId id) => TargetId.FromUInt32(id._value);
    public static explicit operator PartyMemberId(TargetId id) => new PartyMemberId(id.ToUInt32());
    public static implicit operator PartyMemberId(UAlbion.Base.PartyMember id) => PartyMemberId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static PartyMemberId FromInt32(int id) => new PartyMemberId(unchecked((uint)id));
    public static PartyMemberId FromUInt32(uint id) => new PartyMemberId(id);
    public static bool operator ==(PartyMemberId x, PartyMemberId y) => x.Equals(y);
    public static bool operator !=(PartyMemberId x, PartyMemberId y) => !(x == y);
    public static bool operator ==(PartyMemberId x, AssetId y) => x.Equals(y);
    public static bool operator !=(PartyMemberId x, AssetId y) => !(x == y);
    public static bool operator <(PartyMemberId x, PartyMemberId y) => x.CompareTo(y) == -1;
    public static bool operator >(PartyMemberId x, PartyMemberId y) => x.CompareTo(y) == 1;
    public static bool operator <=(PartyMemberId x, PartyMemberId y) => x.CompareTo(y) != 1;
    public static bool operator >=(PartyMemberId x, PartyMemberId y) => x.CompareTo(y) != -1;
    public bool Equals(PartyMemberId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
    public readonly SpriteId ToSmallPartyGraphics() => new SpriteId(AssetType.SmallPartyGraphics, Id);
    public readonly SpriteId ToLargePartyGraphics() => new SpriteId(AssetType.LargePartyGraphics, Id);
    public readonly SpriteId ToFullBodyPicture() => new SpriteId(AssetType.FullBodyPicture, Id);
}

public class PartyMemberIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? PartyMemberId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
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

[JsonConverter(typeof(ToStringJsonConverter<SpriteId>))]
[TypeConverter(typeof(SpriteIdConverter))]
public readonly struct SpriteId : IEquatable<SpriteId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;
    public SpriteId(AssetType type, int id = 0)
    {
        if (!(type >= AssetType.None && type <= AssetType.WallOverlay))
            throw new ArgumentOutOfRangeException($"Tried to construct a SpriteId with a type of {type}");
#if DEBUG
        if (id < 0 || id > 0xffffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a SpriteId with out of range id {id}");
#endif
        _value = (uint)type << 24 | (uint)id;
    }

    SpriteId(uint id) 
    {
        _value = id;
        if (!(Type >= AssetType.None && Type <= AssetType.WallOverlay))
            throw new ArgumentOutOfRangeException($"Tried to construct a SpriteId with a type of {Type}");
    }

    public static SpriteId From<T>(T id) where T : unmanaged, Enum => (SpriteId)AssetMapping.Global.EnumToId(id);

    public int ToDisk(AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        var (enumType, enumValue) = AssetMapping.Global.IdToEnum(this);
        return mapping.EnumToId(enumType, enumValue).Id;
    }

    public static SpriteId FromDisk(AssetType type, int disk, AssetMapping mapping)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (!(type >= AssetType.None && type <= AssetType.WallOverlay))
            throw new ArgumentOutOfRangeException($"Tried to construct a SpriteId with a type of {type}");

        var (enumType, enumValue) = mapping.IdToEnum(new SpriteId(type, disk));
        return (SpriteId)AssetMapping.Global.EnumToId(enumType, enumValue);
    }

    public static SpriteId SerdesU8(string name, SpriteId id, AssetType type, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        byte diskValue = (byte)id.ToDisk(mapping);
        diskValue = s.UInt8(name, diskValue);
        id = FromDisk(type, diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static SpriteId SerdesU16(string name, SpriteId id, AssetType type, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        ushort diskValue = (ushort)id.ToDisk(mapping);
        diskValue = s.UInt16(name, diskValue);
        id = FromDisk(type, diskValue, mapping);
        if (s.IsCommenting()) s.Comment(id.ToString(), true);
        return id;
    }

    public static SpriteId SerdesU16BE(string name, SpriteId id, AssetType type, AssetMapping mapping, ISerializer s)
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
    public static SpriteId None => new SpriteId(AssetType.None);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => AssetMapping.Global.IdToName(this);
    public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);
    public static AssetType[] ValidTypes = { 
        AssetType.AutomapGfx, AssetType.BackgroundGfx, AssetType.CombatBackground, 
        AssetType.CombatGfx, AssetType.CoreGfx, AssetType.Floor, AssetType.FontGfx, 
        AssetType.ItemGfx, AssetType.MonsterGfx, AssetType.NpcLargeGfx, 
        AssetType.NpcSmallGfx, AssetType.Object3D, AssetType.PartyLargeGfx, 
        AssetType.PartySmallGfx, AssetType.PartyInventoryGfx, AssetType.Picture, 
        AssetType.Portrait, AssetType.Slab, AssetType.TacticalGfx, AssetType.TilesetGfx, 
        AssetType.Wall, AssetType.WallOverlay };
    public static SpriteId Parse(string s) => AssetMapping.Global.Parse(s, ValidTypes);

    public static implicit operator AssetId(SpriteId id) => AssetId.FromUInt32(id._value);
    public static implicit operator SpriteId(AssetId id) => new SpriteId(id.ToUInt32());
    public static implicit operator SpriteId(UAlbion.Base.AutomapTiles id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.CombatBackground id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.CombatGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.CoreGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.DungeonBackground id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.DungeonObject id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.Floor id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.FontGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.ItemGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.MonsterGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.NpcLargeGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.NpcSmallGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.PartyInventoryGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.PartyLargeGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.PartySmallGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.Picture id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.Portrait id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.TacticalGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.TilesetGfx id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.UiBackground id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.Wall id) => SpriteId.From(id);
    public static implicit operator SpriteId(UAlbion.Base.WallOverlay id) => SpriteId.From(id);

    public readonly int ToInt32() => unchecked((int)_value);
    public readonly uint ToUInt32() => _value;
    public static SpriteId FromInt32(int id) => new SpriteId(unchecked((uint)id));
    public static SpriteId FromUInt32(uint id) => new SpriteId(id);
    public static bool operator ==(SpriteId x, SpriteId y) => x.Equals(y);
    public static bool operator !=(SpriteId x, SpriteId y) => !(x == y);
    public static bool operator ==(SpriteId x, AssetId y) => x.Equals(y);
    public static bool operator !=(SpriteId x, AssetId y) => !(x == y);
    public static bool operator <(SpriteId x, SpriteId y) => x.CompareTo(y) == -1;
    public static bool operator >(SpriteId x, SpriteId y) => x.CompareTo(y) == 1;
    public static bool operator <=(SpriteId x, SpriteId y) => x.CompareTo(y) != 1;
    public static bool operator >=(SpriteId x, SpriteId y) => x.CompareTo(y) != -1;
    public bool Equals(SpriteId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
}

public class SpriteIdConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) 
        => sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
        => value is string s ? SpriteId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => destinationType == typeof(string) ? value.ToString() : base.ConvertTo(context, culture, value, destinationType);
}
using System;
using System.Globalization;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public readonly struct MetaFontId : IEquatable<MetaFontId>, IEquatable<AssetId>, IComparable, IAssetId
{
    readonly uint _value;

    public MetaFontId(FontId fontId, InkId inkId)
    {
#if DEBUG
        if (fontId.Id is < 0 or > 0xffff)
            throw new ArgumentOutOfRangeException($"Tried to construct a MetaFontId with out of range font id {fontId} ({fontId.Id})");

        if (inkId.Id is < 0 or > 0xff)
            throw new ArgumentOutOfRangeException($"Tried to construct a MetaFontId with out of range ink id {inkId} ({inkId.Id})");
#endif
        _value = (uint)AssetType.MetaFont << 24 | (uint)((byte)inkId.Id << 16) | (uint)fontId.Id;
    }

    MetaFontId(uint id) 
    {
        _value = id;
        if (Type is not (AssetType.None or AssetType.MetaFont))
            throw new ArgumentOutOfRangeException($"Tried to construct a MetaFontId with a type of {Type}");
    }

    public InkId InkId => new((int)((_value & 0xff_0000) >> 16));
    public FontId FontId => new((int)(_value & 0xffff));
    public AssetType Type => (AssetType)((_value & 0xff00_0000) >> 24);
    public int Id => (int)(_value & 0xffffff);
    public static MetaFontId None => new(0);
    public bool IsNone => Type == AssetType.None;

    public override string ToString() => $"{AssetMapping.Global.IdToName(FontId)}:{InkId}";
    public string ToStringNumeric() => Id.ToString(CultureInfo.InvariantCulture);

    public static implicit operator AssetId(MetaFontId id) => AssetId.FromUInt32(id._value);
    public static implicit operator MetaFontId(AssetId id) => new(id.ToUInt32());

    public int ToInt32() => unchecked((int)_value);
    public uint ToUInt32() => _value;
    public static MetaFontId FromInt32(int id) => new(unchecked((uint)id));
    public static MetaFontId FromUInt32(uint id) => new(id);
    public static bool operator ==(MetaFontId x, MetaFontId y) => x.Equals(y);
    public static bool operator !=(MetaFontId x, MetaFontId y) => !(x == y);
    public static bool operator ==(MetaFontId x, AssetId y) => x.Equals(y);
    public static bool operator !=(MetaFontId x, AssetId y) => !(x == y);
    public static bool operator <(MetaFontId x, MetaFontId y) => x.CompareTo(y) == -1;
    public static bool operator >(MetaFontId x, MetaFontId y) => x.CompareTo(y) == 1;
    public static bool operator <=(MetaFontId x, MetaFontId y) => x.CompareTo(y) != 1;
    public static bool operator >=(MetaFontId x, MetaFontId y) => x.CompareTo(y) != -1;
    public bool Equals(MetaFontId other) => _value == other._value;
    public bool Equals(AssetId other) => _value == other.ToUInt32();
    public override bool Equals(object obj) => obj is IAssetId other && other.ToUInt32() == _value;
    public int CompareTo(object obj) => (obj is IAssetId other) ? _value.CompareTo(other.ToUInt32()) : -1;
    public override int GetHashCode() => unchecked((int)_value);
}
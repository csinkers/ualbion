using System;

namespace UAlbion.Config;

public readonly struct AssetPath : IEquatable<AssetPath>
{
    public AssetId AssetId { get; }
    public int SubAsset { get; }
    public int? PaletteId { get; }
    public int? PaletteFrame { get; }
    public string Name { get; }

    public override string ToString() => $"{AssetId.Id}:{SubAsset}{(PaletteId == null ? "" : " P" + PaletteId)} {Name}";

    public AssetPath(AssetId id, int subAsset = 0, int? paletteId = null, string overrideName = null, int? paletteFrame = null)
    {
        AssetId = id;
        Name = overrideName ?? ConfigUtil.AssetName(id);
        SubAsset = subAsset;
        PaletteId = paletteId;
        PaletteFrame = paletteFrame;
    }

    public override bool Equals(object obj) => obj is AssetPath other && Equals(other);
    public bool Equals(AssetPath other) => AssetId == other.AssetId && SubAsset == other.SubAsset && PaletteId == other.PaletteId && Name == other.Name;
    public static bool operator ==(AssetPath left, AssetPath right) => left.Equals(right);
    public static bool operator !=(AssetPath left, AssetPath right) => !(left == right);
    public override int GetHashCode() => HashCode.Combine(AssetId, SubAsset, PaletteId, Name);
}
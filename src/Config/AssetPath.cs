using System;

namespace UAlbion.Config;

public readonly struct AssetPath : IEquatable<AssetPath>
{
    public int Index { get; }
    public int SubAsset { get; }
    public int? PaletteId { get; }
    public int? PaletteFrame { get; }
    public string Name { get; }

    public override string ToString() => $"{Index}:{SubAsset}{(PaletteId==null ? "" : " P"+PaletteId)} {Name}";
    public AssetPath(int index, int subAsset = 0, int? paletteId = null, string overrideName = null, int? paletteFrame = null)
    {
        Index = index;
        SubAsset = subAsset;
        PaletteId = paletteId;
        Name = overrideName;
        PaletteFrame = paletteFrame;
    }

    public AssetPath(AssetInfo info, int subAsset = 0, string overrideName = null)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));
        Index = info.Index;
        SubAsset = subAsset;
        Name = string.IsNullOrEmpty(overrideName) ? ConfigUtil.AssetName(info.AssetId) : overrideName;
        PaletteId = info.Get(AssetProperty.PaletteId, -1);
        if (PaletteId == -1)
            PaletteId = null;

        PaletteFrame = null;
    }

    public override bool Equals(object obj) => obj is AssetPath other && Equals(other);
    public bool Equals(AssetPath other) => Index == other.Index && SubAsset == other.SubAsset && PaletteId == other.PaletteId && Name == other.Name;
    public static bool operator ==(AssetPath left, AssetPath right) => left.Equals(right);
    public static bool operator !=(AssetPath left, AssetPath right) => !(left == right);
    public override int GetHashCode() => HashCode.Combine(Index, SubAsset, PaletteId, Name);
}
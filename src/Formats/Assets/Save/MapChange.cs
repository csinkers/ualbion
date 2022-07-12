using System;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Save;

public class MapChange // Basically an IconChangeEvent without the MapEventType & Scope
{
    public const int SizeOnDisk = 8;

    public byte X { get; set; }
    public byte Y { get; set; }
    public IconChangeType ChangeType { get; set; }
    public ChangeIconLayers Layers { get; set; } // Only applicable for Blocks
    public ushort Value { get; set; }
    public MapId MapId { get; set; }

    public override string ToString() => $"MapΔ {X:X2} {Y:X2} {ChangeType} {Layers} {Value:X4} {MapId}";
    public static MapChange Serdes(int i, MapChange u, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        u ??= new MapChange();
        s.Begin();
        u.X = s.UInt8(nameof(X), u.X);
        u.Y = s.UInt8(nameof(Y), u.Y);
        u.ChangeType = s.EnumU8(nameof(ChangeType), u.ChangeType);
        u.Layers = s.EnumU8(nameof(Layers), u.Layers);
        u.Value = s.UInt16(nameof(Value), u.Value);
        u.MapId = MapId.SerdesU16(nameof(MapId), u.MapId, mapping, s);
        s.End();
        return u;
    }
}
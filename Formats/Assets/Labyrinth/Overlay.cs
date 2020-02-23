using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets.Labyrinth
{
    public class Overlay
    {
        public DungeonOverlayId? TextureNumber { get; set; } // 0, ushort
        public byte AnimationFrames { get; set; } // 2
        public byte WriteZero { get; set; } // 3
        public ushort YOffset { get; set; } // 4
        public ushort XOffset { get; set; } // 6
        public ushort Width { get; set; }   // 8
        public ushort Height { get; set; }  // A

        public override string ToString() =>
            $"O.{TextureNumber}:{AnimationFrames} ({XOffset}, {YOffset}) {Width}x{Height}";

        public static Overlay Serdes(int _, Overlay o, ISerializer s)
        {
            o ??= new Overlay();
            o.TextureNumber = (DungeonOverlayId?)Tweak.Serdes(nameof(o.TextureNumber), (ushort?)o.TextureNumber, s.UInt16);
            o.AnimationFrames = s.UInt8(nameof(o.AnimationFrames), o.AnimationFrames);
            o.WriteZero = s.UInt8(nameof(o.WriteZero), o.WriteZero);
            o.XOffset = s.UInt16(nameof(o.XOffset), o.XOffset);
            o.YOffset = s.UInt16(nameof(o.YOffset), o.YOffset);
            o.Width = s.UInt16(nameof(o.Width), o.Width);
            o.Height = s.UInt16(nameof(o.Height), o.Height);
            return o;
        }
    }
}

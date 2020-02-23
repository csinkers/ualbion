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
            s.Dynamic(o, nameof(o.AnimationFrames));
            s.Dynamic(o, nameof(o.WriteZero));
            s.Dynamic(o, nameof(o.XOffset));
            s.Dynamic(o, nameof(o.YOffset));
            s.Dynamic(o, nameof(o.Width));
            s.Dynamic(o, nameof(o.Height));
            return o;
        }
    }
}
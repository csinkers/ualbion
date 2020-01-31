using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets.Labyrinth
{
    public class Wall
    {
        [Flags]
        public enum WallFlags : byte
        {
            Unknown0 = 1 << 0,
            SelfIlluminating = 1 << 1,
            WriteOverlay = 1 << 2,
            Unk3 = 1 << 3,
            Unk4 = 1 << 4,
            AlphaTested = 1 << 5,
            Transparent = 1 << 6,
            SelfIlluminatingColour = 1 << 6,
        }

        public WallFlags Properties { get; set; } // 0
        public byte[] CollisionData { get; set; } // 1, len = 3 bytes
        public DungeonWallId? TextureNumber { get; set; } // 4, ushort
        public byte AnimationFrames { get; set; } // 6
        public byte AutoGfxType { get; set; }     // 7
        public byte TransparentColour { get; set; }            // 8 (PaletteId??)
        public byte Unk9 { get; set; }            // 9
        public ushort Width { get; set; }         // A
        public ushort Height { get; set; }        // C
        public IList<Overlay> Overlays { get; } = new List<Overlay>();

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

            public static void Serialize(Overlay o, ISerializer s)
            {
                s.UInt16(nameof(o.TextureNumber),
                    () => FormatUtil.Untweak((ushort?)o.TextureNumber),
                    x => o.TextureNumber = (DungeonOverlayId?)FormatUtil.Tweak(x));
                s.Dynamic(o, nameof(o.AnimationFrames));
                s.Dynamic(o, nameof(o.WriteZero));
                s.Dynamic(o, nameof(o.XOffset));
                s.Dynamic(o, nameof(o.YOffset));
                s.Dynamic(o, nameof(o.Width));
                s.Dynamic(o, nameof(o.Height));
            }
        }

        public override string ToString() =>
            $"Wall.{TextureNumber}:{AnimationFrames} {Width}x{Height} ({Properties}) [ {string.Join(", ", Overlays.Select(x => x.ToString()))} ]";

        public static void Serialize(Wall w, ISerializer s)
        {
            s.EnumU8(nameof(w.Properties), () => w.Properties, x => w.Properties = x, x => ((byte)x, x.ToString()));
            s.ByteArray(nameof(w.CollisionData), () => w.CollisionData, x => w.CollisionData = x, 3);
            s.UInt16(nameof(w.TextureNumber),
                () => FormatUtil.Untweak((ushort?)w.TextureNumber),
                x => w.TextureNumber = (DungeonWallId?)FormatUtil.Tweak(x));
            s.Dynamic(w, nameof(w.AnimationFrames));
            s.Dynamic(w, nameof(w.AutoGfxType));
            s.Dynamic(w, nameof(w.TransparentColour));
            s.Dynamic(w, nameof(w.Unk9));
            s.Dynamic(w, nameof(w.Width));
            s.Dynamic(w, nameof(w.Height));

            ushort overlayCount = (ushort)w.Overlays.Count;
            s.UInt16("overlayCount", () => overlayCount, x => overlayCount = x);
            s.List(w.Overlays, overlayCount, Overlay.Serialize, () => new Overlay());
        }
    }
}
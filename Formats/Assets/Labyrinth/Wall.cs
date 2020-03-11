using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Formats.AssetIds;

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

        public override string ToString() =>
            $"Wall.{TextureNumber}:{AnimationFrames} {Width}x{Height} ({Properties}) [ {string.Join(", ", Overlays.Select(x => x.ToString()))} ]";

        public static Wall Serdes(int _, Wall w, ISerializer s)
        {
            w ??= new Wall();
            w.Properties = s.EnumU8(nameof(w.Properties), w.Properties);
            w.CollisionData = s.ByteArray(nameof(w.CollisionData), w.CollisionData, 3);
            w.TextureNumber = (DungeonWallId?)Tweak.Serdes(nameof(w.TextureNumber), (ushort?)w.TextureNumber, s.UInt16);
            w.AnimationFrames = s.UInt8(nameof(w.AnimationFrames), w.AnimationFrames);
            w.AutoGfxType = s.UInt8(nameof(w.AutoGfxType), w.AutoGfxType);
            w.TransparentColour = s.UInt8(nameof(w.TransparentColour), w.TransparentColour);
            w.Unk9 = s.UInt8(nameof(w.Unk9), w.Unk9);
            w.Width = s.UInt16(nameof(w.Width), w.Width);
            w.Height = s.UInt16(nameof(w.Height), w.Height);

            ushort overlayCount = s.UInt16("overlayCount", (ushort)w.Overlays.Count);
            s.List(w.Overlays, overlayCount, Overlay.Serdes);
            return w;
        }
    }
}

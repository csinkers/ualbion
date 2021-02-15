using System;
using System.Collections.Generic;
using System.Linq;
using SerdesNet;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Labyrinth
{
    public class Wall
    {
        [Flags]
        public enum WallFlags : byte
        {
            Unknown0 = 1,
            SelfIlluminating = 1 << 1,
            WriteOverlay = 1 << 2,
            Unk3 = 1 << 3,
            Unk4 = 1 << 4,
            AlphaTested = 1 << 5,
            Transparent = 1 << 6,
            SelfIlluminatingColour = 1 << 6,
        }

        public WallFlags Properties { get; set; } // 0
        public uint Collision { get; set; } // 1, len = 3 bytes
        public SpriteId SpriteId { get; set; } // 4, ushort
        public byte AnimationFrames { get; set; } // 6
        public byte AutoGfxType { get; set; }     // 7
        public byte TransparentColour { get; set; }            // 8 (PaletteId??)
        public byte Unk9 { get; set; }            // 9
        public ushort Width { get; set; }         // A
        public ushort Height { get; set; }        // C
        public IList<Overlay> Overlays { get; } = new List<Overlay>();

        public override string ToString() =>
            $"Wall.{SpriteId}:{AnimationFrames} {Width}x{Height} ({Properties}) [ {string.Join(", ", Overlays.Select(x => x.ToString()))} ]";

        public static Wall Serdes(int _, Wall w, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            w ??= new Wall();
            w.Properties = s.EnumU8(nameof(w.Properties), w.Properties);

            // Either a 24 bit int or a 3 byte array, annoying to serialize either way.
            w.Collision = (w.Collision & 0xffff00) | s.UInt8(nameof(w.Collision), (byte)(w.Collision & 0xff));
            w.Collision = (w.Collision & 0xff00ff) | (uint)s.UInt8(nameof(w.Collision), (byte)((w.Collision >> 8) & 0xff)) << 8;
            w.Collision = (w.Collision & 0x00ffff) | (uint)s.UInt8(nameof(w.Collision), (byte)((w.Collision >> 16) & 0xff)) << 16;

            w.SpriteId = SpriteId.SerdesU16(nameof(w.SpriteId), w.SpriteId, AssetType.Wall, mapping, s);
            w.AnimationFrames = s.UInt8(nameof(w.AnimationFrames), w.AnimationFrames);
            w.AutoGfxType = s.UInt8(nameof(w.AutoGfxType), w.AutoGfxType);
            w.TransparentColour = s.UInt8(nameof(w.TransparentColour), w.TransparentColour);
            w.Unk9 = s.UInt8(nameof(w.Unk9), w.Unk9);
            w.Width = s.UInt16(nameof(w.Width), w.Width);
            w.Height = s.UInt16(nameof(w.Height), w.Height);

            ushort overlayCount = s.UInt16("overlayCount", (ushort)w.Overlays.Count);
            s.List(nameof(w.Overlays), w.Overlays, mapping, overlayCount, Overlay.Serdes);
            return w;
        }
    }
}

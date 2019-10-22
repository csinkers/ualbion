using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class LabyrinthData
    {
        public ushort WallHeight { get; set; }
        public ushort CameraHeight { get; set; } // EffectiveHeight = (CameraHeight << 16) + 165??
        public ushort Unk4 { get; set; }
        public DungeonBackgroundId? BackgroundId { get; set; }
        public ushort BackgroundYPosition { get; set; } // MAX(1096 - (BackgroundYPosition >> 16), 0)??
        public ushort FogDistance { get; set; } // Distance in tiles that fog begins.
        public ushort FogRed { get; set; }
        public ushort FogGreen { get; set; }
        public ushort FogBlue { get; set; }
        public byte Unk12 { get; set; }
        public byte Unk13 { get; set; }
        public byte BackgroundColour { get; set; } // Palette index
        public byte Unk15 { get; set; }
        public ushort FogMode { get; set; }
        public ushort MaxLight { get; set; }
        public ushort WallWidth { get; set; } // Effective = (1 << WallWidth), always between 7 & 10 (?)
        public int EffectiveWallWidth => 1 << WallWidth; // Effective = (1 << WallWidth), always between 7 & 10 (?)
        public ushort BackgroundTileAmount { get; set; }
        public ushort MaxVisibleTiles { get; set; }
        public ushort Unk20 { get; set; }
        public ushort Lighting { get; set; }
        public ushort Unk24 { get; set; }
        public IList<ObjectGroup> ObjectGroups { get; } = new List<ObjectGroup>();
        public IList<Object> Objects { get; } = new List<Object>();
        public IList<FloorAndCeiling> FloorAndCeilings { get; } = new List<FloorAndCeiling>();
        public IList<Wall> Walls { get; } = new List<Wall>();

        public class ObjectGroup
        {
            public ushort AutoGraphicsId { get; set; }
            public IList<SubObject> SubObjects { get; } = new List<SubObject>();

            public override string ToString() =>
                $"Obj: AG{AutoGraphicsId} [ {string.Join("; ", SubObjects.Select(x => x.ToString()))} ]";
        }

        public class SubObject
        {
            public int X;
            public int Y;
            public int Z;
            public int ObjectInfoNumber;
            public override string ToString() => $"{ObjectInfoNumber}({ObjectId}) @ ({X}, {Y}, {Z})";
            internal DungeonObjectId? ObjectId;
        }

        public class FloorAndCeiling
        {
            [Flags]
            public enum FcFlags : byte
            {
                Unknown0               = 1 << 0,
                SelfIlluminating       = 1 << 1,
                NotWalkable            = 1 << 2,
                Unknown3               = 1 << 3,
                Unknown4               = 1 << 4,
                Walkable               = 1 << 5,
                Grayed                 = 1 << 6,
                SelfIlluminatingColour = 1 << 7,
            }

            public FcFlags Properties;
            public byte Unk1;
            public byte Unk2;
            public byte Unk3;
            public byte AnimationCount;
            public byte Unk5;
            public DungeonFloorId? TextureNumber; // ushort
            public ushort Unk8;
            public override string ToString() => $"FC.{TextureNumber}:{AnimationCount} {Properties}";
        }

        public class Object
        {
            [Flags]
            public enum ObjectFlags : byte
            {
                Unk0 = 1 << 0,
                Unk1 = 1 << 1,
                FloorObject = 1 << 2,
                Unk3 = 1 << 3,
                Unk4 = 1 << 4,
                Unk5 = 1 << 5,
                Unk6 = 1 << 6,
                Unk7 = 1 << 7,
            }

            public ObjectFlags Properties; // 0
            public byte[] CollisionData; // 1, len = 3 bytes
            public DungeonObjectId? TextureNumber; // 4, ushort
            public byte AnimationFrames; // 6
            public byte Unk7; // 7
            public ushort Width; // 8
            public ushort Height; // A
            public ushort MapWidth; // C
            public ushort MapHeight; // E

            public override string ToString() =>
                $"EO.{TextureNumber}:{AnimationFrames} {Width}x{Height} [{MapWidth}x{MapHeight}] {Properties}";
        }

        public class Wall
        {
            [Flags]
            public enum WallFlags : byte
            {
                Unknown0               = 1 << 0,
                SelfIlluminating       = 1 << 1,
                WriteOverlay           = 1 << 2,
                Unk3                   = 1 << 3,
                Unk4                   = 1 << 4,
                AlphaTested            = 1 << 5,
                Transparent            = 1 << 6,
                SelfIlluminatingColour = 1 << 6,
            }

            public WallFlags Properties; // 0
            public byte[] CollisionData; // 1, len = 3 bytes
            public DungeonWallId? TextureNumber; // 4, ushort
            public byte AnimationFrames; // 6
            public byte AutoGfxType;     // 7
            public byte TransparentColour;            // 8 (PaletteId??)
            public byte Unk9;            // 9
            public ushort Width;         // A
            public ushort Height;        // C
            public IList<Overlay> Overlays = new List<Overlay>();

            public class Overlay
            {
                public DungeonOverlayId? TextureNumber; // 0, ushort
                public byte AnimationFrames; // 2
                public byte WriteZero; // 3
                public ushort YOffset; // 4
                public ushort XOffset; // 6
                public ushort Width;   // 8
                public ushort Height;  // A

                public override string ToString() =>
                    $"O.{TextureNumber}:{AnimationFrames} ({XOffset}, {YOffset}) {Width}x{Height}";
            }

            public override string ToString() =>
                $"Wall.{TextureNumber}:{AnimationFrames} {Width}x{Height} ({Properties}) [ {string.Join(", ", Overlays.Select(x => x.ToString()))} ]";
        }
    }
}

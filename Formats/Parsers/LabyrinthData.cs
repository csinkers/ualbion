using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
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

    [AssetLoader(XldObjectType.LabyrinthData)]
    public class LabyrinthDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetConfig.Asset config)
        {
            var startOffset = br.BaseStream.Position;

            var l = new LabyrinthData();
            l.WallHeight = br.ReadUInt16(); // 0
            l.CameraHeight = br.ReadUInt16(); // 2
            l.Unk4 = br.ReadUInt16(); // 4
            ushort backgroundId = br.ReadUInt16(); // 6
            l.BackgroundId = backgroundId == 0 ? null : (DungeonBackgroundId?)(backgroundId - 1);
            l.BackgroundYPosition = br.ReadUInt16(); // 8
            l.FogDistance = br.ReadUInt16(); // A
            l.FogRed = br.ReadUInt16(); // C
            l.FogGreen = br.ReadUInt16(); // E
            l.FogBlue = br.ReadUInt16(); // 10
            l.Unk12 = br.ReadByte(); // 12
            l.Unk13 = br.ReadByte(); // 13
            l.BackgroundColour = br.ReadByte(); // 14
            l.Unk15 = br.ReadByte(); // 15
            l.FogMode = br.ReadUInt16(); // 16
            l.MaxLight = br.ReadUInt16(); // 18
            l.WallWidth = br.ReadUInt16(); // 1A
            l.BackgroundTileAmount = br.ReadUInt16(); // 1C
            l.MaxVisibleTiles = br.ReadUInt16(); // 1E
            l.Unk20 = br.ReadUInt16(); // 20
            l.Lighting = br.ReadUInt16(); // 22
            l.Unk24 = br.ReadUInt16(); // 24

            Debug.Assert( br.BaseStream.Position <= startOffset + streamLength);

            int objectGroupCount = br.ReadUInt16(); // 26
            for (int i = 0; i < objectGroupCount; i++)
            {
                var og = new LabyrinthData.ObjectGroup();
                og.AutoGraphicsId = br.ReadUInt16(); // +0
                for (int n = 0; n < 8; n++)
                {
                    var so = new LabyrinthData.SubObject();
                    so.X = br.ReadInt16();
                    so.Z = br.ReadInt16();
                    so.Y = br.ReadInt16();
                    so.ObjectInfoNumber = br.ReadUInt16();
                    if (so.ObjectInfoNumber != 0)
                    {
                        so.ObjectInfoNumber--;
                        og.SubObjects.Add(so);
                    }
                } // +64

                l.ObjectGroups.Add(og);
            }
            Debug.Assert( br.BaseStream.Position <= startOffset + streamLength);

            int floorAndCeilingCount = br.ReadUInt16(); // 28 + objectGroupCount * 42
            for (int i = 0; i < floorAndCeilingCount; i++)
            {
                var fc = new LabyrinthData.FloorAndCeiling();
                fc.Properties = (LabyrinthData.FloorAndCeiling.FcFlags)br.ReadByte();
                fc.Unk1 = br.ReadByte();
                fc.Unk2 = br.ReadByte();
                fc.Unk3 = br.ReadByte();
                fc.AnimationCount = br.ReadByte();
                fc.Unk5 = br.ReadByte();

                ushort textureNumber = br.ReadUInt16();
                if (textureNumber == 0)
                    fc.TextureNumber = null;
                else if (textureNumber < 100)
                    fc.TextureNumber = (DungeonFloorId)(textureNumber - 1);
                else
                    fc.TextureNumber = (DungeonFloorId)textureNumber;

                fc.Unk8 = br.ReadUInt16();
                l.FloorAndCeilings.Add(fc);
            } 
            Debug.Assert( br.BaseStream.Position <= startOffset + streamLength);

            int objectCount = br.ReadUInt16(); // 2A + objectGroupCount * 42 + floorAndCeilingCount * A
            for(int i = 0; i < objectCount; i++)
            {
                var o = new LabyrinthData.Object();
                o.Properties = (LabyrinthData.Object.ObjectFlags)br.ReadByte();
                o.CollisionData = br.ReadBytes(3);

                ushort textureNumber = br.ReadUInt16();
                if (textureNumber == 0)
                    o.TextureNumber = null;
                else if (textureNumber < 100)
                    o.TextureNumber = (DungeonObjectId)(textureNumber - 1);
                else
                    o.TextureNumber = (DungeonObjectId)textureNumber;

                o.AnimationFrames = br.ReadByte();
                o.Unk7 = br.ReadByte();
                o.Width = br.ReadUInt16();
                o.Height = br.ReadUInt16();
                o.MapWidth = br.ReadUInt16();
                o.MapHeight = br.ReadUInt16();
                l.Objects.Add(o);
            }
            Debug.Assert( br.BaseStream.Position <= startOffset + streamLength);

            // Populate objectIds on subobjects to improve debugging experience
            foreach (var so in l.ObjectGroups.SelectMany(x => x.SubObjects))
            {
                if (so.ObjectInfoNumber >= l.Objects.Count)
                    continue;
                so.ObjectId = l.Objects[so.ObjectInfoNumber].TextureNumber;
            }

            int wallCount = br.ReadUInt16();
            for (int i = 0; i < wallCount; i++)
            {
                var w = new LabyrinthData.Wall();
                w.Properties = (LabyrinthData.Wall.WallFlags) br.ReadByte();
                w.CollisionData = br.ReadBytes(3);

                ushort textureNumber = br.ReadUInt16();
                if (textureNumber == 0)
                    w.TextureNumber = null;
                else if (textureNumber < 100)
                    w.TextureNumber = (DungeonWallId)(textureNumber - 1);
                else
                    w.TextureNumber = (DungeonWallId)textureNumber;

                w.AnimationFrames = br.ReadByte();
                w.AutoGfxType = br.ReadByte();
                w.TransparentColour = br.ReadByte();
                w.Unk9 = br.ReadByte();
                w.Width = br.ReadUInt16();
                w.Height = br.ReadUInt16();
                int overlayCount = br.ReadUInt16();
                for (int j = 0; j < overlayCount; j++)
                {
                    var o = new LabyrinthData.Wall.Overlay();

                    textureNumber = br.ReadUInt16();
                    if (textureNumber == 0)
                        o.TextureNumber = null;
                    else if (textureNumber < 100)
                        o.TextureNumber = (DungeonOverlayId)(textureNumber - 1);
                    else
                        o.TextureNumber = (DungeonOverlayId)textureNumber;

                    o.AnimationFrames = br.ReadByte();
                    o.WriteZero = br.ReadByte();
                    o.XOffset = br.ReadUInt16();
                    o.YOffset = br.ReadUInt16();
                    o.Width = br.ReadUInt16();
                    o.Height = br.ReadUInt16();
                    w.Overlays.Add(o);
                }

                l.Walls.Add(w);
            }
            Debug.Assert( br.BaseStream.Position <= startOffset + streamLength);

            return l;
        }
    }
}

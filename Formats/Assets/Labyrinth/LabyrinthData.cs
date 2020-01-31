using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets.Labyrinth
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

        public static void Serialize(LabyrinthData d, ISerializer s, long length)
        {
            PerfTracker.StartupEvent("Start loading labyrinth data");
            var start = s.Offset;
            // s.ByteArray("UnknownBlock6C", () => sheet.UnknownBlock6C, x => sheet.UnknownBlock6C = x, 14);

            s.Dynamic(d, nameof(d.WallHeight));           // 0
            s.Dynamic(d, nameof(d.CameraHeight));         // 2
            s.Dynamic(d, nameof(d.Unk4));                 // 4

            s.UInt16(nameof(d.BackgroundId),
                () => FormatUtil.Untweak((ushort?)d.BackgroundId),
                x => d.BackgroundId = (DungeonBackgroundId?)FormatUtil.Tweak(x));  // 6

            s.Dynamic(d, nameof(d.BackgroundYPosition));  // 8
            s.Dynamic(d, nameof(d.FogDistance));          // A
            s.Dynamic(d, nameof(d.FogRed));               // C
            s.Dynamic(d, nameof(d.FogGreen));             // E
            s.Dynamic(d, nameof(d.FogBlue));              // 10
            s.Dynamic(d, nameof(d.Unk12));                // 12
            s.Dynamic(d, nameof(d.Unk13));                // 13
            s.Dynamic(d, nameof(d.BackgroundColour));     // 14
            s.Dynamic(d, nameof(d.Unk15));                // 15
            s.Dynamic(d, nameof(d.FogMode));              // 16
            s.Dynamic(d, nameof(d.MaxLight));             // 18
            s.Dynamic(d, nameof(d.WallWidth));            // 1A
            s.Dynamic(d, nameof(d.BackgroundTileAmount)); // 1C
            s.Dynamic(d, nameof(d.MaxVisibleTiles));      // 1E
            s.Dynamic(d, nameof(d.Unk20));                // 20
            s.Dynamic(d, nameof(d.Lighting));             // 22
            s.Dynamic(d, nameof(d.Unk24));                // 24
            s.Check();

            Debug.Assert(s.Offset - start <= length);

            ushort objectGroupCount = (ushort)d.ObjectGroups.Count; // 26
            s.UInt16("ObjectGroupCount", () => (ushort)d.ObjectGroups.Count, x => objectGroupCount = x);
            s.List(d.ObjectGroups, objectGroupCount, ObjectGroup.Serialize, () => new ObjectGroup());
            Debug.Assert(s.Offset - start <= length);

            var floorAndCeilingCount = (ushort)d.FloorAndCeilings.Count; // 28 + objectGroupCount * 42
            s.UInt16("FloorAndCeilingCount", () => floorAndCeilingCount, x => floorAndCeilingCount = x);
            s.List(d.FloorAndCeilings, floorAndCeilingCount, FloorAndCeiling.Serialize, () => new FloorAndCeiling	());
            Debug.Assert(s.Offset - start <= length);

            ushort objectCount = (ushort)d.Objects.Count; // 2A + objectGroupCount * 42 + floorAndCeilingCount * A
            s.UInt16("ObjectCount", () => objectCount, x => objectCount = x);
            s.List(d.Objects, objectCount, Object.Serialize, () => new Object());
            Debug.Assert(s.Offset - start <= length);

            // Populate objectIds on subobjects to improve debugging experience
            foreach (var so in d.ObjectGroups.SelectMany(x => x.SubObjects))
            {
                if (so.ObjectInfoNumber >= d.Objects.Count)
                    continue;
                so.ObjectId = d.Objects[so.ObjectInfoNumber].TextureNumber;
            }

            ushort wallCount = (ushort)d.Walls.Count;
            s.UInt16("WallCount", () => wallCount, x => wallCount = x);
            s.List(d.Walls, wallCount, Wall.Serialize, () => new Wall());
            Debug.Assert(s.Offset - start <= length);
            PerfTracker.StartupEvent("Finish loading labyrinth data");
        }
    }
}

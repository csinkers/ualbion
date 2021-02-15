using System;
using System.Collections.Generic;
using SerdesNet;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Labyrinth
{
    public class LabyrinthData
    {
        public ushort WallHeight { get; set; }
        public ushort CameraHeight { get; set; } // EffectiveHeight = (CameraHeight << 16) + 165??
        public ushort Unk4 { get; set; }
        public SpriteId BackgroundId { get; set; }
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
        public IList<LabyrinthObject> Objects { get; } = new List<LabyrinthObject>();
        public IList<FloorAndCeiling> FloorAndCeilings { get; } = new List<FloorAndCeiling>();
        public IList<Wall> Walls { get; } = new List<Wall>();

        public static LabyrinthData Serdes(int _, LabyrinthData d, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            d ??= new LabyrinthData();
            PerfTracker.StartupEvent("Start loading labyrinth data");
            // s.ByteArray("UnknownBlock6C", () => sheet.UnknownBlock6C, x => sheet.UnknownBlock6C = x, 14);

            d.WallHeight   = s.UInt16(nameof(d.WallHeight), d.WallHeight);       // 0
            d.CameraHeight = s.UInt16(nameof(d.CameraHeight), d.CameraHeight); // 2
            d.Unk4         = s.UInt16(nameof(d.Unk4), d.Unk4);                         // 4
            d.BackgroundId = SpriteId.SerdesU16(nameof(BackgroundId), d.BackgroundId, AssetType.BackgroundGraphics, mapping, s); // 6
            d.BackgroundYPosition  = s.UInt16(nameof(d.BackgroundYPosition), d.BackgroundYPosition);   // 8
            d.FogDistance          = s.UInt16(nameof(d.FogDistance), d.FogDistance);                   // A
            d.FogRed               = s.UInt16(nameof(d.FogRed), d.FogRed);                             // C
            d.FogGreen             = s.UInt16(nameof(d.FogGreen), d.FogGreen);                         // E
            d.FogBlue              = s.UInt16(nameof(d.FogBlue), d.FogBlue);                           // 10
            d.Unk12                = s.UInt8(nameof(d.Unk12), d.Unk12);                                // 12
            d.Unk13                = s.UInt8(nameof(d.Unk13), d.Unk13);                                // 13
            d.BackgroundColour     = s.UInt8(nameof(d.BackgroundColour), d.BackgroundColour);          // 14
            d.Unk15                = s.UInt8(nameof(d.Unk15), d.Unk15);                                // 15
            d.FogMode              = s.UInt16(nameof(d.FogMode), d.FogMode);                           // 16
            d.MaxLight             = s.UInt16(nameof(d.MaxLight), d.MaxLight);                         // 18
            d.WallWidth            = s.UInt16(nameof(d.WallWidth), d.WallWidth);                       // 1A
            d.BackgroundTileAmount = s.UInt16(nameof(d.BackgroundTileAmount), d.BackgroundTileAmount); // 1C
            d.MaxVisibleTiles      = s.UInt16(nameof(d.MaxVisibleTiles), d.MaxVisibleTiles);           // 1E
            d.Unk20                = s.UInt16(nameof(d.Unk20), d.Unk20);                               // 20
            d.Lighting             = s.UInt16(nameof(d.Lighting), d.Lighting);                         // 22
            d.Unk24                = s.UInt16(nameof(d.Unk24), d.Unk24);                               // 24
            s.Check();

            ushort objectGroupCount = s.UInt16("ObjectGroupCount", (ushort)d.ObjectGroups.Count); // 26
            s.List(nameof(d.ObjectGroups), d.ObjectGroups, objectGroupCount, ObjectGroup.Serdes);
            s.Check();

            var floorAndCeilingCount = s.UInt16("FloorAndCeilingCount", (ushort)d.FloorAndCeilings.Count); // 28 + objectGroupCount * 42
            s.List(nameof(d.FloorAndCeilings), d.FloorAndCeilings, mapping, floorAndCeilingCount, FloorAndCeiling.Serdes);
            s.Check();

            ushort objectCount = s.UInt16("ObjectCount", (ushort)d.Objects.Count); // 2A + objectGroupCount * 42 + floorAndCeilingCount * A
            s.List(nameof(d.Objects), d.Objects, mapping, objectCount, LabyrinthObject.Serdes);
            s.Check();

            // Populate objectIds on subobjects to improve debugging experience
            foreach (var so in d.ObjectGroups.SelectMany(x => x.SubObjects))
            {
                if (so == null || so.ObjectInfoNumber >= d.Objects.Count) continue;
                so.SpriteId = d.Objects[so.ObjectInfoNumber].SpriteId;
            }

            ushort wallCount = s.UInt16("WallCount", (ushort)d.Walls.Count);
            s.List(nameof(d.Walls), d.Walls, mapping, wallCount, Wall.Serdes);
            s.Check();
            PerfTracker.StartupEvent("Finish loading labyrinth data");
            return d;
        }
    }
}

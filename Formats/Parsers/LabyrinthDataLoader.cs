using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.LabyrinthData)]
    public class LabyrinthDataLoader : IAssetLoader
    {
        static void Translate(LabyrinthData d, ISerializer s, long length)
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
            for (int i = 0; i < objectGroupCount; i++)
            {
                var og = s.Mode == SerializerMode.Reading 
                    ? new LabyrinthData.ObjectGroup()
                    : d.ObjectGroups[i];

                s.Dynamic(og, nameof(og.AutoGraphicsId));

                for (int n = 0; n < 8; n++)
                {
                    var so = og.SubObjects.Count <= n
                        ? new LabyrinthData.SubObject()
                        : og.SubObjects[n];

                    if (og.SubObjects.Count > n)
                        so.ObjectInfoNumber++;

                    s.Dynamic(so, nameof(so.X));
                    s.Dynamic(so, nameof(so.Z));
                    s.Dynamic(so, nameof(so.Y));
                    s.Dynamic(so, nameof(so.ObjectInfoNumber));

                    if (so.ObjectInfoNumber != 0)
                    {
                        so.ObjectInfoNumber--;
                        if (og.SubObjects.Count <= n)
                            og.SubObjects.Add(so);
                    }
                } // +64

                if (d.ObjectGroups.Count <= i)
                    d.ObjectGroups.Add(og);
            }
            Debug.Assert(s.Offset - start <= length);

            var floorAndCeilingCount = (ushort)d.FloorAndCeilings.Count; // 28 + objectGroupCount * 42
            s.UInt16("FloorAndCeilingCount", () => floorAndCeilingCount, x => floorAndCeilingCount = x);
            for (int i = 0; i < floorAndCeilingCount; i++)
            {
                var fc = s.Mode == SerializerMode.Reading 
                    ? new LabyrinthData.FloorAndCeiling()
                    : d.FloorAndCeilings[i];

                s.EnumU8(nameof(fc.Properties), () => fc.Properties, x => fc.Properties = x, x => ((byte)x, x.ToString()));
                s.Dynamic(fc, nameof(fc.Unk1));
                s.Dynamic(fc, nameof(fc.Unk2));
                s.Dynamic(fc, nameof(fc.Unk3));
                s.Dynamic(fc, nameof(fc.AnimationCount));
                s.Dynamic(fc, nameof(fc.Unk5));
                s.UInt16(nameof(fc.TextureNumber),
                    () => FormatUtil.Untweak((ushort?)fc.TextureNumber),
                    x => fc.TextureNumber = (DungeonFloorId?)FormatUtil.Tweak(x));

                s.Dynamic(fc, nameof(fc.Unk8));
                if(d.FloorAndCeilings.Count <= i)
                    d.FloorAndCeilings.Add(fc);
            }
            Debug.Assert(s.Offset - start <= length);

            ushort objectCount = (ushort)d.Objects.Count; // 2A + objectGroupCount * 42 + floorAndCeilingCount * A
            s.UInt16("ObjectCount", () => objectCount, x => objectCount = x);
            for (int i = 0; i < objectCount; i++)
            {
                var o = s.Mode == SerializerMode.Reading 
                    ? new LabyrinthData.Object()
                    : d.Objects[i];

                s.EnumU8(nameof(o.Properties), () => o.Properties, x => o.Properties = x, x => ((byte)x, x.ToString()));
                s.ByteArray(nameof(o.CollisionData), () => o.CollisionData, x => o.CollisionData = x, 3);
                s.UInt16(nameof(o.TextureNumber), 
                    () => FormatUtil.Untweak((ushort?)o.TextureNumber),
                    x => o.TextureNumber = (DungeonObjectId?)FormatUtil.Tweak(x));
                s.Dynamic(o, nameof(o.AnimationFrames));
                s.Dynamic(o, nameof(o.Unk7));
                s.Dynamic(o, nameof(o.Width));
                s.Dynamic(o, nameof(o.Height));
                s.Dynamic(o, nameof(o.MapWidth));
                s.Dynamic(o, nameof(o.MapHeight));

                if (d.Objects.Count <= i)
                    d.Objects.Add(o);
            }
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
            for (int i = 0; i < wallCount; i++)
            {
                var w = s.Mode == SerializerMode.Reading 
                    ? new LabyrinthData.Wall()
                    : d.Walls[i];

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
                for (int j = 0; j < overlayCount; j++)
                {
                    var o = s.Mode == SerializerMode.Reading 
                        ? new LabyrinthData.Wall.Overlay()
                        : w.Overlays[i];

                    s.UInt16(nameof(o.TextureNumber), 
                        () => FormatUtil.Untweak((ushort?)o.TextureNumber),
                        x => o.TextureNumber = (DungeonOverlayId?)FormatUtil.Tweak(x));
                    s.Dynamic(o, nameof(o.AnimationFrames));
                    s.Dynamic(o, nameof(o.WriteZero));
                    s.Dynamic(o, nameof(o.XOffset));
                    s.Dynamic(o, nameof(o.YOffset));
                    s.Dynamic(o, nameof(o.Width));
                    s.Dynamic(o, nameof(o.Height));

                    if (w.Overlays.Count <= j)
                        w.Overlays.Add(o);
                }

                if (d.Walls.Count <= i)
                    d.Walls.Add(w);
            }
            Debug.Assert(s.Offset - start <= length);
            PerfTracker.StartupEvent("Finish loading labyrinth data");
        }

        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var labyrinth = new LabyrinthData();
            Translate(labyrinth, new GenericBinaryReader(br), streamLength);
            return labyrinth;
        }
    }
}

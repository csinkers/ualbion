using System.Diagnostics;
using System.IO;
using System.Linq;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.LabyrinthData)]
    public class LabyrinthDataLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var startOffset = br.BaseStream.Position;

            var l = new LabyrinthData
            { 
                WallHeight = br.ReadUInt16(), // 0
                CameraHeight = br.ReadUInt16(), // 2
                Unk4 = br.ReadUInt16(), // 4
                BackgroundId = (DungeonBackgroundId?)FormatUtil.Tweak(br.ReadUInt16()), // 6
                BackgroundYPosition = br.ReadUInt16(), // 8
                FogDistance = br.ReadUInt16(), // A
                FogRed = br.ReadUInt16(), // C
                FogGreen = br.ReadUInt16(), // E
                FogBlue = br.ReadUInt16(), // 10
                Unk12 = br.ReadByte(), // 12
                Unk13 = br.ReadByte(), // 13
                BackgroundColour = br.ReadByte(), // 14
                Unk15 = br.ReadByte(), // 15
                FogMode = br.ReadUInt16(), // 16
                MaxLight = br.ReadUInt16(), // 18
                WallWidth = br.ReadUInt16(), // 1A
                BackgroundTileAmount = br.ReadUInt16(), // 1C
                MaxVisibleTiles = br.ReadUInt16(), // 1E
                Unk20 = br.ReadUInt16(), // 20
                Lighting = br.ReadUInt16(), // 22
                Unk24 = br.ReadUInt16(), // 24
            };

            Debug.Assert( br.BaseStream.Position <= startOffset + streamLength);

            int objectGroupCount = br.ReadUInt16(); // 26
            for (int i = 0; i < objectGroupCount; i++)
            {
                var og = new LabyrinthData.ObjectGroup
                {
                    AutoGraphicsId = br.ReadUInt16() // +0
                };
                for (int n = 0; n < 8; n++)
                {
                    var so = new LabyrinthData.SubObject
                    {
                        X = br.ReadInt16(),
                        Z = br.ReadInt16(),
                        Y = br.ReadInt16(),
                        ObjectInfoNumber = br.ReadUInt16()
                    };
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
                var fc = new LabyrinthData.FloorAndCeiling
                {
                    Properties = (LabyrinthData.FloorAndCeiling.FcFlags) br.ReadByte(),
                    Unk1 = br.ReadByte(),
                    Unk2 = br.ReadByte(),
                    Unk3 = br.ReadByte(),
                    AnimationCount = br.ReadByte(),
                    Unk5 = br.ReadByte(),
                    TextureNumber = (DungeonFloorId?) FormatUtil.Tweak(br.ReadUInt16()),
                    Unk8 = br.ReadUInt16()
                };
                l.FloorAndCeilings.Add(fc);
            } 
            Debug.Assert( br.BaseStream.Position <= startOffset + streamLength);

            int objectCount = br.ReadUInt16(); // 2A + objectGroupCount * 42 + floorAndCeilingCount * A
            for(int i = 0; i < objectCount; i++)
            {
                var o = new LabyrinthData.Object
                {
                    Properties = (LabyrinthData.Object.ObjectFlags) br.ReadByte(),
                    CollisionData = br.ReadBytes(3),
                    TextureNumber = (DungeonObjectId?) FormatUtil.Tweak(br.ReadUInt16()),
                    AnimationFrames = br.ReadByte(),
                    Unk7 = br.ReadByte(),
                    Width = br.ReadUInt16(),
                    Height = br.ReadUInt16(),
                    MapWidth = br.ReadUInt16(),
                    MapHeight = br.ReadUInt16()
                };
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
                var w = new LabyrinthData.Wall
                {
                    Properties = (LabyrinthData.Wall.WallFlags) br.ReadByte(),
                    CollisionData = br.ReadBytes(3),
                    TextureNumber = (DungeonWallId?) FormatUtil.Tweak(br.ReadUInt16()),
                    AnimationFrames = br.ReadByte(),
                    AutoGfxType = br.ReadByte(),
                    TransparentColour = br.ReadByte(),
                    Unk9 = br.ReadByte(),
                    Width = br.ReadUInt16(),
                    Height = br.ReadUInt16()
                };

                int overlayCount = br.ReadUInt16();
                for (int j = 0; j < overlayCount; j++)
                {
                    var o = new LabyrinthData.Wall.Overlay
                    {
                        TextureNumber = (DungeonOverlayId?) FormatUtil.Tweak(br.ReadUInt16()),
                        AnimationFrames = br.ReadByte(),
                        WriteZero = br.ReadByte(),
                        XOffset = br.ReadUInt16(),
                        YOffset = br.ReadUInt16(),
                        Width = br.ReadUInt16(),
                        Height = br.ReadUInt16()
                    };
                    w.Overlays.Add(o);
                }
                l.Walls.Add(w);
            }
            Debug.Assert( br.BaseStream.Position <= startOffset + streamLength);

            return l;
        }
    }
}

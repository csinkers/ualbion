using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;

namespace UAlbion.Game.Entities.Map3D
{
    public class IsometricLayout : Component
    {
        DungeonTilemap _tilemap;
        byte[] _contents;
        byte[] _floors;
        byte[] _ceilings;
        int _wallCount;

        public DungeonTileMapProperties Properties
        {
            get => _tilemap.Properties;
            set => _tilemap.Properties = value;
        }

        public int TileCount => _tilemap?.Tiles.Length ?? 0;
        public List<int>[] FloorFrames { get; private set; }
        public List<int>[] CeilingFrames { get; private set; }
        public List<int>[] WallFrames { get; private set; }
        public List<int>[] ContentsFrames { get; private set; }

        protected override void Subscribed()
        {
            if (_tilemap != null)
                Resolve<IEngine>()?.RegisterRenderable(_tilemap);
        }

        public void Load(LabyrinthId labyrinthId, IsometricMode mode, DungeonTileMapProperties properties, int? paletteId)
        {
            var assets = Resolve<IAssetManager>();
            var labyrinthData = assets.LoadLabyrinthData(labyrinthId);
            var info = assets.GetAssetInfo(labyrinthId);
            if (labyrinthData == null || info == null)
                return;

            Load(labyrinthData, info, mode, properties, paletteId, assets);
        }

        public void Load(LabyrinthData labyrinthData, AssetInfo info, IsometricMode mode, DungeonTileMapProperties properties, int? paletteId, IAssetManager assets)
        {
            if (labyrinthData == null) throw new ArgumentNullException(nameof(labyrinthData));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (assets == null) throw new ArgumentNullException(nameof(assets));

            var engine = Resolve<IEngine>();
            var coreFactory = Resolve<ICoreFactory>();

            RemoveAllChildren();
            if (_tilemap != null)
                engine.UnregisterRenderable(_tilemap);

            bool floors = mode == IsometricMode.Floors || mode == IsometricMode.All;
            bool ceilings = mode == IsometricMode.Ceilings || mode == IsometricMode.All;
            bool walls = mode == IsometricMode.Walls || mode == IsometricMode.All;
            bool contents = mode == IsometricMode.Contents || mode == IsometricMode.All;

            paletteId ??= info.Get(AssetProperty.PaletteId, 0);
            var palette = assets.LoadPalette(new PaletteId(AssetType.Palette, paletteId.Value));
            if (palette == null)
            {
                Raise(new LogEvent(LogEvent.Level.Error, $"Could not load palette {paletteId}"));
                palette = assets.LoadPalette(Base.Palette.Common);
            }

            _tilemap = new DungeonTilemap(labyrinthData.Id, labyrinthData.Id.ToString(), 0, properties, coreFactory, palette, null)
            {
                PipelineId = (int)DungeonTilemapPipeline.NoCulling
            };

            // Layout:
            // [Empty] [First frame of all floors] [First frame of all ceilings] [First frame of all walls] [Additional floor frames] [Additional ceiling frames] [Additional wall frames]

            int totalTiles = 1;
            if (floors || ceilings)
            {
                for (int i = 0; i < labyrinthData.FloorAndCeilings.Count; i++)
                {
                    var floorInfo = labyrinthData.FloorAndCeilings[i];
                    var floor = floorInfo == null ? null : assets.LoadTexture(floorInfo.SpriteId);
                    _tilemap.DefineFloor(i + 1, floor);
                    if (floors) totalTiles += _tilemap.DayFloors.GetFrameCountForLogicalId(i + 1);
                    if (ceilings) totalTiles += _tilemap.DayFloors.GetFrameCountForLogicalId(i + 1);
                }
            }

            if (walls)
            {
                for (int i = 0; i < labyrinthData.Walls.Count; i++)
                {
                    var wallInfo = labyrinthData.Walls[i];
                    var wall = wallInfo == null ? null : assets.LoadTexture(wallInfo.SpriteId);
                    bool isAlphaTested = wallInfo != null && (wallInfo.Properties & Wall.WallFlags.AlphaTested) != 0;
                    _tilemap.DefineWall(i + 1, wall, 0, 0, wallInfo?.TransparentColour ?? 0, isAlphaTested);

                    foreach (var overlayInfo in wallInfo?.Overlays ?? Array.Empty<Overlay>())
                    {
                        if (overlayInfo.SpriteId.IsNone)
                            continue;

                        var overlay = assets.LoadTexture(overlayInfo.SpriteId);
                        _tilemap.DefineWall(i + 1, overlay, overlayInfo.XOffset, overlayInfo.YOffset,
                            wallInfo?.TransparentColour ?? 0, isAlphaTested);
                    }

                    totalTiles += _tilemap.DayWalls.GetFrameCountForLogicalId(i + 1);
                }
            }

            if (contents)
                totalTiles += labyrinthData.ObjectGroups.Count; // TODO: Object frames

            _wallCount = labyrinthData.Walls.Count;
            _floors = new byte[totalTiles];
            _ceilings = new byte[totalTiles];
            _contents = new byte[totalTiles];
            var frames = new int[totalTiles];

            int index = 1;

            // Add initial frames
            if (floors)
            {
                FloorFrames = new List<int>[labyrinthData.FloorAndCeilings.Count + 1];
                FloorFrames[0] = new List<int> { 0 };
                for (byte i = 1; i <= labyrinthData.FloorAndCeilings.Count; i++)
                {
                    _floors[index] = i;
                    FloorFrames[i] = new List<int> { index };
                    index++;
                }
            }

            if (ceilings)
            {
                CeilingFrames = new List<int>[labyrinthData.FloorAndCeilings.Count + 1];
                CeilingFrames[0] = new List<int> { 0 };
                for (byte i = 1; i <= labyrinthData.FloorAndCeilings.Count; i++)
                {
                    _ceilings[index] = i;
                    CeilingFrames[i] = new List<int> { index };
                    index++;
                }
            }

            if (walls)
            {
                WallFrames = new List<int>[labyrinthData.Walls.Count + 1];
                WallFrames[0] = new List<int> { 0 };
                for (byte i = 1; i <= labyrinthData.Walls.Count; i++)
                {
                    _contents[index] = (byte)(i + 100);
                    WallFrames[i] = new List<int> { index };
                    index++;
                }
            }

            if (contents)
            {
                ContentsFrames = new List<int>[labyrinthData.ObjectGroups.Count + 1];
                ContentsFrames[0] = new List<int> { 0 };
                for (byte i = 1; i <= labyrinthData.ObjectGroups.Count; i++)
                {
                    _contents[index] = i;
                    ContentsFrames[i] = new List<int> { index };
                    index++;
                }
            }

            // Add animation frames
            if (floors)
            {
                for (byte i = 1; i <= labyrinthData.FloorAndCeilings.Count; i++)
                {
                    int frameCount = _tilemap.DayFloors.GetFrameCountForLogicalId(i);
                    for (int j = 1; j < frameCount; j++)
                    {
                        _floors[index] = i;
                        FloorFrames[i].Add(index);
                        frames[index++] = j;
                    }
                }
            }

            if (ceilings)
            {
                for (byte i = 1; i <= labyrinthData.FloorAndCeilings.Count; i++)
                {
                    int frameCount = _tilemap.DayFloors.GetFrameCountForLogicalId(i);
                    for (int j = 1; j < frameCount; j++)
                    {
                        _ceilings[index] = i;
                        CeilingFrames[i].Add(index);
                        frames[index++] = j;
                    }
                }
            }

            if (walls)
            {
                for (byte i = 1; i <= labyrinthData.Walls.Count; i++)
                {
                    int frameCount = _tilemap.DayWalls.GetFrameCountForLogicalId(i);
                    for (int j = 1; j < frameCount; j++)
                    {
                        _contents[index] = (byte)(i + 100);
                        WallFrames[i].Add(index);
                        frames[index++] = j;
                    }
                }
            }

            _tilemap.Resize(totalTiles);
            for (int i = 0; i < totalTiles; i++)
                SetTile(i, i, frames[i]);

            engine.RegisterRenderable(_tilemap);
        }

        protected override void Unsubscribed()
        {
            if (_tilemap != null)
                Resolve<IEngine>().UnregisterRenderable(_tilemap);
        }

        void SetTile(int index, int order, int frameCount)
        {
            byte floorIndex = _floors[index];
            byte ceilingIndex = _ceilings[index];
            int contents = _contents[index];
            byte wallIndex = (byte)(contents < 100 || contents - 100 >= _wallCount
                ? 0
                : contents - 100);

            _tilemap.Set(order, floorIndex, ceilingIndex, wallIndex, frameCount);
        }
    }
}
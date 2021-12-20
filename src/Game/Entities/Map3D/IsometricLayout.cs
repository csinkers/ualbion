using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.ScriptEvents;

namespace UAlbion.Game.Entities.Map3D
{
    public class IsometricLayout : Component
    {
        IExtrudedTilemap _tilemap;
        byte[] _contents;
        byte[] _floors;
        byte[] _ceilings;
        int _wallCount;

        public int TileCount => _tilemap?.TileCount ?? 0;
        public List<int>[] FloorFrames { get; private set; }
        public List<int>[] CeilingFrames { get; private set; }
        public List<int>[] WallFrames { get; private set; }
        public List<int>[] ContentsFrames { get; private set; }

        public void Update(TilemapRequest request)
        {
            if (request == null)
                return;

            _tilemap.Width = request.Width;
            _tilemap.TileCount = request.TileCount;
            _tilemap.Scale = request.Scale;
            _tilemap.Rotation = request.Rotation;
            _tilemap.Origin = request.Origin;
            _tilemap.VerticalSpacing = request.VerticalSpacing;
            _tilemap.HorizontalSpacing = request.HorizontalSpacing;
            _tilemap.FogColor = request.FogColor;
            _tilemap.AmbientLightLevel = request.AmbientLightLevel;
            _tilemap.ObjectYScaling = request.ObjectYScaling;
        }

        public void Load(LabyrinthId labyrinthId, IsometricMode mode, TilemapRequest request, int? paletteId)
        {
            var assets = Resolve<IAssetManager>();
            var labyrinthData = assets.LoadLabyrinthData(labyrinthId);
            var info = assets.GetAssetInfo(labyrinthId);
            if (labyrinthData == null || info == null)
                return;

            Load(labyrinthData, info, mode, request, paletteId, assets);
        }

        public void Load(LabyrinthData labyrinthData, AssetInfo info, IsometricMode mode, TilemapRequest request, int? paletteNumber, IAssetManager assets)
        {
            if (labyrinthData == null) throw new ArgumentNullException(nameof(labyrinthData));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (assets == null) throw new ArgumentNullException(nameof(assets));

            RemoveAllChildren();

            bool floors = mode is IsometricMode.Floors or IsometricMode.All;
            bool ceilings = mode is IsometricMode.Ceilings or IsometricMode.All;
            bool walls = mode is IsometricMode.Walls or IsometricMode.All;
            bool contents = mode is IsometricMode.Contents or IsometricMode.All;

            paletteNumber ??= info.Get(AssetProperty.PaletteId, 0);
            var paletteId = new PaletteId(AssetType.Palette, paletteNumber.Value);
            var palette = assets.LoadPalette(paletteId);
            if (palette == null)
            {
                Error($"Could not load palette {paletteNumber}");
                palette = assets.LoadPalette(Base.Palette.Common);
            }
            else Raise(new LoadPaletteEvent(paletteId));

            var etmManager = Resolve<IEtmManager>();
            request.Pipeline = DungeonTilemapPipeline.NoCulling;
            request.DayPalette = palette;
            _tilemap = etmManager.CreateTilemap(request /*labyrinthData.Id, 0, request, palette, null, DungeonTilemapPipeline.NoCulling */);

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
                }

                // Count the frames in a separate loop to avoid rebuilding the composited texture over and over
                for (int i = 0; i < labyrinthData.FloorAndCeilings.Count; i++)
                {
                    if (floors) totalTiles += _tilemap.DayFloors.GetFrameCountForLogicalId(i + 1);
                    if (ceilings) totalTiles += _tilemap.DayFloors.GetFrameCountForLogicalId(i + 1);
                }
            }

            if (walls)
            {
                for (int i = 0; i < labyrinthData.Walls.Count; i++)
                {
                    var wallInfo = labyrinthData.Walls[i];
                    bool isAlphaTested = wallInfo != null && (wallInfo.Properties & Wall.WallFlags.AlphaTested) != 0;
                    var wall = wallInfo == null ? null : assets.LoadTexture(wallInfo.SpriteId);
                    _tilemap.DefineWall(i + 1, wall, 0, 0, wallInfo?.TransparentColour ?? 0, isAlphaTested);

                    foreach (var overlayInfo in wallInfo?.Overlays ?? Array.Empty<Overlay>())
                    {
                        if (overlayInfo.SpriteId.IsNone)
                            continue;

                        var overlay = assets.LoadTexture(overlayInfo.SpriteId);
                        _tilemap.DefineWall(i + 1,
                            overlay,
                            overlayInfo.XOffset, overlayInfo.YOffset,
                            wallInfo?.TransparentColour ?? 0, isAlphaTested);
                    }
                }

                // Count the frames in a separate loop to avoid rebuilding the composited texture over and over
                for (int i = 0; i < labyrinthData.Walls.Count; i++)
                    totalTiles += _tilemap.DayWalls.GetFrameCountForLogicalId(i + 1);
            }

            if (contents)
            {
                var transparent = new SimpleTexture<byte>(AssetId.None, "Transparent", 1, 1, new byte[] { 0 })
                    .AddRegion(Vector2.Zero, Vector2.One, 0);

                for (byte i = 1; i <= labyrinthData.ObjectGroups.Count; i++)
                    _tilemap.DefineWall(i, transparent, 0, 0, 0, true);

                totalTiles += labyrinthData.ObjectGroups.Count;
            }

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

            _tilemap.TileCount = totalTiles;
            for (int i = 0; i < totalTiles; i++)
                SetTile(i, i, frames[i]);

            for (int i = 0; i < totalTiles; i++)
                AddSprites(labyrinthData, i,  request);
        }

        void SetTile(int index, int order, int frameCount)
        {
            byte floorIndex = _floors[index];
            byte ceilingIndex = _ceilings[index];
            int contents = _contents[index];
            byte wallIndex = (byte)(contents < 100 || contents - 100 >= _wallCount
                ? contents
                : contents - 100);

            EtmTileFlags flags = 0;
            _tilemap.SetTile(order, floorIndex, ceilingIndex, wallIndex, frameCount, flags);
        }

        void AddSprites(LabyrinthData labyrinthData, int index, TilemapRequest request)
        {
            int contents = _contents[index];
            if (contents == 0 || contents >= labyrinthData.ObjectGroups.Count)
                return;

            var x = (int)(index % request.Width);
            var y = (int)(index / request.Width);

            var objectInfo = labyrinthData.ObjectGroups[contents - 1];
            foreach (var subObject in objectInfo.SubObjects)
            {
                var mapObject = AttachChild(MapObject.Build(x, y, labyrinthData, subObject, request));
                if (mapObject != null)
                    Info($"at ({x},{y}): ({subObject.X}, {subObject.Y}, {subObject.Z}) resolves to {mapObject.Position} ({mapObject.SpriteId})");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Config.Properties;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Ids;
using UAlbion.Formats.ScriptEvents;

namespace UAlbion.Game.Entities.Map3D;

public class IsometricLayout : GameComponent
{
    readonly Dictionary<MapObject, Vector3> _relativeSpritePositions = [];
    readonly ModContext _modContext;
    IExtrudedTilemap _tilemap;
    byte[] _contents;
    byte[] _floors;
    byte[] _ceilings;
    int _wallCount;

    public IsometricLayout(ModContext modContext) 
        => _modContext = modContext ?? throw new ArgumentNullException(nameof(modContext));

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

        foreach (var kvp in _relativeSpritePositions)
            kvp.Key.Position = kvp.Value + request.Origin;
    }

    public void Load(LabyrinthId labyrinthId, IsometricMode mode, TilemapRequest request, int? paletteId)
    {
        var labyrinthData = Assets.LoadLabyrinthData(labyrinthId);
        var node = Assets.GetAssetInfo(labyrinthId);
        if (labyrinthData == null || node == null)
            return;

        var context = new AssetLoadContext(labyrinthId, node, _modContext);
        Load(labyrinthData, context, mode, request, paletteId, Assets);
    }

    public void Load(LabyrinthData labyrinthData, AssetLoadContext context, IsometricMode mode, TilemapRequest request, int? paletteNumber, IAssetManager assets)
    {
        ArgumentNullException.ThrowIfNull(labyrinthData);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(assets);

        RemoveAllChildren();
        _relativeSpritePositions.Clear();

        bool floors   = mode is IsometricMode.Floors or IsometricMode.All;
        bool ceilings = mode is IsometricMode.Ceilings or IsometricMode.All;
        bool walls    = mode is IsometricMode.Walls or IsometricMode.All;
        bool contents = mode is IsometricMode.Contents or IsometricMode.All;

        paletteNumber ??= context.GetProperty(AssetProps.Palette).Id;
        var paletteId = new PaletteId(paletteNumber.Value);
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

        _tilemap?.Dispose();
        _tilemap = etmManager.CreateTilemap(request /*labyrinthData.Id, 0, request, palette, null, DungeonTilemapPipeline.NoCulling */);

        // Layout:
        // [Empty] [First frame of all floors] [First frame of all ceilings] [First frame of all walls] [Additional floor frames] [Additional ceiling frames] [Additional wall frames]

        int totalTiles = 1;
        if (floors || ceilings)
            totalTiles = DefineFloors(labyrinthData, assets, floors, totalTiles, ceilings);

        if (walls)
            totalTiles = DefineWalls(labyrinthData, assets, totalTiles);

        if (contents)
            totalTiles = DefineContents(labyrinthData, totalTiles);

        _wallCount = labyrinthData.Walls.Count;
        _floors = new byte[totalTiles];
        _ceilings = new byte[totalTiles];
        _contents = new byte[totalTiles];
        var frames = new int[totalTiles];

        int index = 1;

        // Add initial frames
        index = BuildFloorFrames(labyrinthData, floors, index);
        index = BuildCeilingFrames(labyrinthData, ceilings, index);
        index = BuildWallFrames(labyrinthData, walls, index);
        index = BuildContentFrames(labyrinthData, contents, index);

        // Add animation frames
        index = BuildExtraFloorFrames(labyrinthData, floors, index, frames);
        index = BuildExtraCeilingFrames(labyrinthData, ceilings, index, frames);
        index = BuildExtraWallFrames(labyrinthData, walls, index, frames);
        BuildExtraContentFrames(labyrinthData, contents, index, frames);

        _tilemap.TileCount = totalTiles;
        for (int i = 0; i < totalTiles; i++)
            SetTile(i, i, frames[i]);

        for (int i = 0; i < totalTiles; i++)
            AddSprites(labyrinthData, i,  request);
    }

    void BuildExtraContentFrames(LabyrinthData labyrinthData, bool contents, int index, int[] frames)
    {
        if (contents)
        {
            for (byte i = 1; i <= labyrinthData.ObjectGroups.Count; i++)
            {
                int frameCount = labyrinthData.FrameCountForObjectGroup(i - 1);
                for (int j = 1; j < frameCount; j++)
                {
                    _contents[index] = i;
                    ContentsFrames[i].Add(index);
                    frames[index++] = j;
                }
            }
        }
    }

    int BuildExtraWallFrames(LabyrinthData labyrinthData, bool walls, int index, int[] frames)
    {
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

        return index;
    }

    int BuildExtraCeilingFrames(LabyrinthData labyrinthData, bool ceilings, int index, int[] frames)
    {
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

        return index;
    }

    int BuildExtraFloorFrames(LabyrinthData labyrinthData, bool floors, int index, int[] frames)
    {
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

        return index;
    }

    int BuildContentFrames(LabyrinthData labyrinthData, bool contents, int index)
    {
        if (contents)
        {
            ContentsFrames = new List<int>[labyrinthData.ObjectGroups.Count + 1];
            ContentsFrames[0] = [0];
            for (byte i = 1; i <= labyrinthData.ObjectGroups.Count; i++)
            {
                _contents[index] = i;
                ContentsFrames[i] = [index];
                index++;
            }
        }

        return index;
    }

    int BuildWallFrames(LabyrinthData labyrinthData, bool walls, int index)
    {
        if (walls)
        {
            WallFrames = new List<int>[labyrinthData.Walls.Count + 1];
            WallFrames[0] = [0];
            for (byte i = 1; i <= labyrinthData.Walls.Count; i++)
            {
                _contents[index] = (byte)(i + 100);
                WallFrames[i] = [index];
                index++;
            }
        }

        return index;
    }

    int BuildCeilingFrames(LabyrinthData labyrinthData, bool ceilings, int index)
    {
        if (ceilings)
        {
            CeilingFrames = new List<int>[labyrinthData.FloorAndCeilings.Count + 1];
            CeilingFrames[0] = [0];
            for (byte i = 1; i <= labyrinthData.FloorAndCeilings.Count; i++)
            {
                _ceilings[index] = i;
                CeilingFrames[i] = [index];
                index++;
            }
        }

        return index;
    }

    int BuildFloorFrames(LabyrinthData labyrinthData, bool floors, int index)
    {
        if (floors)
        {
            FloorFrames = new List<int>[labyrinthData.FloorAndCeilings.Count + 1];
            FloorFrames[0] = [0];
            for (byte i = 1; i <= labyrinthData.FloorAndCeilings.Count; i++)
            {
                _floors[index] = i;
                FloorFrames[i] = [index];
                index++;
            }
        }

        return index;
    }

    int DefineContents(LabyrinthData labyrinthData, int totalTiles)
    {
        var transparent = new SimpleTexture<byte>(AssetId.None, "Transparent", 1, 1, [0]);
        transparent.AddRegion(Vector2.Zero, Vector2.One, 0);

        for (byte i = 1; i <= labyrinthData.ObjectGroups.Count; i++)
        {
            _tilemap.DefineWall(i, transparent, 0, 0, 0, true);
            totalTiles += labyrinthData.FrameCountForObjectGroup(i - 1);
        }

        return totalTiles;
    }

    int DefineWalls(LabyrinthData labyrinthData, IAssetManager assets, int totalTiles)
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
        return totalTiles;
    }

    int DefineFloors(LabyrinthData labyrinthData, IAssetManager assets, bool floors, int totalTiles, bool ceilings)
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

        return totalTiles;
    }

    void SetTile(int index, int order, int frameCount)
    {
        byte floorIndex = _floors[index];
        byte ceilingIndex = _ceilings[index];
        int contents = _contents[index];
        byte wallIndex = (byte)(contents < 100 || contents - 100 >= _wallCount
            ? contents
            : contents - 100);

        _tilemap.SetTile(order, floorIndex, ceilingIndex, wallIndex, frameCount, EtmTileFlags.Translucent);
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
            var mapObject = AttachChild(MapObject.Build(x, y, labyrinthData, subObject, request, false));
            if (mapObject != null)
            {
                _relativeSpritePositions[mapObject] = mapObject.Position;
                mapObject.Position = _relativeSpritePositions[mapObject] + request.Origin;
            }
        }
    }
}

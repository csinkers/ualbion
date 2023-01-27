using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities.Map2D;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map3D;

public class DungeonMap : Component, IMap
{
    readonly MapData3D _mapData;
    LabyrinthData _labyrinthData;
    LogicalMap3D _logicalMap;
    Selection3D _selection;
    float _backgroundRed;
    float _backgroundGreen;
    float _backgroundBlue;
    ISkybox _skybox;

    public DungeonMap(MapId mapId, MapData3D mapData)
    {
        On<WorldCoordinateSelectEvent>(Select);
        On<MapInitEvent>(_ => FireEventChains(TriggerType.MapInit, true));
        On<SlowClockEvent>(_ => FireEventChains(TriggerType.EveryStep, false));
        On<HourElapsedEvent>(_ => FireEventChains(TriggerType.EveryHour, false));
        On<DayElapsedEvent>(_ => FireEventChains(TriggerType.EveryDay, false));
        // On<UnloadMapEvent>(_ => Unload());

        MapId = mapId;
        _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
    }

    public override string ToString() => $"DungeonMap:{MapId} {LogicalSize.X}x{LogicalSize.Y} TileSize: {TileSize}";
    public MapId MapId { get; }
    public MapType MapType => MapType.ThreeD;
    public IMapData MapData => _mapData;
    public Vector2 LogicalSize { get; private set; }
    public Vector3 TileSize => _labyrinthData?.TileSize ?? Vector3.One * 512;
    public float BaseCameraHeight => (_labyrinthData?.CameraHeight ?? 0) != 0 ? _labyrinthData.CameraHeight * 8 : TileSize.Y / 2;

    protected override void Subscribed()
    {
        var state = Resolve<IGameState>();
        var camera = Resolve<ICamera>();

        if (state.Party == null)
            return;

        foreach (var player in state.Party.StatusBarOrder)
            player.SetPositionFunc(() => camera.Position / TileSize);

        if (_labyrinthData != null)
        {
            Raise(new SetClearColourEvent(_backgroundRed, _backgroundGreen, _backgroundBlue, 1.0f));
            return;
        }

        var assets = Resolve<IAssetManager>();
        var factory = Resolve<ICoreFactory>();
        _labyrinthData = assets.LoadLabyrinthData(_mapData.LabDataId);

        if (_labyrinthData == null)
            return;

        _logicalMap = new LogicalMap3D(_mapData, _labyrinthData, state.TemporaryMapChanges, state.PermanentMapChanges);

        var properties = new TilemapRequest
        {
            Id = MapId,
            Width = (uint)_logicalMap.Width,
            Scale = _labyrinthData.TileSize,
            Origin = _labyrinthData.TileSize.Y / 2 * Vector3.UnitY,
            HorizontalSpacing = _labyrinthData.TileSize * Vector3.UnitX,
            VerticalSpacing = _labyrinthData.TileSize * Vector3.UnitZ,
            AmbientLightLevel = _labyrinthData.Lighting,
            FogColor = _labyrinthData.FogColor,
            ObjectYScaling = _labyrinthData.ObjectYScaling,
            Pipeline = DungeonTilemapPipeline.Normal
        };

        _selection = AttachChild(new Selection3D());
        AttachChild(new MapRenderable3D(_logicalMap, _labyrinthData, properties));
        AttachChild(new ScriptManager());
        AttachChild(new Collider3D(_logicalMap));

        if (!_labyrinthData.BackgroundId.IsNone)
        {
            var background = assets.LoadTexture(_labyrinthData.BackgroundId);
            if (background == null)
                Error($"Could not load background image {_labyrinthData.BackgroundId}");

            _skybox = factory.CreateSkybox(background);
        }

        var palette = assets.LoadPalette(_logicalMap.PaletteId);
        uint backgroundColour = palette.GetPaletteAtTime(0)[_labyrinthData.BackgroundColour];
        _backgroundRed = (backgroundColour & 0xff) / 255.0f;
        _backgroundGreen = (backgroundColour & 0xff00 >> 8) / 255.0f;
        _backgroundBlue = (backgroundColour & 0xff0000 >> 16) / 255.0f;

        //if(_labyrinthData.CameraHeight != 0)
        //    Debugger.Break();

        //if(_labyrinthData.Unk12 != 0) // 7=1|2|4 (Jirinaar), 54=32|16|4|2, 156=128|16|8|2 (Tall town)
        //    Debugger.Break();

        // Raise(new LogEvent(LogEvent.Level.Info, $"WallHeight: {_labyrinthData.WallHeight} MaxObj: {maxObjectHeightRaw} EffWallWidth: {_labyrinthData.EffectiveWallWidth}"));

        foreach (var npc in _logicalMap.Npcs)
        {
            if (npc.SpriteOrGroup.IsNone)
                continue;

            if(npc.SpriteOrGroup.Type != AssetType.ObjectGroup)
            {
                Warn($"[3DMap] Tried to load npc with object group of incorrect type: {npc.SpriteOrGroup}");
                continue;
            }

            if (npc.SpriteOrGroup.Id >= _labyrinthData.ObjectGroups.Count)
            {
                Warn($"[3DMap] Tried to load object group {npc.SpriteOrGroup.Id}, but the max group id is {_labyrinthData.ObjectGroups.Count-1}.");
                continue;
            }

            var objectData = _labyrinthData.ObjectGroups[npc.SpriteOrGroup.Id]; // TODO: Verify SpriteOrGroup is an ObjectGroup
            // TODO: Build proper NPC objects with AI, sound effects etc
            foreach (var subObject in objectData.SubObjects)
                AttachChild(MapObject.Build(npc.Waypoints[0].X, npc.Waypoints[0].Y, _labyrinthData, subObject, properties));
        }

        for (int y = 0; y < _logicalMap.Height; y++)
        {
            for (int x = 0; x < _logicalMap.Width; x++)
            {
                var group = _logicalMap.GetObject(x, y);
                if (group == null) continue;
                foreach (var subObject in group.SubObjects)
                    AttachChild(MapObject.Build(x, y, _labyrinthData, subObject, properties));
            }
        }

        Raise(new SetClearColourEvent(_backgroundRed, _backgroundGreen, _backgroundBlue, 1.0f));
    }

    protected override void Unsubscribed()
    {
        _skybox?.Dispose();
        _skybox = null;
        base.Unsubscribed();
    }

    void Select(WorldCoordinateSelectEvent worldCoordinateSelectEvent)
    {
        // TODO
    }

    IEnumerable<MapEventZone> GetZonesOfType(TriggerTypes triggerType) => _mapData.GetZonesOfType(triggerType);
    void FireEventChains(TriggerType type, bool log)
    {
        var zones = GetZonesOfType(type.ToBitField());
        if (!log)
            Raise(new SetLogLevelEvent(LogLevel.Warning));

        foreach (var zone in zones)
            Raise(new TriggerChainEvent(_mapData, zone.EventIndex, new EventSource(_mapData.Id, type, zone.X, zone.Y)));

        if (!log)
            Raise(new SetLogLevelEvent(LogLevel.Info));
    }
}
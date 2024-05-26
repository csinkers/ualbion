using System;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map2D;

public class FlatMap : GameComponent, IMap
{
    readonly MapData2D _mapData;
    readonly Container _sceneObjects;
    LogicalMap2D _logicalMap;
    PartyCaterpillar _partyMovement;

    public MapId MapId { get; }
    public MapType MapType => _logicalMap.UseSmallSprites ? MapType.TwoDOutdoors : MapType.TwoD;
    public Vector2 LogicalSize => new(_logicalMap.Width, _logicalMap.Height);
    public Vector3 TileSize => new(16, 16, 1);
    public IMapData MapData => _mapData;
    public float BaseCameraHeight => 0.0f;
    public override string ToString() => $"FlatMap: {MapId} ({MapId.Id})";

    public FlatMap(MapId mapId, MapData2D mapData)
    {
        MapId = mapId;
        _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        _sceneObjects = new($"MapObjects_{mapId}");

        On<PlayerEnteredTileEvent>(OnPlayerEnteredTile);
        On<NpcEnteredTileEvent>(OnNpcEnteredTile);
        On<ChangeIconEvent>(ChangeIcon);
        OnAsync<MapInitEvent>(_ => FireEventChains(TriggerType.MapInit, true));
        OnAsync<SlowClockEvent>(_ => FireEventChains(TriggerType.EveryStep, false));
        OnAsync<HourElapsedEvent>(_ => FireEventChains(TriggerType.EveryHour, true));
        OnAsync<DayElapsedEvent>(_ => FireEventChains(TriggerType.EveryDay, true));
        On<PartyChangedEvent>(_ => RebuildPartyMembers());
        On<TriggerMapTileEvent>(TileTriggered);
        // On<UnloadMapEvent>(_ => Unload());
    }

    void Setup()
    {
        var tileSize = new Vector2(TileSize.X, TileSize.Y);
        var state = Resolve<IGameState>();
        var gameFactory = Resolve<IGameFactory>();

        _logicalMap = AttachChild(new LogicalMap2D(Assets, _mapData, state.TemporaryMapChanges, state.PermanentMapChanges) { TileSize = tileSize });

        AttachChild(new ScriptManager());
        AttachChild(new Collider2D(
            (x, y) => _logicalMap.GetPassability(_logicalMap.Index(x, y)),
            !_logicalMap.UseSmallSprites));

        // The renderable and selector are children of the Scene, so we don't render the map when the player is in the main menu / inventory screen / combat
        var tileset = Assets.LoadTileGraphics(_logicalMap.TileData.Id.ToTilesetGfx());
        var renderable = new MapRenderable2D(
            _logicalMap,
            tileset,
            gameFactory,
            tileSize);

        var selector = new SelectionHandler2D(_logicalMap, renderable);
        selector.HighlightIndexChanged += (_, x) => renderable.SetHighlightIndex(x);

        _sceneObjects.Add(renderable);
        _sceneObjects.Add(selector);

        var movementSettings = _logicalMap.UseSmallSprites
            ? new MovementSettings(SmallSpriteAnimations.Frames)
            {
                MaxTrailDistance = ReadVar(V.Game.PartyMovement.MaxTrailDistanceSmall),
                MinTrailDistance = ReadVar(V.Game.PartyMovement.MinTrailDistanceSmall),
                TicksPerFrame = ReadVar(V.Game.PartyMovement.TicksPerFrame),
                TicksPerTile = ReadVar(V.Game.PartyMovement.TicksPerTile)
            }
            : new MovementSettings(LargeSpriteAnimations.Frames)
            {
                MaxTrailDistance = ReadVar(V.Game.PartyMovement.MaxTrailDistanceLarge),
                MinTrailDistance = ReadVar(V.Game.PartyMovement.MinTrailDistanceLarge),
                TicksPerFrame = ReadVar(V.Game.PartyMovement.TicksPerFrame),
                TicksPerTile = ReadVar(V.Game.PartyMovement.TicksPerTile)
            };

        var initialPos = new Vector2(_logicalMap.Width / 2.0f, _logicalMap.Height / 2.0f);
        _partyMovement = AttachChild(new PartyCaterpillar(initialPos, Direction.East, movementSettings, _logicalMap));
        Raise(new CameraJumpEvent((int)initialPos.X, (int)initialPos.Y));

        AttachChild(new NpcManager2D(_logicalMap, _sceneObjects));
        RebuildPartyMembers();
    }

    protected override void Subscribed()
    {
        Raise(new SetClearColourEvent(0,0,0, 1.0f));
        if (_logicalMap == null)
            Setup();

        var scene = Resolve<ISceneManager>().ActiveScene;
        scene.Add(_sceneObjects);
    }

    protected override void Unsubscribed()
    {
        if (_sceneObjects.Parent is IScene scene)
            scene.Remove(_sceneObjects);

        base.Unsubscribed();
    }

    void RebuildPartyMembers()
    {
        var existing = Children.Where(x => x is SmallPlayer or LargePlayer).ToList();
        foreach (var player in existing)
            player.Remove();

        var state = Resolve<IGameState>();

        if (state.Party == null)
            return;

        int i = 0;
        foreach (var player in state.Party.StatusBarOrder)
        {
            var iCopy = i; // Make a copy to ensure each closure captures its own number.
            player.SetPositionFunc(() => _partyMovement.GetPositionHistory(iCopy).Item1);
            (Vector3, int) PositionFunc() => _partyMovement.GetPositionHistory(iCopy);

            if (_logicalMap.UseSmallSprites)
                _sceneObjects.Add(new SmallPlayer(player.Id, PositionFunc, TileSize, _sceneObjects));
            else
                _sceneObjects.Add(new LargePlayer(player.Id, PositionFunc, TileSize, _sceneObjects));

            i++;
        }
    }

    async AlbionTask FireEventChains(TriggerType type, bool log)
    {
        var zones = ZoneListPool.Shared.Borrow();
        _logicalMap.GetZonesOfType(zones, type.ToBitField());

        try
        {
            // if (!log)
            //     Raise(new SetLogLevelEvent(LogLevel.Warning));

            foreach (var zone in zones)
                await RaiseAsync(new TriggerChainEvent(_logicalMap.EventSet, zone.EventIndex,
                    new EventSource(_mapData.Id, type, zone.X, zone.Y)));

            // if (!log)
            //     Raise(new SetLogLevelEvent(LogLevel.Info));
        }
        finally
        {
            ZoneListPool.Shared.Return(zones);
        }
    }

    void OnNpcEnteredTile(NpcEnteredTileEvent e)
    {
        var zone = _logicalMap.GetOffsetZone(e.X, e.Y);
        if (zone?.Node == null)
            return;

        if ((zone.Trigger & TriggerTypes.Npc) == 0)
            return;

        var source = new EventSource(_mapData.Id, TriggerType.Npc, zone.X, zone.Y);
        Raise(new TriggerChainEvent(_logicalMap.EventSet, zone.EventIndex, source));
    }

    void OnPlayerEnteredTile(PlayerEnteredTileEvent e)
    {
        var zone = _logicalMap.GetOffsetZone(e.X, e.Y);
        if (zone?.Node == null)
            return;

        if ((zone.Trigger & TriggerTypes.Normal) == 0)
            return;

        var source = new EventSource(_mapData.Id, TriggerType.Normal, zone.X, zone.Y);
        Raise(new TriggerChainEvent(_mapData, zone.EventIndex, source));
    }

    void TileTriggered(TriggerMapTileEvent e)
    {
        var zone = _logicalMap.GetOffsetZone(e.X, e.Y);
        if (zone?.Node == null)
            return;

        var source = new EventSource(_mapData.Id, e.Type, zone.X, zone.Y);
        Raise(new TriggerChainEvent(_logicalMap.EventSet, zone.EventIndex, source));
    }

    void ChangeIcon(ChangeIconEvent e)
    {
        var context = (EventContext)Context;

        bool relative = e.Scope is EventScope.RelPerm or EventScope.RelTemp;
        bool temp = e.Scope is EventScope.AbsTemp or EventScope.RelTemp;

        if (relative && context.Source.AssetId.Type != AssetType.Map)
        {
            ApiUtil.Assert($"Event {e} must be triggered from a map event if using relative coordinates");
            return;
        }

        byte x = relative ? (byte)(e.X + context.Source.X) : (byte)e.X;
        byte y = relative ? (byte)(e.Y + context.Source.Y) : (byte)e.Y;

        _logicalMap.Modify(x, y, e.ChangeType, temp, e.Layers, e.Value);
    }
}

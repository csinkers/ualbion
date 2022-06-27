using System;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core.Events;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.Ids;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;
using MoveVars = UAlbion.Formats.Config.GameVars.PartyMovement;

namespace UAlbion.Game.Entities.Map2D;

public class FlatMap : Component, IMap
{
    readonly MapData2D _mapData;
    LogicalMap2D _logicalMap;
    IMovement _partyMovement;

    public MapId MapId { get; }
    public MapType MapType => _logicalMap.UseSmallSprites ? MapType.TwoDOutdoors : MapType.TwoD;
    public Vector2 LogicalSize => new(_logicalMap.Width, _logicalMap.Height);
    public Vector3 TileSize => new(16, 16, 1);
    public IMapData MapData => _mapData;
    public float BaseCameraHeight => 0.0f;
    public override string ToString() { return $"FlatMap: {MapId} ({MapId.Id})"; }

    public FlatMap(MapId mapId, MapData2D mapData)
    {
        On<PlayerEnteredTileEvent>(OnPlayerEnteredTile);
        On<NpcEnteredTileEvent>(OnNpcEnteredTile);
        On<ChangeIconEvent>(ChangeIcon);
        On<MapInitEvent>(_ => FireEventChains(TriggerTypes.MapInit, true));
        On<SlowClockEvent>(_ => FireEventChains(TriggerTypes.EveryStep, false));
        On<HourElapsedEvent>(_ => FireEventChains(TriggerTypes.EveryHour, true));
        On<DayElapsedEvent>(_ => FireEventChains(TriggerTypes.EveryDay, true));
        On<PartyChangedEvent>(_ => RebuildPartyMembers());
        // On<UnloadMapEvent>(_ => Unload());

        MapId = mapId;
        _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
    }

    protected override void Subscribed()
    {
        Raise(new SetClearColourEvent(0,0,0, 1.0f));
        if (_logicalMap != null)
            return;

        var assetManager = Resolve<IAssetManager>();
        var state = Resolve<IGameState>();
        var gameFactory = Resolve<IGameFactory>();
        _logicalMap = new LogicalMap2D(assetManager, _mapData, state.TemporaryMapChanges, state.PermanentMapChanges);
        var tileset = assetManager.LoadTileGraphics(_logicalMap.TileData.Id.ToTilesetGfx());

        AttachChild(new ScriptManager());
        AttachChild(new Collider2D(
            (x, y) => _logicalMap.GetPassability(_logicalMap.Index(x, y)),
            !_logicalMap.UseSmallSprites));

        var renderable = AttachChild(new MapRenderable2D(
            _logicalMap,
            tileset,
            gameFactory,
            new Vector2(TileSize.X, TileSize.Y)));

        var selector = AttachChild(new SelectionHandler2D(_logicalMap, renderable));
        selector.HighlightIndexChanged += (_, x) => renderable.SetHighlightIndex(x);
        _logicalMap.TileSize = renderable.TileSize;

        var movementSettings = _logicalMap.UseSmallSprites
            ? new MovementSettings(SmallSpriteAnimations.Frames)
            {
                MaxTrailDistance = GetVar(MoveVars.MaxTrailDistanceSmall),
                MinTrailDistance = GetVar(MoveVars.MinTrailDistanceSmall),
                TicksPerFrame = GetVar(MoveVars.TicksPerFrame),
                TicksPerTile = GetVar(MoveVars.TicksPerTile)
            }
            : new MovementSettings(LargeSpriteAnimations.Frames)
            {
                MaxTrailDistance = GetVar(MoveVars.MaxTrailDistanceLarge),
                MinTrailDistance = GetVar(MoveVars.MinTrailDistanceLarge),
                TicksPerFrame = GetVar(MoveVars.TicksPerFrame),
                TicksPerTile = GetVar(MoveVars.TicksPerTile)
            };

        var initialPos = new Vector2(_logicalMap.Width / 2.0f, _logicalMap.Height / 2.0f);
        _partyMovement = AttachChild(new PartyCaterpillar(initialPos, Direction.East, movementSettings, _logicalMap));
        Raise(new CameraJumpEvent((int)initialPos.X, (int)initialPos.Y));

        AttachChild(new NpcManager2D(_logicalMap));
        RebuildPartyMembers();
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
                AttachChild(new SmallPlayer(player.Id, PositionFunc));
            else
                AttachChild(new LargePlayer(player.Id, PositionFunc));

            i++;
        }

    }

    void FireEventChains(TriggerTypes type, bool log)
    {
        var zones = _logicalMap.GetZonesOfType(type);
        if (!log)
            Raise(new SetLogLevelEvent(LogLevel.Warning));

        foreach (var zone in zones)
            Raise(new TriggerChainEvent(zone.ChainSource, zone.Chain, zone.Node, new EventSource(_mapData.Id, _mapData.Id.ToMapText(), type, zone.X, zone.Y)));

        if (!log)
            Raise(new SetLogLevelEvent(LogLevel.Info));
    }

    void OnNpcEnteredTile(NpcEnteredTileEvent e)
    {
        MapEventZone zone;
        while (true)
        {
            zone = _logicalMap.GetZone(e.X, e.Y);
            if (zone == null || !zone.Trigger.HasFlag(TriggerTypes.Npc))
                return;

            if (zone.Node.Event is not OffsetEvent offset)
                break;

            e = new NpcEnteredTileEvent(e.NpcNum, e.X + offset.X, e.Y + offset.Y);
        }

        Raise(new TriggerChainEvent(zone.ChainSource, zone.Chain, zone.Node, new EventSource(_mapData.Id, _mapData.Id.ToMapText(), TriggerTypes.Npc, e.X, e.Y)));
    }

    void OnPlayerEnteredTile(PlayerEnteredTileEvent e)
    {
        MapEventZone zone = null;
        for (int i = 0; i < 255; i++) // Offset chains should have no need to ever be longer than the map width/height.
        {
            zone = _logicalMap.GetZone(e.X, e.Y);
            if (zone?.Node == null || !zone.Trigger.HasFlag(TriggerTypes.Normal))
                return;

            if (zone.Node.Event is not OffsetEvent offset)
                break;
            e = new PlayerEnteredTileEvent(e.X + offset.X, e.Y + offset.Y);
        }

        if (zone == null)
        {
            Error($"Encountered infinite offset event loop including ({e.X}, {e.Y})");
            return;
        }

        Raise(new TriggerChainEvent(zone.ChainSource, zone.Chain, zone.Node, new EventSource(_mapData.Id, _mapData.Id.ToMapText(), TriggerTypes.Normal, e.X, e.Y)));
    }

    void ChangeIcon(ChangeIconEvent e)
    {
        var context = Resolve<IEventManager>().Context;
        if (context.Source.AssetId.Type != AssetType.Map)
        {
            ApiUtil.Assert($"Expected event {e} to be triggered from a map event");
            return;
        }

        bool relative = e.Scope is EventScope.RelPerm or EventScope.RelTemp;
        bool temp = e.Scope is EventScope.AbsTemp or EventScope.RelTemp;

        byte x = relative ? (byte)(e.X + context.Source.X) : (byte)e.X;
        byte y = relative ? (byte)(e.Y + context.Source.Y) : (byte)e.Y;

        _logicalMap.Modify(x, y, e.ChangeType, e.Value, temp);
    }
}
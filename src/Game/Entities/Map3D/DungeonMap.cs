using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Labyrinth;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities.Map3D
{
    public class DungeonMap : Component, IMap
    {
        readonly MapData3D _mapData;
        LabyrinthData _labyrinthData;
        Selection3D _selection;
        float _backgroundRed;
        float _backgroundGreen;
        float _backgroundBlue;

        public DungeonMap(MapId mapId, MapData3D mapData)
        {
            On<WorldCoordinateSelectEvent>(Select);
            On<MapInitEvent>(e => FireEventChains(TriggerTypes.MapInit, true));
            On<SlowClockEvent>(e => FireEventChains(TriggerTypes.EveryStep, false));
            On<HourElapsedEvent>(e => FireEventChains(TriggerTypes.EveryHour, false));
            On<DayElapsedEvent>(e => FireEventChains(TriggerTypes.EveryDay, false));
            // On<UnloadMapEvent>(e => Unload());

            MapId = mapId;
            _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        }

        public override string ToString() => $"DungeonMap:{MapId} {LogicalSize.X}x{LogicalSize.Y} TileSize: {TileSize}";
        public MapId MapId { get; }
        public MapType MapType => MapType.ThreeD;
        public IMapData MapData => _mapData;
        public Vector2 LogicalSize { get; private set; }
        public Vector3 TileSize { get; private set; }
        public float BaseCameraHeight => (_labyrinthData?.CameraHeight ?? 0) != 0 ? _labyrinthData.CameraHeight * 8 : TileSize.Y / 2;

        protected override void Subscribed()
        {
            if (_labyrinthData != null)
                return;

            var assets = Resolve<IAssetManager>();
            _labyrinthData = assets.LoadLabyrinthData(_mapData.LabDataId);

            if (_labyrinthData == null)
            {
                TileSize = Vector3.One * 512;
                return;
            }

            TileSize = new Vector3(_labyrinthData.EffectiveWallWidth, _labyrinthData.WallHeight, _labyrinthData.EffectiveWallWidth);
            _selection = AttachChild(new Selection3D());
            AttachChild(new MapRenderable3D(MapId, _mapData, _labyrinthData, TileSize));
            AttachChild(new ScriptManager());

            if (!_labyrinthData.BackgroundId.IsNone)
                AttachChild(new Skybox(_labyrinthData.BackgroundId));

            var palette = assets.LoadPalette(_mapData.PaletteId);
            uint backgroundColour = palette.GetPaletteAtTime(0)[_labyrinthData.BackgroundColour];
            _backgroundRed = (backgroundColour & 0xff) / 255.0f;
            _backgroundGreen = (backgroundColour & 0xff00 >> 8) / 255.0f;
            _backgroundBlue = (backgroundColour & 0xff0000 >> 16) / 255.0f;

            //if(_labyrinthData.CameraHeight != 0)
            //    Debugger.Break();

            //if(_labyrinthData.Unk12 != 0) // 7=1|2|4 (Jirinaar), 54=32|16|4|2, 156=128|16|8|2 (Tall town)
            //    Debugger.Break();

            var maxObjectHeightRaw = _labyrinthData.ObjectGroups.Max(x => x.SubObjects.Max(y => (int?)y.Y));
            float objectYScaling = TileSize.Y / _labyrinthData.WallHeight;
            if (maxObjectHeightRaw > _labyrinthData.WallHeight * 1.5f)
                objectYScaling /= 2; // TODO: Figure out the proper way to handle this.

            Raise(new LogEvent(LogEvent.Level.Info, $"WallHeight: {_labyrinthData.WallHeight} MaxObj: {maxObjectHeightRaw} EffWallWidth: {_labyrinthData.EffectiveWallWidth}"));

            foreach (var npc in _mapData.Npcs.Values)
            {
                if (npc.SpriteOrGroup.Id >= _labyrinthData.ObjectGroups.Count)
                {
                    CoreUtil.LogWarn($"[3DMap] Tried to load object group {npc.SpriteOrGroup.Id}, but the max group id is {_labyrinthData.ObjectGroups.Count-1}.");
                    continue;
                }

                var objectData = _labyrinthData.ObjectGroups[npc.SpriteOrGroup.Id]; // TODO: Verify SpriteOrGroup is an ObjectGroup
                // TODO: Build proper NPC objects with AI, sound effects etc
                foreach (var subObject in objectData.SubObjects) 
                    AttachChild(BuildMapObject(npc.Waypoints[0].X, npc.Waypoints[0].Y, subObject, objectYScaling));
            }

            for (int y = 0; y < _mapData.Height; y++)
            {
                for (int x = 0; x < _mapData.Width; x++)
                {
                    var contents = _mapData.Contents[y * _mapData.Width + x];
                    if (contents == 0 || contents >= _labyrinthData.ObjectGroups.Count)
                        continue;

                    var objectInfo = _labyrinthData.ObjectGroups[contents - 1];
                    foreach (var subObject in objectInfo.SubObjects)
                        AttachChild(BuildMapObject(x, y, subObject, objectYScaling));
                }
            }

            Raise(new SetClearColourEvent(_backgroundRed, _backgroundGreen, _backgroundBlue));
        }

        void Select(WorldCoordinateSelectEvent worldCoordinateSelectEvent)
        {
            // TODO
        }

        IEnumerable<MapEventZone> GetZonesOfType(TriggerTypes triggerType)
        {
            var matchingKeys = _mapData.ZoneTypeLookup.Keys.Where(x => (x & triggerType) == triggerType);
            return matchingKeys.SelectMany(x => _mapData.ZoneTypeLookup[x]);
        }

        void FireEventChains(TriggerTypes type, bool log)
        {
            var zones = GetZonesOfType(type);
            if (!log)
                Raise(new SetLogLevelEvent(LogEvent.Level.Warning));

            foreach (var zone in zones)
                Raise(new TriggerChainEvent(zone.Chain, zone.Node, new EventSource(_mapData.Id, type, zone.X, zone.Y)));

            if (!log)
                Raise(new SetLogLevelEvent(LogEvent.Level.Info));
        }

        MapObject BuildMapObject(int tileX, int tileY, SubObject subObject, float objectYScaling)
        {
            var definition = _labyrinthData.Objects[subObject.ObjectInfoNumber];
            if (definition.SpriteId.IsNone)
                return null;

            bool onFloor = (definition.Properties & LabyrinthObjectFlags.FloorObject) != 0;

            // We should probably be offsetting the main tilemap by half a tile to centre the objects
            // rather than fiddling with the object positions... will need to reevaluate when working on
            // collision detection, path-finding etc.
            var objectBias = new Vector3(-1.0f, 0, -1.0f) / 2;
                /*
                (MapId == MapId.Jirinaar3D || MapId == MapId.AltesFormergebäude || MapId == MapId.FormergebäudeNachKampfGegenArgim)
                    ? new Vector3(-1.0f, 0, -1.0f) / 2
                    : new Vector3(-1.0f, 0, -1.0f); // / 2;
                */

            var tilePosition = new Vector3(tileX, 0, tileY) + objectBias;
            var offset = new Vector3(
                subObject.X,
                subObject.Y * objectYScaling,
                subObject.Z);

            var smidgeon = onFloor
                ? new Vector3(0,offset.Y < float.Epsilon ? 0.5f : -0.5f, 0)
                : Vector3.Zero;

            Vector3 position = tilePosition * TileSize + offset + smidgeon;

            return new MapObject(
                definition.SpriteId,
                position,
                new Vector2(definition.MapWidth, definition.MapHeight),
                (definition.Properties & LabyrinthObjectFlags.FloorObject) != 0
            );
        }
    }

    public class Selection3D : Component
    {
        ISceneGraph _sceneGraph;
        public Selection3D()
        {
            OnAsync<WorldCoordinateSelectEvent, Selection>(OnSelect);
        }

        protected override void Subscribed()
        {
            _sceneGraph ??= Resolve<ICoreFactory>().CreateSceneGraph();
            base.Subscribed();
        }

        bool OnSelect(WorldCoordinateSelectEvent e, Action<Selection> continuation)
        {
            var hits = new List<Selection>(); // TODO: Get rid of the extra allocation and copying
            _sceneGraph.RayIntersect(e.Origin, e.Direction, hits);
            foreach (var hit in hits)
                continuation(hit);
            return true;

            // Find floor / ceiling hit (furthest point)
            // Iterate all tiles on a straight-line path between origin and floor hit
            // For each tile, yield if filled and if empty iterate contents performing hit checks.
/*
            float denominator = Vector3.Dot(Normal, e.Direction);
            if (Math.Abs(denominator) < 0.00001f)
                return;

            float t = Vector3.Dot(-e.Origin, Normal) / denominator;
            if (t < 0)
                return;

            Vector3 intersectionPoint = e.Origin + t * e.Direction;
            int x = (int)(intersectionPoint.X / _renderable.TileSize.X);
            int y = (int)(intersectionPoint.Y / _renderable.TileSize.Y);

            int highlightIndex = y * _map.Width + x;
            var underlayTile = _map.GetUnderlay(x, y);
            var overlayTile = _map.GetOverlay(x, y);

            e.RegisterHit(t, new MapTileHit(
                new Vector2(x, y),
                intersectionPoint,
                _renderable.GetWeakUnderlayReference(x, y),
                _renderable.GetWeakOverlayReference(x, y)));

            if (underlayTile != null) e.RegisterHit(t, underlayTile);
            if (overlayTile != null) e.RegisterHit(t, overlayTile);
            e.RegisterHit(t, this);

            var zone = _map.GetZone(x, y);
            if (zone != null)
                e.RegisterHit(t, zone);

            var chain = zone?.Chain;
            if (chain != null)
            {
                foreach (var zoneEvent in chain.Events)
                    e.RegisterHit(t, zoneEvent);
            }

            if (_lastHighlightIndex != highlightIndex)
            {
                HighlightIndexChanged?.Invoke(this, highlightIndex);
                _lastHighlightIndex = highlightIndex;
            }
            */
        }

        public void AddToScene(IPositioned child)
        {
            _sceneGraph.Add(child);
        }

        public void RemoveFromScene(IPositioned child)
        {
            _sceneGraph.Remove(child);
        }
    }
}

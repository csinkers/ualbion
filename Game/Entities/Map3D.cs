using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Entities
{
    public class Map3D : Component, IMap
    {
        readonly MapRenderable3D _renderable;
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Map3D, SubscribedEvent>((x, e) => x.Subscribed()),
            new Handler<Map3D, WorldCoordinateSelectEvent>((x, e) => x.Select(e)),
            new Handler<Map3D, CameraJumpEvent>((x, e) => x.Raise(new EngineCameraMoveEvent(e.X * x.TileSize.X, e.Y * x.TileSize.Y, true))),
            new Handler<Map3D, CameraMoveEvent>((x, e) => x.Raise(new EngineCameraMoveEvent(e.X * x.TileSize.X, e.Y * x.TileSize.Y))),
            // new Handler<Map3D, UnloadMapEvent>((x, e) => x.Unload()),
        };

        void Select(WorldCoordinateSelectEvent worldCoordinateSelectEvent)
        {
        }

        public Map3D(Assets assets, MapDataId mapId) : base(Handlers)
        {
            MapId = mapId;
            var mapData = assets.LoadMap3D(mapId);
            var labyrinthData = assets.LoadLabyrinthData(mapData.LabDataId);
            if (labyrinthData != null)
                _renderable = new MapRenderable3D(assets, mapData, labyrinthData);
        }

        public MapDataId MapId { get; }
        public Vector2 LogicalSize { get; }
        public Vector2 TileSize { get; } = new Vector2(64.0f, 64.0f);

        void Subscribed()
        {
            _renderable?.Attach(Exchange);
        }
    }
}
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;
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
            // new Handler<Map3D, UnloadMapEvent>((x, e) => x.Unload()),
        };

        readonly LabyrinthData _labyrinthData;

        void Select(WorldCoordinateSelectEvent worldCoordinateSelectEvent)
        {
        }

        public Map3D(Assets assets, MapDataId mapId) : base(Handlers)
        {
            MapId = mapId;
            var mapData = assets.LoadMap3D(mapId);
            _labyrinthData = assets.LoadLabyrinthData(mapData.LabDataId);
            if (_labyrinthData != null)
            {
                _renderable = new MapRenderable3D(assets, mapData, _labyrinthData);
                TileSize = new Vector3(64.0f, _labyrinthData.WallHeight, 64.0f);
            }
            else
                TileSize = new Vector3(64.0f, 64.0f, 64.0f);
        }

        public MapDataId MapId { get; }
        public Vector2 LogicalSize { get; }
        public Vector3 TileSize { get; }

        void Subscribed()
        {
            if (_renderable != null)
                Exchange.Attach(_renderable);
            Raise(new SetTileSizeEvent(TileSize, _labyrinthData.CameraHeight != 0 ? _labyrinthData.CameraHeight : 32));
        }
    }
}
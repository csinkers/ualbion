using System.Collections.Generic;
using System.Linq;
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

        readonly Assets _assets;
        readonly LabyrinthData _labyrinthData;
        readonly MapData3D _mapData;

        void Select(WorldCoordinateSelectEvent worldCoordinateSelectEvent)
        {
        }

        public Map3D(Assets assets, MapDataId mapId) : base(Handlers)
        {
            _assets = assets;
            MapId = mapId;
            _mapData = assets.LoadMap3D(mapId);
            _labyrinthData = assets.LoadLabyrinthData(_mapData.LabDataId);
            if (_labyrinthData != null)
            {
                _renderable = new MapRenderable3D(assets, _mapData, _labyrinthData);
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

            foreach (var npc in _mapData.Npcs)
            {
                var objectData = _labyrinthData.Objects[npc.ObjectNumber - 1];
                foreach (var subObject in objectData.SubObjects)
                {
                    if (subObject.ObjectInfoNumber == 0)
                        continue;

                    float height = (float)subObject.Y / TileSize.Y;
                    if (height < 0)
                        height += TileSize.Y;

                    var position = new Vector3(npc.Waypoints[0].X, 0, npc.Waypoints[0].Y) * TileSize;
                    var definition = _labyrinthData.ExtraObjects[subObject.ObjectInfoNumber - 1];
                    if(definition.TextureNumber == null)
                        continue;

                    Exchange.Attach(new MapObjectSprite(
                        definition.TextureNumber.Value,
                        position,
                        new Vector2(definition.Width, definition.Height)));
                }
            }

            for (int y = 0; y < _mapData.Height; y++)
            {
                for (int x = 0; x < _mapData.Width; x++)
                {
                    var contents = _mapData.Contents[y * _mapData.Width + x];
                    if (contents == 0 || contents > 100)
                        continue;

                    var objectInfo = _labyrinthData.Objects[contents - 1];
                    foreach (var subObject in objectInfo.SubObjects)
                    {
                        if (subObject.ObjectInfoNumber == 0)
                            continue;

                        float height = (float)subObject.Y / TileSize.Y;
                        if (height < 0)
                            height += TileSize.Y;

                        var position = new Vector3(x, 0, y) * TileSize;
                        var definition = _labyrinthData.ExtraObjects[subObject.ObjectInfoNumber - 1];
                        if (definition.TextureNumber == null)
                            continue;

                        Exchange.Attach(new MapObjectSprite(
                            definition.TextureNumber.Value,
                            position,
                            new Vector2(definition.Width, definition.Height)));
                    }
                }
            }
        }
    }
}
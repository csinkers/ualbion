using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Entities
{
    public class Map3D : Component, IMap
    {
        readonly MapData3D _mapData;
        static readonly IList<Handler> Handlers = new Handler[]
        {
        };

        public Map3D(Assets assets, MapDataId mapId) : base(Handlers)
        {
            MapId = mapId;
            _mapData = assets.LoadMap3D(mapId);
        }

        public MapDataId MapId { get; }
        public Vector2 LogicalSize { get; }
    }
}
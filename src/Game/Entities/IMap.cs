using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Entities
{
    public interface IMap : IComponent
    {
        MapDataId MapId { get; }
        MapType MapType { get; }
        Vector2 LogicalSize { get; }
        Vector3 TileSize { get; }
        float BaseCameraHeight { get; }
        IMapData MapData { get; }
    }
}

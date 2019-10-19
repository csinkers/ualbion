using System.Numerics;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Entities
{
    public interface IMap
    {
        MapDataId MapId { get; }
        Vector2 LogicalSize { get; }
    }
}
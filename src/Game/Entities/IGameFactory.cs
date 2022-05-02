using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Game.Entities.Map2D;

namespace UAlbion.Game.Entities;

public interface IGameFactory : ICoreFactory
{
    IMapLayer CreateMapLayer(LogicalMap2D logicalMap, ITileGraphics tileset, bool isOverlay);
}
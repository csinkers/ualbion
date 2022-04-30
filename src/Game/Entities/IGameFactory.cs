using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities.Map2D;

namespace UAlbion.Game.Entities;

public interface IGameFactory : ICoreFactory
{
    IMapLayer CreateMapLayer(LogicalMap2D logicalMap, ITexture tileset, bool isOverlay);
}
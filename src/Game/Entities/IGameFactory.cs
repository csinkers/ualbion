using System;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities.Map2D;

namespace UAlbion.Game.Entities;

public interface IGameFactory : ICoreFactory
{
    IMapLayer CreateMapLayer(LogicalMap2D logicalMap, ITexture tileset, Func<int, TileData> getTileFunc, DrawLayer layer, IconChangeType iconChangeType);
}
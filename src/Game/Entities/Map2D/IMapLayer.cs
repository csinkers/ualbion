using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities.Map2D;

public interface IMapLayer : IComponent
{
    int? HighlightIndex { get; set; }
    SpriteInstanceData? GetSpriteData(int i, int i1);
}
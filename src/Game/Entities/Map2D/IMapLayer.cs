using UAlbion.Api.Eventing;

namespace UAlbion.Game.Entities.Map2D;

public interface IMapLayer : IComponent
{
    int? HighlightIndex { get; set; }
    object GetSpriteData(int i, int i1);
}
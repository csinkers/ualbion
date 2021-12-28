using UAlbion.Core;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities.Map2D;

public interface IMapLayer : IComponent
{
    int? HighlightIndex { get; set; }
    WeakSpriteReference GetWeakSpriteReference(int x, int y);
}
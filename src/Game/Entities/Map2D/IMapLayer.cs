using UAlbion.Core;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities.Map2D
{
    public interface IMapLayer : IComponent
    {
        int? HighlightIndex { get; set; }
        IWeakSpriteReference GetWeakSpriteReference(int x, int y);
    }
}
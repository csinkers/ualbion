using System.Numerics;
using UAlbion.Core.Visual;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Veldrid.Visual;

public interface IMapLayerBehavior<out TInstance>
{
    bool IsAnimated(int index);
    TInstance BuildInstanceData(int index, int tickCount, Vector3 position);
    SpriteKey GetSpriteKey();
    bool IsChangeApplicable(IconChangeType type);
}
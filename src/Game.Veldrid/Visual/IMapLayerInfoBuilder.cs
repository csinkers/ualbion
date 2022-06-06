using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Veldrid.Visual;

public interface IMapLayerInfoBuilder<out TInstance>
{
    Vector2 TileSize { get; }
    TInstance BlankInstance { get; }
    bool IsAnimated(TileData tile);
    TInstance BuildInstance(Vector3 position, ushort imageNumber, SpriteFlags flags);
    SpriteKey GetSpriteKey(DrawLayer drawLayer, SpriteKeyFlags spriteKeyFlags);
}
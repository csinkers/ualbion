using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class MapSprite : Sprite
    {
        public Vector3 TilePosition
        {
            get => Position / Resolve<IMapManager>().Current.TileSize;
            set
            {
                var map = Resolve<IMapManager>().Current;
                var tilePosition = Position / map.TileSize;
                if (tilePosition == value)
                    return;

                Position = value * map.TileSize;
            }
        }

        public MapSprite(ITextureId id, DrawLayer layer, SpriteKeyFlags keyFlags, SpriteFlags flags)
            : base(id, Vector3.Zero, layer, keyFlags, flags) { }
    }
}

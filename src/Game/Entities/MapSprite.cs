using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class MapSprite<T> : Sprite<T> where T : Enum
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

        public MapSprite(T id, DrawLayer layer, SpriteKeyFlags keyFlags, SpriteFlags flags)
            : base(id, Vector3.Zero, layer, keyFlags, flags) { }
    }
}

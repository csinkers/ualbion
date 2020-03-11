using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class MapSprite<T> : Sprite<T> where T : Enum
    {
        Vector3 _tilePosition;
        public Vector3 TilePosition
        {
            get => _tilePosition;
            set
            {
                if (_tilePosition == value)
                    return;

                _tilePosition = value;

                var map = Resolve<IMapManager>().Current;
                Position = TilePosition * map.TileSize;
            }
        }

        public MapSprite(T id, DrawLayer layer, SpriteKeyFlags keyFlags, SpriteFlags flags)
            : base(id, Vector3.Zero, layer, keyFlags, flags) { }
    }
}

using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public sealed class PositionedSpriteBatch : IDisposable
    {
        SpriteLease _sprite;
        Vector3 _position;
        public PositionedSpriteBatch(SpriteLease lease, Vector2 size)
        {
            _sprite = lease ?? throw new ArgumentNullException(nameof(lease));
            Size = size;
        }

        public DrawLayer RenderOrder => _sprite.Key.RenderOrder;

        /// <summary>
        /// Position of the sprite batch in normalised device coordinates.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (Position == value)
                    return;

                var delta = value - _position;
                var instances = _sprite.Access();
                for (int i = 0; i < instances.Length; i++)
                    instances[i].OffsetBy(delta);

                _position = value;
            }
        }

        public Vector2 Size { get; }

        public void Dispose()
        {
            _sprite?.Dispose();
            _sprite = null;
        }
    }
}

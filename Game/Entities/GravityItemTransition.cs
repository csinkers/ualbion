using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class GravityItemTransition : Component
    {
        protected static readonly Random Random = new Random();
        protected static readonly object SyncRoot = new object();
    }

    public class GravityItemTransition<T> : GravityItemTransition where T : Enum
    {
        const float Gravity = 9.8f;
        const float InitialX = 2.0f;
        const float InitialY = 2.2f;

        readonly Sprite<T> _sprite;
        Vector2 _velocity;

        public GravityItemTransition(T spriteId, int subImage, Vector2 fromPosition, Vector2 size)
        {
            On<EngineUpdateEvent>(e => Update(e.DeltaSeconds));

            lock (SyncRoot)
            {
                _velocity = new Vector2(
                    (float)(Random.NextDouble() - 0.5) * InitialX,
                    (float)Random.NextDouble() * InitialY);
            }

            _sprite = AttachChild(new Sprite<T>(
                spriteId,
                new Vector3(fromPosition, 0),
                DrawLayer.InterfaceOverlay,
                SpriteKeyFlags.NoTransform | SpriteKeyFlags.NoDepthTest,
                SpriteFlags.LeftAligned)
            {
                Size = size,
                Frame = subImage
            });
        }

        void Update(float deltaSeconds)
        {
            if (_sprite.Position.Y > UiConstants.StatusBarExtents.Bottom)
            {
                Remove();
                return;
            }

            _velocity += new Vector2(0, -Gravity) * deltaSeconds;
            _sprite.Position += new Vector3(_velocity, 0) * deltaSeconds;
        }
    }
}
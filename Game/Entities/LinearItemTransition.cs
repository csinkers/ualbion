using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class LinearItemTransition<T> : Component where T : Enum
    {
        readonly Sprite<T> _sprite;
        readonly Vector2 _fromPosition;
        readonly Vector2 _toPosition;
        readonly float _transitionTimeSeconds;
        float _elapsedTime;

        public LinearItemTransition(T spriteId, int subImage, Vector2 fromPosition, Vector2 toPosition, float transitionTimeSeconds, Vector2 size)
        {
            On<EngineUpdateEvent>(e => Update(e.DeltaSeconds));

            _fromPosition = fromPosition;
            _toPosition = toPosition;
            _transitionTimeSeconds = transitionTimeSeconds;

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
            _elapsedTime += deltaSeconds;
            float t = _elapsedTime / _transitionTimeSeconds;
            if (t > 1.0f)
            {
                Remove();
                return;
            }

            _sprite.Position = new Vector3(Vector2.Lerp(_fromPosition, _toPosition, t), 0);
        }
    }
}
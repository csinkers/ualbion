using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Entities
{
    public class ItemTransition<T> : Component where T : Enum
    {
        readonly Vector2 _fromPosition;
        readonly Vector2 _toPosition;
        readonly float _transitionTimeSeconds;
        readonly Sprite<T> _sprite;
        float _elapsedTime;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<ItemTransition<T>, EngineUpdateEvent>((x,e) => x.Update(e.DeltaSeconds))
        );

        public ItemTransition(T spriteId, int subImage, Vector2 fromPosition, Vector2 toPosition, float transitionTimeSeconds, Vector2 size) : base(Handlers)
        {
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
                Detach();
                return;
            }

            _sprite.Position = new Vector3(Vector2.Lerp(_fromPosition, _toPosition, t), 0);
        }
    }
}

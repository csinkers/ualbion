using System;
using System.Numerics;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities;

public class LinearItemTransition : Component
{
    readonly Sprite _sprite;
    readonly Vector2 _fromPosition;
    readonly Vector2 _toPosition;
    readonly float _transitionTimeSeconds;
    readonly Action _continuation;
    float _elapsedTime;

    public LinearItemTransition(SpriteId spriteId, int subImage, Vector2 fromPosition, Vector2 toPosition, float transitionTimeSeconds, Vector2 size, Action continuation)
    {
        On<EngineUpdateEvent>(e => Update(e.DeltaSeconds));

        _fromPosition = fromPosition;
        _toPosition = toPosition;
        _transitionTimeSeconds = transitionTimeSeconds;
        _continuation = continuation;

        _sprite = AttachChild(new Sprite(
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
            _continuation?.Invoke();
            return;
        }

        _sprite.Position = new Vector3(Vector2.Lerp(_fromPosition, _toPosition, t), 0);
    }
}
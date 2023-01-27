using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats.Ids;
using TransitionVars = UAlbion.Formats.Config.GameVars.Ui.Transitions;

namespace UAlbion.Game.Entities;

public class GravityItemTransition : Component
{
    static readonly Random Random = new();
    static readonly object SyncRoot = new();

    readonly Action _continuation;
    readonly Sprite _sprite;
    Vector2 _velocity;

    public GravityItemTransition(SpriteId spriteId, int subImage, Vector2 fromPosition, Vector2 size, Action continuation)
    {
        _continuation = continuation;
        On<EngineUpdateEvent>(e => Update(e.DeltaSeconds));

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

    protected override void Subscribed()
    {
        lock (SyncRoot)
        {
            _velocity = new Vector2(
                (float)(Random.NextDouble() - 0.5) * Var(TransitionVars.DiscardItemMaxInitialX),
                (float)Random.NextDouble() * Var(TransitionVars.DiscardItemMaxInitialY));
        }

        base.Subscribed();
    }

    void Update(float deltaSeconds)
    {
        if (_sprite.Position.Y > UiConstants.StatusBarExtents.Bottom)
        {
            Remove();
            _continuation?.Invoke();
            return;
        }

        _velocity += new Vector2(0, -Var(TransitionVars.DiscardItemGravity) * deltaSeconds);
        _sprite.Position += new Vector3(_velocity, 0) * deltaSeconds;
    }
}
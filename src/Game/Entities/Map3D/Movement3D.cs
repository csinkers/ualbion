using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Entities.Map3D;

public class Movement3D : Component
{
    const float TurnRateDegreesPerFrame = 3.0f;
    float _pendingYawDegrees;
    bool _noclip;

    public Movement3D()
    {
        On<PartyMoveEvent>(OnMove);
        On<PartyJumpEvent>(OnJump);
        On<PartyTurnEvent>(OnTurn);
        On<PartyMove3DEvent>(OnMove3D);
        On<PartyTurn3DEvent>(e => _pendingYawDegrees = e.YawDegrees);
        On<FastClockEvent>(OnTick);
        On<NoClipEvent>(_ =>
        {
            _noclip = !_noclip;
            Info($"Clipping {(_noclip ? "off" : "on")}");
        });
    }

    void OnTick(FastClockEvent e)
    {
        var absPendingYaw = Math.Abs(_pendingYawDegrees);
        if (absPendingYaw > 0.1f)
        {
            int sign = Math.Sign(_pendingYawDegrees);
            float d = e.Frames * TurnRateDegreesPerFrame;
            float newAbs = Math.Max(0, absPendingYaw - d);
            _pendingYawDegrees = sign * newAbs;

            var dRadians = -sign * d * MathF.PI / 180.0f;
            Raise(new CameraRotateEvent(dRadians, 0));
        }

        var party = Resolve<IParty>();
        var state = Resolve<IMovementState>();
        var detector = Resolve<ICollisionManager>();
        var (dx, dy) = (1.0f, 1.0f);
        (dx, dy) = CheckForCollisions(state.NoClip ? null : detector, state.X, state.Y, dx, dy);
    }

    static (float dx, float dy) CheckForCollisions(ICollisionManager detector, ushort stateX, ushort stateY, float dx, float dy)
    {
        return (0, 0);
    }

    void OnMove3D(PartyMove3DEvent e)
    {
    }

    void OnMove(PartyMoveEvent e)
    {
    }

    void OnTurn(PartyTurnEvent e)
    {
    }

    void OnJump(PartyJumpEvent e)
    {
    }

}
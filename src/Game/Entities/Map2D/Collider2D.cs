using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Entities.Map2D;

public class Collider2D : Component, IMovementCollider
{
    readonly Func<int, int, Passability> _getPassability;
    readonly bool _isLargeMap;

    public Collider2D(Func<int, int, Passability> getPassability, bool isLargeMap)
    {
        _getPassability = getPassability ?? throw new ArgumentNullException(nameof(getPassability));
        _isLargeMap = isLargeMap;
    }

    protected override void Subscribed() => Resolve<ICollisionManager>()?.Register(this);
    protected override void Unsubscribed() => Resolve<ICollisionManager>()?.Unregister(this);
    public bool IsOccupied(int fromX, int fromY, int toX, int toY) => 
        IsOccupiedCore(fromX, fromY, toX, toY) ||
        _isLargeMap && IsOccupiedCore(fromX - 1, fromY, toX - 1, toY);

    bool IsOccupiedCore(int fromX, int fromY, int toX, int toY)
    {
        int dx = Math.Sign(toX - fromX);
        int dy = Math.Sign(toY - fromY);
        var sourcePassability = _getPassability(fromX, fromY);
        var destPassability = _getPassability(toX, toY);
        bool sourceOk = IsAllowed(dx, dy, sourcePassability);
        bool destOk = IsAllowed(-dx, -dy, destPassability);
        return !sourceOk || !destOk;
    }

    static bool IsAllowed(int dx, int dy, Passability passability) =>
        (passability & Passability.Solid) == 0 &&
        dx switch
        {
            > 0 when (passability & Passability.BlockEast) != 0 => false,
            < 0 when (passability & Passability.BlockWest) != 0 => false,
            _ => dy switch
            {
                < 0 when (passability & Passability.BlockNorth) != 0 => false,
                > 0 when (passability & Passability.BlockSouth) != 0 => false,
                _ => true
            }
        };
}
using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Entities.Map2D;

public class Collider2D : Component, IMovementCollider
{
    readonly LogicalMap2D _logicalMap;
    readonly bool _isLargeMap;

    public Collider2D(LogicalMap2D logicalMap, bool isLargeMap)
    {
        _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
        _isLargeMap = isLargeMap;
    }

    protected override void Subscribed() => Resolve<ICollisionManager>()?.Register(this);
    protected override void Unsubscribed() => Resolve<ICollisionManager>()?.Unregister(this);
    public bool IsOccupied(int tx, int ty) => 
        IsOccupiedCore(tx, ty) || 
        _isLargeMap && IsOccupiedCore(tx-1, ty);

    bool IsOccupiedCore(int tx, int ty)
    {
        if (tx < 0) return true;
        if (ty < 0) return true;
        if (tx >= _logicalMap.Width) return true;
        if (ty >= _logicalMap.Height) return true;

        var underlayTile = _logicalMap.GetUnderlay(tx, ty);
        var overlayTile = _logicalMap.GetOverlay(tx, ty);

        bool underlayBlocked = underlayTile != null && underlayTile.Collision != Passability.Passable;
        bool overlayBlocked = overlayTile != null && overlayTile.Collision != Passability.Passable;
        return underlayBlocked || overlayBlocked;
    }

    public Passability GetPassability(int tx, int ty) => _logicalMap.GetPassability(_logicalMap.Index(tx, ty));
}
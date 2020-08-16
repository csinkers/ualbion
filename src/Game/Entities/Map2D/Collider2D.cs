using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game.Entities.Map2D
{
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
        public bool IsOccupied(Vector2 tilePosition) => 
            IsOccupiedCore(tilePosition) || 
            (_isLargeMap && IsOccupiedCore(tilePosition + new Vector2(-1.0f, 0.0f)));

        bool IsOccupiedCore(Vector2 tilePosition)
        {
            var underlayTile = _logicalMap.GetUnderlay((int)tilePosition.X, (int)tilePosition.Y);
            var overlayTile = _logicalMap.GetOverlay((int)tilePosition.X, (int)tilePosition.Y);

            bool underlayBlocked = underlayTile != null && underlayTile.Collision != Passability.Passable;
            bool overlayBlocked = overlayTile != null && overlayTile.Collision != Passability.Passable;
            return underlayBlocked || overlayBlocked;
        }

        public Passability GetPassability(Vector2 tilePosition) =>
            _logicalMap.GetPassability(_logicalMap.Index((int)tilePosition.X, (int)tilePosition.Y));
    }
}

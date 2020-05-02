using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.Assets.Map;

namespace UAlbion.Game.Entities.Map2D
{
    public class Collider : Component, ICollider
    {
        readonly LogicalMap _logicalMap;
        readonly bool _isLargeMap;

        public Collider(LogicalMap logicalMap, bool isLargeMap)
        {
            _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
            _isLargeMap = isLargeMap;
        }

        protected override void Subscribed()
        {
            Resolve<ICollisionManager>()?.Register(this);
            base.Subscribed();
        }

        public override void Detach()
        {
            Resolve<ICollisionManager>()?.Unregister(this);
            base.Detach();
        }

        public bool IsOccupied(Vector2 tilePosition) => IsOccupiedCore(tilePosition) || (_isLargeMap && IsOccupiedCore(tilePosition + new Vector2(-1.0f, 0.0f)));

        bool IsOccupiedCore(Vector2 tilePosition)
        {
            var underlayTile = _logicalMap.GetUnderlay((int)tilePosition.X, (int)tilePosition.Y);
            var overlayTile = _logicalMap.GetOverlay((int)tilePosition.X, (int)tilePosition.Y);

            bool underlayBlocked = underlayTile != null && underlayTile.Collision != Passability.Passable;
            bool overlayBlocked = overlayTile != null && overlayTile.Collision != Passability.Passable;
            return underlayBlocked || overlayBlocked;
        }

        public Passability GetPassability(Vector2 tilePosition)
        {
            var underlayTile = _logicalMap.GetUnderlay((int)tilePosition.X, (int)tilePosition.Y);
            var overlayTile = _logicalMap.GetOverlay((int)tilePosition.X, (int)tilePosition.Y);
            return underlayTile?.Collision ?? overlayTile?.Collision ?? Passability.Passable;
        }
    }
}

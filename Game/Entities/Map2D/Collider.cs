using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Entities.Map2D
{
    public class Collider : Component, ICollider
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Collider, ExchangeDisabledEvent>((x, e) => x.Resolve<ICollisionManager>()?.Unregister(x))
        );

        readonly LogicalMap _logicalMap;

        public Collider(LogicalMap logicalMap) : base(Handlers)
        {
            _logicalMap = logicalMap ?? throw new ArgumentNullException(nameof(logicalMap));
        }

        public override void Subscribed()
        {
            Resolve<ICollisionManager>()?.Register(this);
            base.Subscribed();
        }

        public bool IsOccupied(Vector2 tilePosition)
        {
            var underlayTile = _logicalMap.GetUnderlay((int)tilePosition.X, (int)tilePosition.Y);
            var overlayTile = _logicalMap.GetOverlay((int)tilePosition.X, (int)tilePosition.Y);

            bool underlayBlocked = underlayTile != null && underlayTile.Collision != TilesetData.Passability.Passable;
            bool overlayBlocked = overlayTile != null && overlayTile.Collision != TilesetData.Passability.Passable;
            return underlayBlocked || overlayBlocked;
        }

        public TilesetData.Passability GetPassability(Vector2 tilePosition)
        {
            var underlayTile = _logicalMap.GetUnderlay((int)tilePosition.X, (int)tilePosition.Y);
            var overlayTile = _logicalMap.GetOverlay((int)tilePosition.X, (int)tilePosition.Y);
            return underlayTile?.Collision ?? overlayTile?.Collision ?? TilesetData.Passability.Passable;
        }
    }
}
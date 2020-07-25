using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.Assets.Map;

namespace UAlbion.Game
{
    public class CollisionManager : ServiceComponent<ICollisionManager>, ICollisionManager
    {
        readonly IList<IMovementCollider> _colliders = new List<IMovementCollider>();

        public bool IsOccupied(Vector2 tilePosition)
            => _colliders.Aggregate(false, (acc, x) => acc | x.IsOccupied(tilePosition));

        public void Register(IMovementCollider collider)
        {
            if (!_colliders.Contains(collider))
                _colliders.Add(collider);
        }

        public void Unregister(IMovementCollider collider) => _colliders.Remove(collider);
        public Passability GetPassability(Vector2 tilePosition)
            => _colliders.Select(x => x.GetPassability(tilePosition)).FirstOrDefault();
    }
}

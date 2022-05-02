using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Game;

public class CollisionManager : ServiceComponent<ICollisionManager>, ICollisionManager
{
    readonly IList<IMovementCollider> _colliders = new List<IMovementCollider>();

    public bool IsOccupied(int x, int y)
        => _colliders.Aggregate(false, (acc, coll) => acc | coll.IsOccupied(x, y));

    public void Register(IMovementCollider collider)
    {
        if (!_colliders.Contains(collider))
            _colliders.Add(collider);
    }

    public void Unregister(IMovementCollider collider) => _colliders.Remove(collider);
    public Passability GetPassability(int x, int y)
        => _colliders.Select(coll => coll.GetPassability(x, y)).FirstOrDefault();
}
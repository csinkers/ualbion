using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using Veldrid.Utilities;

namespace UAlbion.Core.Veldrid;

public class SceneGraph : ServiceComponent<ISceneGraph>, ISceneGraph
{
    readonly Octree<IPositioned> _octree = new(new BoundingBox(-Vector3.One, Vector3.One), 2);
    [ThreadStatic] static List<RayCastHit<IPositioned>> _hitList;

    public SceneGraph()
    {
        On<AddPositionedComponentEvent>(e => Add(e.Positioned));
        On<RemovePositionedComponentEvent>(e => Remove(e.Positioned));
        On<PositionedComponentMovedEvent>(e =>
        {
            Remove(e.Positioned);
            Add(e.Positioned);
        });
    }

    public void Add(IPositioned entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _octree.AddItem(new BoundingBox(
                entity.Position - entity.Dimensions,
                entity.Position + entity.Dimensions),
            entity);
    }

    public void Remove(IPositioned entity) => _octree.RemoveItem(entity);

    public void RayIntersect(Vector3 origin, Vector3 direction, List<Selection> hits)
    {
        ArgumentNullException.ThrowIfNull(hits);

        _hitList ??= new List<RayCastHit<IPositioned>>();
        _hitList.Clear();
        _octree.RayCast(new Ray(origin, direction), _hitList, (ray, item, list) =>
        {
            var hit = item.RayIntersect(ray.Origin, ray.Direction);
            if (!hit.HasValue) 
                return 0;

            list.Add(new RayCastHit<IPositioned>(item, hit.Value.Item2, hit.Value.Item1));
            return 1;
        });

        foreach (var hit in _hitList)
            hits.Add(new Selection(hit.Location, hit.Distance, hit.Item));
    }

    public void FrustumIntersect(Matrix4x4 frustum, List<IPositioned> hits) 
        => _octree.GetContainedObjects(new BoundingFrustum(frustum), hits);
}
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core.Events;

namespace UAlbion.Core;

public interface ISceneGraph
{
    void Add(IPositioned entity);
    void Remove(IPositioned entity);
    void RayIntersect(Vector3 origin, Vector3 direction, List<Selection> hits);
    void FrustumIntersect(Matrix4x4 frustum, List<IPositioned> hits);
}
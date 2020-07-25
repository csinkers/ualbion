using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;

namespace UAlbion.TestCommon
{
    public class MockSceneGraph : ISceneGraph
    {
        public void Add(IPositioned entity) { }
        public void Remove(IPositioned entity) { }
        public void RayIntersect(Vector3 origin, Vector3 direction, List<Selection> hits) { }
        public void FrustumIntersect(Matrix4x4 frustum, List<IPositioned> hits) { }
    }
}
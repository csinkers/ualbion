using System.Numerics;
using Veldrid.Utilities;

namespace UAlbion.Core
{
    public interface ISceneObject
    {
        BoundingBox Extents { get; }
    }

    public class SceneGraph : Component
    {
        readonly Octree<ISceneObject> _octree = new Octree<ISceneObject>(new BoundingBox(-Vector3.One, Vector3.One), 2);

        static readonly Handler[] Handlers =
        {
        };

        public SceneGraph() : base(Handlers)
        {
        }
    }
}

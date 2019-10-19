using Veldrid.Utilities;

namespace UAlbion.Core
{
    public interface ISceneObject
    {
        BoundingBox Extents { get; }
    }
}
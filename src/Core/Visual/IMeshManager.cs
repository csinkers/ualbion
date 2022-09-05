using System.Numerics;

namespace UAlbion.Core.Visual;

public interface IMeshManager
{
    IMeshInstance BuildInstance(MeshId id, Vector3 position, Vector3 scale);
}
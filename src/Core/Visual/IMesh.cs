using System.Numerics;

namespace UAlbion.Core.Visual;

public interface IMesh
{
    MeshId Id { get; }
    Vector3 BoxMax { get; }
    Vector3 BoxMin { get; }
    Vector3 SphereCenter { get; }
    float SphereRadius { get; }
}
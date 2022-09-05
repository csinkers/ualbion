using System.Numerics;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Visual;

public interface IMeshInstance : IComponent, IPositioned
{
    MeshId Id { get; }
    void SetPosition(Vector3 position);
}
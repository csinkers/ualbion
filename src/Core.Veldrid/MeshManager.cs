using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Meshes;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid;

public class MeshManager : ServiceComponent<IMeshManager>, IMeshManager
{
    readonly Func<MeshId, Mesh> _loadFunc;
    public MeshManager(Func<MeshId, Mesh> loadFunc) 
        => _loadFunc = loadFunc ?? throw new ArgumentNullException(nameof(loadFunc));

    public IMeshInstance BuildInstance(MeshId id, Vector3 position, Vector3 scale)
    {
        var mesh = _loadFunc(id);
        return new MeshInstance(mesh, position, scale);
    }
}
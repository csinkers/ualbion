using System;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Meshes;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Ids;

namespace UAlbion.Core.Veldrid;

public class MeshManager : ServiceComponent<IMeshManager>, IMeshManager
{
    // new BatchManager<MeshId, GpuMeshInstanceData>(key => ((VeldridCoreFactory)Resolve<ICoreFactory>()).CreateMeshBatch(key));
    public IMeshInstance BuildInstance(MeshId id, Vector3 position, Vector3 scale)
    {
        var assets = Resolve<IAssetManager>();
        if (assets.LoadMapObject((MapObjectId)id.Id) is not Mesh mesh)
            throw new InvalidOperationException($"Could not load mesh for {id}");

        return new MeshInstance(mesh, position, scale);
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Meshes;
using UAlbion.Core.Visual;
using Veldrid.Utilities;

namespace UAlbion.Core.Veldrid;

public class Mesh : IMesh
{
    public Mesh(MeshId id, ConstructedMesh16 mesh, MaterialDefinition material, Dictionary<string, ITexture> textures)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(material);
        Id = id;
        Textures = textures ?? throw new ArgumentNullException(nameof(textures));

        var vertices = mesh.Vertices;
        Vertices = Unsafe.As<VertexPositionNormalTexture[], MeshVertex[]>(ref vertices);
        Indices = mesh.Indices;
        Material = material;
        BoundingSphere = mesh.GetBoundingSphere();
        BoundingBox = mesh.GetBoundingBox();
    }

    public MeshId Id { get; }
#pragma warning disable CA1051 // Do not declare visible instance fields - 
    public readonly MeshVertex[] Vertices;
    public readonly ushort[] Indices;
#pragma warning restore CA1051 // Do not declare visible instance fields
    public Dictionary<string, ITexture> Textures { get; }
    public MaterialDefinition Material { get; }
    public Vector3 BoxMax => BoundingBox.Max;
    public Vector3 BoxMin => BoundingBox.Min;
    public Vector3 SphereCenter => BoundingSphere.Center;
    public float SphereRadius => BoundingSphere.Radius;

#pragma warning disable CA1051 // Do not declare visible instance fields - need fields to use more efficient ref calls
    public BoundingBox BoundingBox;
    public BoundingSphere BoundingSphere;
#pragma warning restore CA1051 // Do not declare visible instance fields
}

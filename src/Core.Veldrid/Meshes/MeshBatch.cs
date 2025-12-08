using System;
using System.Numerics;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Meshes;

public class MeshBatch : RenderableBatch<MeshId, GpuMeshInstanceData>
{
    readonly Func<MeshId, Mesh> _loadFunc;
    internal MultiBuffer<MeshVertex> VertexBuffer { get; private set; }
    internal MultiBuffer<ushort> IndexBuffer { get; private set; }
    internal MultiBuffer<GpuMeshInstanceData> Instances { get; private set; }
    internal SingleBuffer<MeshUniform> Uniform { get; private set; }
    public ITextureHolder Diffuse { get; private set; }
    internal MeshResourceSet ResourceSet { get; private set; }

    public MeshBatch(MeshId id, Func<MeshId, Mesh> loadFunc) : base(id, DisableInstancesFunc)
        => _loadFunc = loadFunc ?? throw new ArgumentNullException(nameof(loadFunc));

    static void DisableInstancesFunc(Span<GpuMeshInstanceData> instances)
    {
        for (int i = 0; i < instances.Length; i++)
        {
            ref var instance = ref instances[i];
            instance.Position = new Vector3(1e12f, 1e12f, 1e12f);
            instance.Scale = Vector3.Zero;
        }
    }

    protected override void Subscribed()
    {
        var mesh = _loadFunc(Key);
        if (mesh == null)
            throw new InvalidOperationException($"Could not load mesh {Key}");

        var samplerSource = Resolve<ISpriteSamplerSource>();
        var source = Resolve<ITextureSource>();

        if (VertexBuffer == null)
        {
            var textureName = mesh.Material.DiffuseTexture;
            if (!mesh.Textures.TryGetValue(textureName, out var texture))
                throw new InvalidOperationException($"Texture \"{textureName}\" not loaded in {mesh.Id}");

            var uniform = new MeshUniform
            {
                AmbientReflectivity = mesh.Material.AmbientReflectivity,
                DiffuseReflectivity = mesh.Material.DiffuseReflectivity,
                SpecularReflectivity = mesh.Material.SpecularReflectivity,
                EmissiveCoefficient = mesh.Material.EmissiveCoefficient,
                IlluminationModel = mesh.Material.IlluminationModel,
                Opacity = mesh.Material.Opacity,
                OpticalDensity = mesh.Material.OpticalDensity,
                Sharpness = mesh.Material.Sharpness,
                SpecularExponent = mesh.Material.SpecularExponent,
                TransmissionFilter = mesh.Material.TransmissionFilter
            };

#pragma warning disable CA2000
            VertexBuffer = AttachChild(new MultiBuffer<MeshVertex>(in mesh.Vertices, BufferUsage.VertexBuffer, $"VB_{mesh.Id}"));
            Instances = AttachChild(new MultiBuffer<GpuMeshInstanceData>(MinSize, BufferUsage.VertexBuffer, $"VB_Inst:{Name}"));
            IndexBuffer = AttachChild(new MultiBuffer<ushort>(in mesh.Indices, BufferUsage.IndexBuffer, $"IB_{mesh.Id}"));

            Uniform = AttachChild(new SingleBuffer<MeshUniform>(in uniform, BufferUsage.UniformBuffer, $"UB_{mesh.Id}"));
            Diffuse = source.GetSimpleTexture(texture);
#pragma warning restore CA2000
        }

        ResourceSet = new MeshResourceSet
        {
            Name = $"RS_Mesh:{mesh.Id}",
            Uniform = Uniform,
            Diffuse = Diffuse,
            Sampler = samplerSource.GetSampler(SpriteSampler.Point),
        };
        AttachChild(ResourceSet);
    }

    protected override void Unsubscribed() => CleanupSet();
    protected override ReadOnlySpan<GpuMeshInstanceData> ReadOnlyInstances => Instances.Data;
    protected override Span<GpuMeshInstanceData> MutableInstances => Instances.Borrow();
    protected override void Resize(int instanceCount) => Instances.Resize(instanceCount);

    void CleanupSet()
    {
        ResourceSet.Dispose();
        RemoveChild(ResourceSet);
        ResourceSet = null;
    }

    protected override void Dispose(bool disposing)
    {
        CleanupSet();
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
        Instances.Dispose();
        base.Dispose(disposing);
    }
}
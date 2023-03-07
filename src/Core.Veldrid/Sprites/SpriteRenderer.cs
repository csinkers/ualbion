using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

public sealed class SpriteRenderer : Component, IRenderer<GlobalSet, MainPassSet>, IDisposable
{
    readonly MultiBuffer<Vertex2DTextured> _vertexBuffer;
    readonly MultiBuffer<ushort> _indexBuffer;
    readonly SpritePipeline _pipeline;
    readonly SpritePipeline _noCullPipeline;

    static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
    static readonly Vertex2DTextured[] Vertices =
    {
        new(-0.5f, 0.0f, 0.0f, 0.0f), new(0.5f, 0.0f, 1.0f, 0.0f),
        new(-0.5f, 1.0f, 0.0f, 1.0f), new(0.5f, 1.0f, 1.0f, 1.0f),
    };

    public Type[] HandledTypes { get; } = { typeof(VeldridSpriteBatch<SpriteInfo, GpuSpriteInstanceData>) };

    public SpriteRenderer(in OutputDescription outputFormat)
    {
        _vertexBuffer = new MultiBuffer<Vertex2DTextured>(Vertices, BufferUsage.VertexBuffer, "SpriteVertexBuffer");
        _indexBuffer = new MultiBuffer<ushort>(Indices, BufferUsage.IndexBuffer, "SpriteIndexBuffer");
        _pipeline = BuildPipeline(
            new DepthStencilStateDescription
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.LessEqual
            }, outputFormat);

        _noCullPipeline = BuildPipeline(
            new DepthStencilStateDescription
            {
                DepthTestEnabled = true,
                DepthWriteEnabled = true,
                DepthComparison = ComparisonKind.Always
            }, outputFormat);
        AttachChild(_vertexBuffer);
        AttachChild(_indexBuffer);
        AttachChild(_pipeline);
        AttachChild(_noCullPipeline);
    }

    static SpritePipeline BuildPipeline(DepthStencilStateDescription depthMode, in OutputDescription outputFormat)
    {
        return new()
        {
            Name = "P:Sprite",
            AlphaBlend = BlendStateDescription.SingleAlphaBlend,
            CullMode = FaceCullMode.None,
            DepthStencilMode = depthMode,
            FillMode = PolygonFillMode.Solid,
            OutputDescription = outputFormat,
            Topology = PrimitiveTopology.TriangleList,
            UseDepthTest = true,
            UseScissorTest = true,
            Winding = FrontFace.Clockwise,
        };
    }

    public void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, GlobalSet globalSet, MainPassSet renderPassSet)
    {
        if (cl == null) throw new ArgumentNullException(nameof(cl));
        if (globalSet == null) throw new ArgumentNullException(nameof(globalSet));
        if (renderPassSet == null) throw new ArgumentNullException(nameof(renderPassSet));
        if (renderable is not VeldridSpriteBatch<SpriteInfo, GpuSpriteInstanceData> batch)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        cl.PushDebugGroup(batch.Name);
        if (batch.Key.ScissorRegion.HasValue)
        {
            var rect = batch.Key.ScissorRegion.Value;
            cl.SetScissorRect(0, (uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height);
        }

        cl.SetPipeline(
            (batch.Key.Flags & SpriteKeyFlags.NoDepthTest) != 0
                ? _noCullPipeline.Pipeline 
                : _pipeline.Pipeline);

        cl.SetGraphicsResourceSet(0, globalSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, renderPassSet.ResourceSet);
        cl.SetGraphicsResourceSet(2, batch.SpriteResources.ResourceSet);
        cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
        cl.SetVertexBuffer(1, batch.Instances.DeviceBuffer);
        cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);

        cl.DrawIndexed((uint)Indices.Length, (uint)batch.ActiveInstances, 0, 0, 0);

        if (batch.Key.ScissorRegion.HasValue)
            cl.SetFullScissorRect(0);
        cl.PopDebugGroup();
    }

    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _pipeline.Dispose();
        _noCullPipeline.Dispose();
    }
}

[VertexShader(typeof(SpriteVertexShader))]
[FragmentShader(typeof(SpriteFragmentShader))]
internal partial class SpritePipeline : PipelineHolder { }

[Name("SpriteSF.frag")]
[Input(0, typeof(SpriteIntermediateData))]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(SpriteSet))]
[Output(0, typeof(SimpleFramebuffer))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal partial class SpriteFragmentShader : IFragmentShader { }

[Name("SpriteSV.vert")]
[Input(0, typeof(Vertex2DTextured))]
[Input(1, typeof(GpuSpriteInstanceData), InstanceStep = 1)]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(SpriteSet))]
[Output(0, typeof(SpriteIntermediateData))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal partial class SpriteVertexShader : IVertexShader { }

internal sealed partial class SpriteSet : ResourceSetHolder
{
    [Texture("uSprite")] ITextureHolder _texture; // Only one of texture & textureArray will be used at a time
    [TextureArray("uSpriteArray")] ITextureArrayHolder _textureArray;
    [Sampler("uSpriteSampler")] ISamplerHolder _sampler;
    [UniformBuffer("_Uniform")] IBufferHolder<SpriteUniform> _uniform;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SpriteUniform  : IUniformFormat // Length must be multiple of 16
{
    [Uniform("uTexSize")] public Vector2 TextureSize { get; set; } // 8 bytes
    [Uniform("uFlags", EnumPrefix = "SKF")] public SpriteKeyFlags Flags { get; set; } // 4 bytes
    [Uniform("_pad1")] uint Padding { get; set; } // 4 bytes
}

// ReSharper disable UnusedMember.Global
#pragma warning disable 649 // CS0649 Field is never assigned to, and will always have its default value
#pragma warning disable CA1051 // Do not declare visible instance fields
internal partial struct GpuSpriteInstanceData : IVertexFormat
{
    [Vertex("Flags", EnumPrefix = "SF")] public SpriteFlags Flags;
    [Vertex("InstancePos")] public Vector4 Position;
    [Vertex("Size")]        public Vector2 Size;
    [Vertex("TexOffset")]   public Vector2 TexPosition; // Normalised texture coordinates
    [Vertex("TexSize")]     public Vector2 TexSize; // Normalised texture coordinates
    [Vertex("TexLayer")]    public uint TexLayer;
}
#pragma warning restore CA1051 // Do not declare visible instance fields

internal partial struct SpriteIntermediateData : IVertexFormat
{
    [Vertex("TexPosition")] public Vector2 TexturePosition;
    [Vertex("Layer", Flat = true)] public float TextureLayer;
    [Vertex("UvClamp")] public Vector4 UvClamp;
    [Vertex("Flags", Flat = true, EnumPrefix = "SF")] public SpriteFlags Flags;
    [Vertex("NormCoords")] public Vector2 NormalisedSpriteCoordinates;
    [Vertex("WorldPosition")] public Vector3 WorldPosition;
}
#pragma warning restore 649
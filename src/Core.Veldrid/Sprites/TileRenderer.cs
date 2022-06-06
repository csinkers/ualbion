using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

public sealed class TileRenderer : Component, IRenderer, IDisposable
{
    public Type[] HandledTypes { get; } = { typeof(TileLayerRenderable) };
    static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
    static readonly Vertex2D[] Vertices =
    {
        new(0.0f, 0.0f), new(1.0f, 0.0f),
        new(0.0f, 1.0f), new(1.0f, 1.0f),
    };

    readonly TilePipeline _pipeline;
    readonly MultiBuffer<Vertex2D> _vertexBuffer;
    readonly MultiBuffer<ushort> _indexBuffer;

    public TileRenderer(IFramebufferHolder framebuffer) {
        _vertexBuffer = new MultiBuffer<Vertex2D>(Vertices, BufferUsage.VertexBuffer, "TilesVertexBuffer");
        _indexBuffer = new MultiBuffer<ushort>(Indices, BufferUsage.IndexBuffer, "TilesIndexBuffer");
        _pipeline = new()
            {
                Name = "P:Tile",
                AlphaBlend = BlendStateDescription.SingleAlphaBlend,
                CullMode = FaceCullMode.None,
                DepthStencilMode = new DepthStencilStateDescription
                {
                    DepthTestEnabled = true,
                    DepthWriteEnabled = true,
                    DepthComparison = ComparisonKind.Always
                },
                FillMode = PolygonFillMode.Solid,
                Framebuffer = framebuffer,
                Topology = PrimitiveTopology.TriangleList,
                UseDepthTest = true,
                UseScissorTest = true,
                Winding = FrontFace.Clockwise,
            };

        AttachChild(_vertexBuffer);
        AttachChild(_indexBuffer);
        AttachChild(_pipeline);
    }

    public void Render(IRenderable renderable, CommonSet commonSet, IFramebufferHolder framebuffer, CommandList cl, GraphicsDevice device)
    {
        if (cl == null) throw new ArgumentNullException(nameof(cl));
        if (commonSet == null) throw new ArgumentNullException(nameof(commonSet));
        if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));
        if (renderable is not TileLayerRenderable tileLayer)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        if (!tileLayer.IsActive)
            return;

        cl.PushDebugGroup(tileLayer.Name);
        cl.SetPipeline(_pipeline.Pipeline);

        cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
        cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);
        cl.SetFramebuffer(framebuffer.Framebuffer);

        cl.SetGraphicsResourceSet(0, commonSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, tileLayer.Tileset.Resources.ResourceSet);
        cl.SetGraphicsResourceSet(2, tileLayer.Resources.ResourceSet);
        cl.DrawIndexed((uint)Indices.Length);

        cl.PopDebugGroup();
    }

    public void Dispose()
    {
        _vertexBuffer.Dispose();
        _indexBuffer.Dispose();
        _pipeline.Dispose();
    }
}

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1815
#pragma warning disable CS0649

[VertexShader(typeof(TileVertexShader))]
[FragmentShader(typeof(TileFragmentShader))]
internal partial class TilePipeline : PipelineHolder { }

[Name("TilesSF.frag")]
[Input(0, typeof(TileIntermediateData))]
[ResourceSet(0, typeof(CommonSet))]
[ResourceSet(1, typeof(TilesetResourceSet))]
[ResourceSet(2, typeof(TileLayerResourceSet))]
[Output(0, typeof(SimpleFramebuffer))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal partial class TileFragmentShader : IFragmentShader { }

[Name("TilesSV.vert")]
[Input(0, typeof(Vertex2D))]
[ResourceSet(0, typeof(CommonSet))]
[ResourceSet(1, typeof(TilesetResourceSet))]
[ResourceSet(2, typeof(TileLayerResourceSet))]
[Output(0, typeof(TileIntermediateData))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal partial class TileVertexShader : IVertexShader { }

internal sealed partial class TilesetResourceSet : ResourceSetHolder
{
    [Texture("uTile", ShaderStages.Fragment)]            ITextureHolder _texture; // Only one of texture & textureArray will be used at a time
    [TextureArray("uTileArray", ShaderStages.Fragment)]  ITextureArrayHolder _textureArray;
    [Sampler("uTileSampler", ShaderStages.Fragment)]     ISamplerHolder _sampler;
    [UniformBuffer("_SetUniform")]                       IBufferHolder<TilesetUniform> _uniform;
    [StructuredBuffer("Tiles", ShaderStages.Fragment)]   IBufferHolder<GpuTileData> _tiles;
    [StructuredBuffer("Regions", ShaderStages.Fragment)] IBufferHolder<GpuTextureRegion> _regions;
}

internal sealed partial class TileLayerResourceSet : ResourceSetHolder
{
    [UniformBuffer("_LayerUniform")]                 IBufferHolder<TileLayerUniform> _uniform;
    [StructuredBuffer("Map", ShaderStages.Fragment)] IBufferHolder<GpuMapTile> _map;
}

[Flags]
public enum GpuTileFlags : uint
{
    Bouncy = 1,
    NoDraw = 2,
    UseUnderlay = 4,
}

[StructLayout(LayoutKind.Sequential)]
public struct GpuTileData : IStructuredFormat
{
    [Structured("Layer")] public uint Layer; // 0
    [Structured("Type")] public uint Type; // 1
    [Structured("FrameCount")] public uint FrameCount; // 2
    [Structured("Flags", EnumPrefix = "TF")] public GpuTileFlags Flags; // 3

    [Structured("DayImage")] public uint DayImage; // 4
    [Structured("NightImage")] public uint NightImage; // 6

    [Structured("Unk7")] public uint Unk7; // 8
    [Structured("PalFrames")] public uint PalFrames; // 9

    public override string ToString() => $"D{DayImage} N{NightImage} F{FrameCount} P{PalFrames} L{Layer} T{Type} 7:{Unk7}";
}

[StructLayout(LayoutKind.Sequential)]
public struct GpuTextureRegion : IStructuredFormat
{
    [Structured("Offset")] public Vector4 Offset; // xy = uv offset, z = layer
}

[StructLayout(LayoutKind.Sequential)]
public struct GpuMapTile : IStructuredFormat // Needs to match MapTile, i.e. uint
{
    [Structured("Tile")] public uint Tile;
}

[Flags]
public enum GpuTilesetFlags : uint
{
    UseArray = 1,
    UsePalette = 2,
    UseBlend = 4,
}

[Flags]
public enum GpuTileLayerFlags : uint
{
    DrawUnderlay = 1,
    DrawOverlay = 2,
    OpaqueUnderlay = 4,
}

[StructLayout(LayoutKind.Sequential)]
internal struct TilesetUniform : IUniformFormat // Length must be multiple of 16
{
    [Uniform("uTileWorldSize")] public Vector2 TileWorldSize { get; set; } // 8 bytes
    [Uniform("uTileUvSize")] public Vector2 TileUvSize { get; set; } // 8 bytes
    [Uniform("uTilesetFlags", EnumPrefix = "TSF")] public GpuTilesetFlags Flags { get; set; } // 4 byte
    [Uniform("uPad1")] uint _Pad1 { get; set; } // 4 bytes
    [Uniform("uPad2")] Vector2 _Pad2 { get; set; } // 8 bytes
}

[StructLayout(LayoutKind.Sequential)]
internal struct TileLayerUniform : IUniformFormat // Length must be multiple of 16
{
    [Uniform("uMapWidth")] public uint MapWidth { get; set; } // 4 bytes
    [Uniform("uMapHeight")] public uint MapHeight { get; set; } // 4 bytes
    [Uniform("uFrame")] public int FrameNumber { get; set; } // 4 bytes
    [Uniform("uLayerFlags", EnumPrefix = "TLF")] public GpuTileLayerFlags LayerFlags { get; set; } // 4 bytes
}

internal partial struct TileIntermediateData : IVertexFormat
{
    [Vertex("WorldPosition")] public Vector4 WorldPos;
    [Vertex("TilePosition")] public Vector2 TilePos;
}

#pragma warning restore CS0649
#pragma warning restore CA1815
#pragma warning restore CA1051 // Do not declare visible instance fields

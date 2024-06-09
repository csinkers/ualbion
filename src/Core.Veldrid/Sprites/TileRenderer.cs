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
    public Type[] HandledTypes { get; } = [typeof(TileLayerRenderable)];
    static readonly ushort[] Indices = [0, 1, 2, 2, 1, 3];
    static readonly Vertex2D[] Vertices =
    [
        new(0.0f, 0.0f), new(1.0f, 0.0f),
        new(0.0f, 1.0f), new(1.0f, 1.0f)
    ];

    readonly TilePipeline _pipeline;
    readonly MultiBuffer<Vertex2D> _vertexBuffer;
    readonly MultiBuffer<ushort> _indexBuffer;

    public TileRenderer(in OutputDescription outputFormat) {
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
                OutputDescription = outputFormat,
                Topology = PrimitiveTopology.TriangleList,
                UseDepthTest = true,
                UseScissorTest = true,
                Winding = FrontFace.Clockwise,
            };

        AttachChild(_vertexBuffer);
        AttachChild(_indexBuffer);
        AttachChild(_pipeline);
    }

    public void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, IResourceSetHolder set1, IResourceSetHolder set2)
    {
        var globalSet = (GlobalSet)set1 ?? throw new ArgumentNullException(nameof(set1));
        var renderPassSet = (MainPassSet)set2 ?? throw new ArgumentNullException(nameof(set2));

        ArgumentNullException.ThrowIfNull(cl);
        if (renderable is not TileLayerRenderable tileLayer)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        if (!tileLayer.IsActive)
            return;

        cl.PushDebugGroup(tileLayer.Name);
        cl.SetPipeline(_pipeline.Pipeline);

        cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
        cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);

        cl.SetGraphicsResourceSet(0, globalSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, renderPassSet.ResourceSet);
        cl.SetGraphicsResourceSet(2, tileLayer.Tileset.Resources.ResourceSet);
        cl.SetGraphicsResourceSet(3, tileLayer.Resources.ResourceSet);
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
internal sealed partial class TilePipeline : PipelineHolder { }

[Name("TilesSF.frag")]
[Input(0, typeof(TileIntermediateData))]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(TilesetResourceSet))]
[ResourceSet(3, typeof(TileLayerResourceSet))]
[Output(0, typeof(SimpleFramebuffer))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal sealed partial class TileFragmentShader : IFragmentShader { }

[Name("TilesSV.vert")]
[Input(0, typeof(Vertex2D))]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(TilesetResourceSet))]
[ResourceSet(3, typeof(TileLayerResourceSet))]
[Output(0, typeof(TileIntermediateData))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
internal sealed partial class TileVertexShader : IVertexShader { }

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
    TypeMask = Type1 | Type2 | Type4,
    LayerMask = Layer1 | Layer2,
    CollMask = CollTop | CollRight | CollBottom | CollLeft | Solid,
    SitMask = Sit1 | Sit2 | Sit4 | Sit8,
    MiscMask = Bouncy | UseUnderlayFlags | Unk12 | Unk18 | NoDraw | DebugDot,

    // Animation cycle options
    Bouncy = 1, // 0x00000001 - Animation bounces back and forth from first to last frame instead of starting over again
    UseUnderlayFlags = 1U << 1, // 0x00000002 a small orange debug marker shown in top left when this is set on an overlay tile
    Type1      = 1U << 2,  // 0x00000004  
    Type2      = 1U << 3,  // 0x00000008  
    Type4      = 1U << 4,  // 0x00000010  

    // Layering options - Used to set tile ordering relative to the top/middle/bottom of NPC/player sprites
    Layer1     = 1U << 5,  // 0x00000020
    Layer2     = 1U << 6,  // 0x00000040  

    // Collision options
    CollTop    = 1U << 7,  // 0x00000080  Overlay: Orange line marker on top side
    CollRight  = 1U << 8,  // 0x00000100  Overlay: Orange line marker on right side
    CollBottom = 1U << 9,  // 0x00000200  Overlay: Orange line marker on bottom side
    CollLeft   = 1U << 10, // 0x00000400  Overlay: Orange line marker on left side
    Solid      = 1U << 11, // 0x00000800  Underlay: White solid debug marker. Overlay: Orange solid debug marker.

    Unk12      = 1U << 12, // 0x00001000  
    Unk18      = 1U << 18, // 0x00040000  

    // Debug options
    NoDraw     = 1U << 21, // 0x00200000  
    DebugDot   = 1U << 22, // 0x00400000  Orange dot in lower right, both layers.

    // Sitting options
    Sit1       = 1U << 23, // 0x00800000  
    Sit2       = 1U << 24, // 0x01000000  
    Sit4       = 1U << 25, // 0x02000000  
    Sit8       = 1U << 26, // 0x04000000  
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
    DrawUnderlay   = 1,
    DrawOverlay    = 2,
    OpaqueUnderlay = 4,
    DrawCollision  = 8,
    DrawSitState   = 0x10,
    DrawMisc       = 0x20,
    DrawZones      = 0x40,
    DrawDebug      = 0x80,
}

[StructLayout(LayoutKind.Sequential)]
internal struct TilesetUniform : IUniformFormat // Length must be multiple of 16
{
    [Uniform("uTileWorldSize")] public Vector2 TileWorldSize { get; set; } // 8 bytes
    [Uniform("uTileUvSize")] public Vector2 TileUvSize { get; set; } // 8 bytes
    [Uniform("uTilesetFlags", EnumPrefix = "TSF")] public GpuTilesetFlags Flags { get; set; } // 4 byte

    // ReSharper disable InconsistentNaming
    // ReSharper disable UnusedMember.Local
    [Uniform("uPad1")] uint _Pad1 { get; set; } // 4 bytes
    [Uniform("uPad2")] Vector2 _Pad2 { get; set; } // 8 bytes
    // ReSharper restore UnusedMember.Local
    // ReSharper restore InconsistentNaming
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

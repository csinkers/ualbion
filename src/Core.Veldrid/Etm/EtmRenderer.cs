using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Api.Eventing;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Etm;

public sealed class EtmRenderer : Component, IRenderer, IDisposable
{
    readonly MultiBuffer<Vertex3DTextured> _vertexBuffer;
    readonly MultiBuffer<ushort> _indexBuffer;
    readonly EtmPipeline _normalPipeline;
    readonly EtmPipeline _nonCullingPipeline;

    public Type[] HandledTypes { get; } = [typeof(EtmWindow)];

    public EtmRenderer(in OutputDescription outputFormat)
    {
        _vertexBuffer = new MultiBuffer<Vertex3DTextured>(Cube.Vertices, BufferUsage.VertexBuffer) { Name = "TileMapVertexBuffer"};
        _indexBuffer = new MultiBuffer<ushort>(Cube.Indices, BufferUsage.IndexBuffer) { Name = "TileMapIndexBuffer"};
        _normalPipeline = BuildPipeline("P_TileMapRenderer", FaceCullMode.Back, outputFormat);
        _nonCullingPipeline = BuildPipeline("P_TileMapRendererNoCull", FaceCullMode.None, outputFormat);
        AttachChild(_vertexBuffer);
        AttachChild(_indexBuffer);
        AttachChild(_normalPipeline);
        AttachChild(_nonCullingPipeline);
    }

    static EtmPipeline BuildPipeline(string name, FaceCullMode cullMode, in OutputDescription outputFormat)
        => new()
        {
            AlphaBlend = BlendStateDescription.SingleAlphaBlend,
            CullMode = cullMode,
            DepthStencilMode = DepthStencilStateDescription.DepthOnlyLessEqual,
            FillMode = PolygonFillMode.Solid,
            OutputDescription = outputFormat,
            Name = name,
            Topology = PrimitiveTopology.TriangleList,
            UseDepthTest = true,
            UseScissorTest = false,
            Winding = FrontFace.CounterClockwise,
        };

    public void Render(IRenderable renderable, CommandList cl, GraphicsDevice device, IResourceSetHolder set1, IResourceSetHolder set2)
    {
        var globalSet = (GlobalSet)set1 ?? throw new ArgumentNullException(nameof(set1));
        var renderPassSet = (MainPassSet)set2 ?? throw new ArgumentNullException(nameof(set2));

        ArgumentNullException.ThrowIfNull(cl);
        if (renderable is not EtmWindow window)
            throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

        var tilemap = window.Tilemap;
        cl.PushDebugGroup(tilemap.Name);

        cl.SetPipeline(tilemap.RendererId == DungeonTilemapPipeline.NoCulling 
            ? _nonCullingPipeline.Pipeline 
            : _normalPipeline.Pipeline);

        cl.SetGraphicsResourceSet(0, globalSet.ResourceSet);
        cl.SetGraphicsResourceSet(1, renderPassSet.ResourceSet);
        cl.SetGraphicsResourceSet(2, tilemap.ResourceSet.ResourceSet);
        cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
        cl.SetVertexBuffer(1, tilemap.TileBuffer);
        cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);

        cl.DrawIndexed((uint)Cube.Indices.Length, (uint)tilemap.Tiles.Length, 0, 0, 0);
        cl.PopDebugGroup();
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _normalPipeline?.Dispose();
        _nonCullingPipeline?.Dispose();
    }
}

[VertexShader(typeof(EtmVertexShader))]
[FragmentShader(typeof(EtmFragmentShader))]
sealed partial class EtmPipeline : PipelineHolder { }

sealed partial class EtmSet : ResourceSetHolder
{
    [UniformBuffer("Properties", ShaderStages.Vertex)] IBufferHolder<DungeonTileMapProperties> _properties;
    [TextureArray("DayFloors", ShaderStages.Fragment)] ITextureArrayHolder _dayFloors;
    [TextureArray("DayWalls", ShaderStages.Fragment)] ITextureArrayHolder _dayWalls;
    [TextureArray("NightFloors", ShaderStages.Fragment)] ITextureArrayHolder _nightFloors;
    [TextureArray("NightWalls", ShaderStages.Fragment)] ITextureArrayHolder _nightWalls;
    [Sampler("TextureSampler", ShaderStages.Fragment)] ISamplerHolder _textureSampler;
}

[Name("ExtrudedTileMapSV.vert")]
[Input(0, typeof(Vertex3DTextured))]
[Input(1, typeof(DungeonTile), InstanceStep = 1)]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(EtmSet))]
[Output(0, typeof(EtmIntermediate))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
sealed partial class EtmVertexShader : IVertexShader { }

[Name( "ExtrudedTileMapSF.frag")]
[Input(0, typeof(EtmIntermediate))]
[ResourceSet(0, typeof(GlobalSet))]
[ResourceSet(1, typeof(MainPassSet))]
[ResourceSet(2, typeof(EtmSet))]
[Output(0, typeof(SimpleFramebuffer))]
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
sealed partial class EtmFragmentShader : IFragmentShader { }

#pragma warning disable CA1815 // Override equals and operator equals on value types
[StructLayout(LayoutKind.Sequential)]
public struct DungeonTileMapProperties : IUniformFormat
{
    [Uniform("uScale")]            	public Vector4 Scale { get; set; }
    [Uniform("uRotation")]         	public Vector4 Rotation { get; set; }
    [Uniform("uOrigin")]           	public Vector4 Origin { get; set; }
    [Uniform("uHorizontalSpacing")]	public Vector4 HorizontalSpacing { get; set; }
    [Uniform("uVerticalSpacing")]  	public Vector4 VerticalSpacing { get; set; }
    [Uniform("uWidth")]            	public uint Width { get; set; }
    [Uniform("uAmbient")]          	public uint AmbientLightLevel { get; set; }
    [Uniform("uFogColor")]         	public uint FogColor { get; set; }
    [Uniform("uYScale")]            public float ObjectYScaling { get; set; }

    public DungeonTileMapProperties(
        Vector3 scale,
        Vector3 rotation,
        Vector3 origin,
        Vector3 horizontalSpacing,
        Vector3 verticalSpacing,
        uint width,
        uint ambientLightLevel,
        uint fogColor,
        float objectYScaling)
    {
        Scale = new Vector4(scale, 1);
        Rotation = new Vector4(rotation, 1);
        Origin = new Vector4(origin, 1);
        HorizontalSpacing = new Vector4(horizontalSpacing, 1);
        VerticalSpacing = new Vector4(verticalSpacing, 1);
        Width = width;
        AmbientLightLevel = ambientLightLevel;
        FogColor = fogColor;
        ObjectYScaling = objectYScaling;
    }
}

public partial struct DungeonTile : IVertexFormat, IEquatable<DungeonTile>
{
    [Vertex("Textures")] public uint Textures { get; set; }
    [Vertex("WallSize")] public Vector2 WallSize { get; set; }
    [Vertex("Flags", EnumPrefix = "TF")] public DungeonTileFlags Flags { get; set; }

    public byte Floor // 0 = No floor
    {
        get => (byte)(Textures & 0xff);
        set => Textures = (Textures & 0xffffff00) | value;
    }

    public byte Ceiling // 0 = No Ceiling
    {
        get => (byte)((Textures >> 8) & 0xff);
        set => Textures = (Textures & 0xffff00ff) | ((uint)value << 8);
    }

    public byte Wall // 0 = No Wall
    {
        get => (byte)((Textures >> 16) & 0xff);
        set => Textures = (Textures & 0xff00ffff) | ((uint)value << 16);
    }

    public byte Overlay // 0 = No overlay
    {
        get => (byte)((Textures >> 24) & 0xff);
        set => Textures = (Textures & 0x00ffffff) | ((uint)value << 24);
    }

    public override string ToString() => $"{Floor}.{Ceiling}.{Wall} ({Flags})";
    public override bool Equals(object obj) => obj is DungeonTile other && Equals(other);

    public bool Equals(DungeonTile other) =>
        Floor == other.Floor &&
        Ceiling == other.Ceiling && 
        Wall == other.Wall &&
        Flags == other.Flags && 
        WallSize == other.WallSize;

    public override int GetHashCode() => HashCode.Combine(Textures, Flags, WallSize);
    public static bool operator ==(DungeonTile left, DungeonTile right) => left.Equals(right);
    public static bool operator !=(DungeonTile left, DungeonTile right) => !(left == right);
}

#pragma warning restore CA1815 // Override equals and operator equals on value types
[SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
sealed partial class EtmIntermediate : IVertexFormat
{
#pragma warning disable 649
    [Vertex("TexCoords")] public Vector2 TextureCordinates;
    [Vertex("Textures", Flat = true)] public uint Textures;
    [Vertex("Flags", EnumPrefix = "TF", Flat = true)] public DungeonTileFlags Flags;
#pragma warning restore 649
}

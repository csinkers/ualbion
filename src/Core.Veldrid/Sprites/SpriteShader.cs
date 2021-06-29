using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites
{
#pragma warning disable CA1051 // Do not declare visible instance fields
    [VertexShader(typeof(SpriteVertexShader))]
    [FragmentShader(typeof(SpriteFragmentShader))]
    public partial class SpritePipeline : PipelineHolder { }

    [Name("SpriteSV.vert")]
    [Input(0, typeof(Vertex2DTextured))]
    [Input(1, typeof(GpuSpriteInstanceData), InstanceStep = 1)]
    [ResourceSet(0, typeof(CommonSet))]
    [ResourceSet(1, typeof(SpriteArraySet))]
    [Output(0, typeof(SpriteIntermediateData))]
    public partial class SpriteVertexShader : IVertexShader { }

    [Name("SpriteSF.frag")]
    [Input(0, typeof(SpriteIntermediateData))]
    [ResourceSet(0, typeof(CommonSet))]
    [ResourceSet(1, typeof(SpriteArraySet))]
    [Output(0, typeof(ColorOnly))]
    public partial class SpriteFragmentShader : IFragmentShader { }

    public sealed partial class SpriteArraySet : ResourceSetHolder
    {
        [Resource("uSprite")] Texture2DArrayHolder _texture;
        [Resource("uSpriteSampler")] SamplerHolder _sampler;
        [Resource("_Uniform")] SingleBuffer<SpriteUniform> _uniform;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SpriteUniform  : IUniformFormat // Length must be multiple of 16
    {
        [Uniform("uFlags", EnumPrefix = "SKF")] public SpriteKeyFlags Flags { get; set; } // 4 bytes
        [Uniform("uTexSizeW")] public float TextureWidth { get; set; } // 4 bytes
        [Uniform("uTexSizeH")] public float TextureHeight { get; set; } // 4 bytes
        [Uniform("_pad1")] uint Padding { get; set; } // 4 bytes
    }

    public partial struct SpriteIntermediateData : IVertexFormat
    {
        [Vertex("TexPosition")] public Vector2 TexturePosition;
        [Vertex("Layer", Flat = true)] public float TextureLayer;
        [Vertex("Flags", Flat = true, EnumPrefix = "SF")] public SpriteFlags Flags;
        [Vertex("NormCoords")] public Vector2 NormalisedSpriteCoordinates;
        [Vertex("WorldPosition")] public Vector3 WorldPosition;
    }

    public partial struct ColorOnly : IVertexFormat
    {
        [Vertex("Color")] public Vector4 OutputColor;
    }

    public partial struct GpuSpriteInstanceData : IVertexFormat
    {
        [Vertex("InstancePos")]  public Vector4 Position;
        [Vertex("Size")]      public Vector2 Size;
        [Vertex("TexOffset")] public Vector2 TexPosition; // Normalised texture coordinates
        [Vertex("TexSize")]   public Vector2 TexSize; // Normalised texture coordinates
        [Vertex("TexLayer")]  public uint TexLayer;
        [Vertex("Flags", EnumPrefix = "SF")] public SpriteFlags Flags;
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites
{
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable 649 // CS0649 Field is never assigned to, and will always have its default value
    [VertexShader(typeof(SpriteVertexShader))]
    [FragmentShader(typeof(SpriteFragmentShader))]
    internal partial class SpritePipeline : PipelineHolder { }

    [Name("SpriteSV.vert")]
    [Input(0, typeof(Vertex2DTextured))]
    [Input(1, typeof(GpuSpriteInstanceData), InstanceStep = 1)]
    [ResourceSet(0, typeof(CommonSet))]
    [ResourceSet(1, typeof(SpriteArraySet))]
    [Output(0, typeof(SpriteIntermediateData))]
    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    internal partial class SpriteVertexShader : IVertexShader { }

    [Name("SpriteSF.frag")]
    [Input(0, typeof(SpriteIntermediateData))]
    [ResourceSet(0, typeof(CommonSet))]
    [ResourceSet(1, typeof(SpriteArraySet))]
    [Output(0, typeof(SimpleFramebuffer))]
    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    internal partial class SpriteFragmentShader : IFragmentShader { }

    internal sealed partial class SpriteArraySet : ResourceSetHolder
    {
        [Resource("uSprite")] ITextureArrayHolder _texture;
        [Resource("uSpriteSampler")] ISamplerHolder _sampler;
        [Resource("_Uniform")] IBufferHolder<SpriteUniform> _uniform;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SpriteUniform  : IUniformFormat // Length must be multiple of 16
    {
        [Uniform("uFlags", EnumPrefix = "SKF")] public SpriteKeyFlags Flags { get; set; } // 4 bytes
        [Uniform("uTexSizeW")] public float TextureWidth { get; set; } // 4 bytes
        [Uniform("uTexSizeH")] public float TextureHeight { get; set; } // 4 bytes
        [Uniform("_pad1")] uint Padding { get; set; } // 4 bytes
    }

    internal partial struct SpriteIntermediateData : IVertexFormat
    {
        [Vertex("TexPosition")] public Vector2 TexturePosition;
        [Vertex("Layer", Flat = true)] public float TextureLayer;
        [Vertex("Flags", Flat = true, EnumPrefix = "SF")] public SpriteFlags Flags;
        [Vertex("NormCoords")] public Vector2 NormalisedSpriteCoordinates;
        [Vertex("WorldPosition")] public Vector3 WorldPosition;
    }

    internal partial struct GpuSpriteInstanceData : IVertexFormat
    {
        [Vertex("InstancePos")]  public Vector4 Position;
        [Vertex("Size")]      public Vector2 Size;
        [Vertex("TexOffset")] public Vector2 TexPosition; // Normalised texture coordinates
        [Vertex("TexSize")]   public Vector2 TexSize; // Normalised texture coordinates
        [Vertex("TexLayer")]  public uint TexLayer;
        [Vertex("Flags", EnumPrefix = "SF")] public SpriteFlags Flags;
    }
#pragma warning restore 649
#pragma warning restore CA1051 // Do not declare visible instance fields
}

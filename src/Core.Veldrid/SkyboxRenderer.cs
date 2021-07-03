using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
#pragma warning disable 649
    [VertexShader(typeof(SkyboxVertexShader))]
    [FragmentShader(typeof(SkyboxFragmentShader))]
    partial class SkyboxPipeline : PipelineHolder
    {
    }

    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    partial struct SkyboxIntermediate : IVertexFormat
    {
        [Vertex("TexPosition")] public Vector2 TextureCoordinates;
        [Vertex("NormCoords")] public Vector2 NormalisedSpriteCoordinates;
        [Vertex("WorldPosition")] public Vector3 WorldPosition;
    }

    [Name("SkyBoxSV.vert")]
    [Input(0, typeof(Vertex2DTextured))]
    [ResourceSet(0, typeof(SkyboxResourceSet))]
    [Output(0, typeof(SkyboxIntermediate))]
    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    internal partial class SkyboxVertexShader : IVertexShader { }

    [Name("SkyBoxSF.frag")]
    [Input(0, typeof(SkyboxIntermediate))]
    [ResourceSet(0, typeof(SkyboxResourceSet))]
    [ResourceSet(1, typeof(CommonSet))]
    [Output(0, typeof(ColorOnly))]
    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    internal partial class SkyboxFragmentShader : IFragmentShader { }

    partial class SkyboxResourceSet : ResourceSetHolder
    {
        [Resource("uSampler", ShaderStages.Fragment)] ISamplerHolder _sampler;
        [Resource("uTexture", ShaderStages.Fragment)] ITextureHolder _texture;
        [Resource("_Uniform", ShaderStages.Vertex)] IBufferHolder<SkyboxUniformInfo> _uniform;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SkyboxUniformInfo // Length must be multiple of 16
    {
        [Uniform("uYaw")] public float uYaw; // 4
        [Uniform("uPitch")] public float uPitch;  // 8
        [Uniform("uVisibleProportion")] public float uVisibleProportion;  // 12
        [Uniform("_pad1")] readonly uint _pad1;   // 16
    }
#pragma warning restore 649

    public sealed class SkyboxRenderer : Component, IRenderer, IDisposable
    {
        static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
        static readonly Vertex2DTextured[] Vertices =
        {
            new (-1.0f, -1.0f, 0.0f, 0.0f), new (1.0f, -1.0f, 1.0f, 0.0f),
            new (-1.0f, 1.0f, 0.0f, 1.0f), new (1.0f, 1.0f, 1.0f, 1.0f),
        };

        readonly SkyboxPipeline _pipeline;
        readonly MultiBuffer<Vertex2DTextured> _vertexBuffer;
        readonly MultiBuffer<ushort> _indexBuffer;

        static SkyboxPipeline BuildPipeline(IFramebufferHolder framebuffer) => new()
            {
                Name = "P_Skybox",
                AlphaBlend = BlendStateDescription.SingleDisabled,
                CullMode = FaceCullMode.None,
                FillMode = PolygonFillMode.Solid,
                Framebuffer = framebuffer,
                DepthStencilMode = DepthStencilStateDescription.Disabled,
                Winding = FrontFace.Clockwise,
                UseDepthTest = false,
                UseScissorTest = true,
                Topology = PrimitiveTopology.TriangleList,
            };

        public SkyboxRenderer(IFramebufferHolder framebuffer)
        {
            _vertexBuffer = new MultiBuffer<Vertex2DTextured>(Vertices, BufferUsage.VertexBuffer, "SpriteVertexBuffer");
            _indexBuffer = new MultiBuffer<ushort>(Indices, BufferUsage.IndexBuffer, "SpriteIndexBuffer");
            _pipeline = BuildPipeline(framebuffer);
            AttachChild(_vertexBuffer);
            AttachChild(_indexBuffer);
            AttachChild(_pipeline);
        }

        public void Render(IRenderable renderable, CommonSet commonSet, IFramebufferHolder framebuffer, CommandList cl, GraphicsDevice device)
        {
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            if (commonSet == null) throw new ArgumentNullException(nameof(commonSet));
            if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));
            if (renderable is not Skybox skybox)
                throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

            cl.PushDebugGroup(skybox.Name);

            cl.SetPipeline(_pipeline.Pipeline);
            cl.SetGraphicsResourceSet(0, skybox.ResourceSet.ResourceSet);
            cl.SetGraphicsResourceSet(1, commonSet.ResourceSet);
            cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
            cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);
            cl.SetFramebuffer(framebuffer.Framebuffer);

            cl.DrawIndexed((uint)Indices.Length, 1, 0, 0, 0);
            cl.PopDebugGroup();
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _pipeline?.Dispose();
        }
    }
}


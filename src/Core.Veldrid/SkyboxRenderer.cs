using System;
using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Core.Veldrid.Sprites;
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
    public partial class SkyboxVertexShader : IVertexShader { }

    [Name("SkyBoxSF.frag")]
    [Input(0, typeof(SkyboxIntermediate))]
    [ResourceSet(0, typeof(SkyboxResourceSet))]
    [ResourceSet(1, typeof(CommonSet))]
    [Output(0, typeof(ColorOnly))]
    public partial class SkyboxFragmentShader : IFragmentShader { }

    partial class SkyboxResourceSet : ResourceSetHolder
    {
        [Resource("uSampler", ShaderStages.Fragment)] SamplerHolder _sampler;
        [Resource("uTexture", ShaderStages.Fragment)] Texture2DHolder _texture;
        [Resource("_Uniform", ShaderStages.Vertex)] SingleBuffer<SkyboxUniformInfo> _uniform;
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

    public sealed class SkyboxRenderer : Component
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
            _vertexBuffer = AttachChild(new MultiBuffer<Vertex2DTextured>(Vertices, BufferUsage.VertexBuffer, "SpriteVertexBuffer"));
            _indexBuffer = AttachChild(new MultiBuffer<ushort>(Indices, BufferUsage.IndexBuffer, "SpriteIndexBuffer"));
            _pipeline = AttachChild(BuildPipeline(framebuffer));
        }

        public void Render(CommandList cl, Skybox skybox, CommonSet commonSet, IFramebufferHolder framebuffer)
        {
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            if (skybox == null) throw new ArgumentNullException(nameof(skybox));
            if (commonSet == null) throw new ArgumentNullException(nameof(commonSet));
            if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));

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
            _pipeline?.Dispose();
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }
    }
}


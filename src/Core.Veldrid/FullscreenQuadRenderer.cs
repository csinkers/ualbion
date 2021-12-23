using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public sealed class FullscreenQuad : Component, IRenderable, IDisposable
    {
        readonly SingleBuffer<FullscreenQuadUniformInfo> _uniform;

        public string Name { get; }
        public DrawLayer RenderOrder { get; }
        public ITextureHolder Source { get; }
        public Vector4 NormalisedDestRectangle
        {
            get => _uniform.Data.uRect;
            set => _uniform.Data = new FullscreenQuadUniformInfo { uRect = value };
        }
        internal FullscreenQuadResourceSet ResourceSet;

        public FullscreenQuad(string name,
            DrawLayer renderOrder,
            ITextureHolder source,
            Vector4 normalisedDestWindowXYWH)
        {
            Name = name;
            RenderOrder = renderOrder;
            Source = source ?? throw new ArgumentNullException(nameof(source));
            _uniform = new SingleBuffer<FullscreenQuadUniformInfo>(new FullscreenQuadUniformInfo
            {
                uRect = normalisedDestWindowXYWH
            }, BufferUsage.UniformBuffer);

            AttachChild(_uniform);
        }

        protected override void Subscribed()
        {
            base.Subscribed();
            ResourceSet = new FullscreenQuadResourceSet
            {
                Name = "RS_" + Name,
                Texture = Source,
                Uniform = _uniform,
                Sampler = Resolve<ISpriteSamplerSource>().GetSampler(SpriteSampler.TriLinear)
            };
            AttachChild(ResourceSet);
        }

        protected override void Unsubscribed()
        {
            CleanupSet();
            base.Unsubscribed();
        }

        void CleanupSet()
        {
            if (ResourceSet == null) return;
            ResourceSet.Dispose();
            RemoveChild(ResourceSet);
            ResourceSet = null;
        }

        public void Dispose()
        {
            CleanupSet();
            _uniform?.Dispose();
        }
    }

#pragma warning disable 649
    [VertexShader(typeof(FullscreenQuadVertexShader))]
    [FragmentShader(typeof(FullscreenQuadFragmentShader))]
    partial class FullscreenQuadPipeline : PipelineHolder { }

    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    partial struct FullscreenQuadIntermediate : IVertexFormat
    {
        [Vertex("NormCoords")] public Vector2 NormalisedTextureCoordinates;
    }

    [Name("FullscreenQuadSV.vert")]
    [Input(0, typeof(Vertex2DTextured))]
    [ResourceSet(0, typeof(FullscreenQuadResourceSet))]
    [Output(0, typeof(FullscreenQuadIntermediate))]
    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    internal partial class FullscreenQuadVertexShader : IVertexShader { }

    [Name("FullscreenQuadSF.frag")]
    [Input(0, typeof(FullscreenQuadIntermediate))]
    [ResourceSet(0, typeof(FullscreenQuadResourceSet))]
    [Output(0, typeof(SimpleFramebuffer))]
    [SuppressMessage("Microsoft.Naming", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used for code generation")]
    internal partial class FullscreenQuadFragmentShader : IFragmentShader { }

    partial class FullscreenQuadResourceSet : ResourceSetHolder
    {
        [Resource("uSampler", ShaderStages.Fragment)] ISamplerHolder _sampler;
        [Resource("uTexture", ShaderStages.Fragment)] ITextureHolder _texture;
        [Resource("_Uniform", ShaderStages.Vertex)] IBufferHolder<FullscreenQuadUniformInfo> _uniform;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FullscreenQuadUniformInfo // Length must be multiple of 16
    {
        [Uniform("uRect")] public Vector4 uRect;
    }
#pragma warning restore 649
    public sealed class FullscreenQuadRenderer : Component, IRenderer, IDisposable
    {
        static readonly ushort[] Indices = { 0, 1, 2, 2, 1, 3 };
        static readonly Vertex2DTextured[] Vertices =
        {
            new (-1.0f, -1.0f, 0.0f, 0.0f), new (1.0f, -1.0f, 1.0f, 0.0f),
            new (-1.0f, 1.0f, 0.0f, 1.0f), new (1.0f, 1.0f, 1.0f, 1.0f),
        };

        readonly Dictionary<OutputDescription, FullscreenQuadPipeline> _pipelines = new();

        readonly MultiBuffer<Vertex2DTextured> _vertexBuffer;
        readonly MultiBuffer<ushort> _indexBuffer;

        static FullscreenQuadPipeline BuildPipeline(OutputDescription outputDescription) 
            => new()
            {
                Name = "P_FullscreenQuad",
                AlphaBlend = BlendStateDescription.SingleDisabled,
                CullMode = FaceCullMode.None,
                DepthStencilMode = DepthStencilStateDescription.Disabled,
                FillMode = PolygonFillMode.Solid,
                OutputDescription = outputDescription,
                Topology = PrimitiveTopology.TriangleList,
                UseDepthTest = false,
                UseScissorTest = false,
                Winding = FrontFace.Clockwise,
            };

        public FullscreenQuadRenderer()
        {
            _vertexBuffer = new MultiBuffer<Vertex2DTextured>(Vertices, BufferUsage.VertexBuffer, "QuadVertexBuffer");
            _indexBuffer = new MultiBuffer<ushort>(Indices, BufferUsage.IndexBuffer, "QuadIndexBuffer");
            AttachChild(_vertexBuffer);
            AttachChild(_indexBuffer);
        }

        public void Render(IRenderable renderable, CommonSet commonSet, IFramebufferHolder framebuffer, CommandList cl, GraphicsDevice device)
        {
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            if (commonSet == null) throw new ArgumentNullException(nameof(commonSet));
            if (framebuffer == null) throw new ArgumentNullException(nameof(framebuffer));
            if (renderable is not FullscreenQuad FullscreenQuad)
                throw new ArgumentException($"{GetType().Name} was passed renderable of unexpected type {renderable?.GetType().Name ?? "null"}", nameof(renderable));

            if (framebuffer.OutputDescription == null)
                return;

            if (!_pipelines.TryGetValue(framebuffer.OutputDescription.Value, out var pipeline))
            {
                pipeline = BuildPipeline(framebuffer.OutputDescription.Value);
                AttachChild(pipeline);
                _pipelines[framebuffer.OutputDescription.Value] = pipeline;
            }

            if (pipeline.Pipeline == null)
                return; // Can't render it this frame as it needs to be initialised in the pre-draw steps

            cl.PushDebugGroup(FullscreenQuad.Name);

            cl.SetPipeline(pipeline.Pipeline);
            cl.SetGraphicsResourceSet(0, FullscreenQuad.ResourceSet.ResourceSet);
            cl.SetVertexBuffer(0, _vertexBuffer.DeviceBuffer);
            cl.SetIndexBuffer(_indexBuffer.DeviceBuffer, IndexFormat.UInt16);
            cl.SetFramebuffer(framebuffer.Framebuffer);

            cl.DrawIndexed((uint)Indices.Length);
            cl.PopDebugGroup();
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            foreach (var pipeline in _pipelines.Values)
                pipeline.Dispose();

            _pipelines.Clear();
            RemoveAllChildren();
        }
    }
}
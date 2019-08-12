using System;
using System.Collections.Generic;
using UAlbion.Core.Events;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace UAlbion.Core.Objects
{
    class FullScreenQuad : Component, IRenderer, IRenderable
    {
        static readonly IList<Handler> Handlers = new Handler[] { new Handler<FullScreenQuad, RenderEvent>((x, e) => e.Add(x)) };
        static readonly ushort[] QuadIndices = { 0, 1, 2, 0, 2, 3 };
        public RenderPasses RenderPasses => RenderPasses.SwapchainOutput;
        public int RenderOrder => int.MaxValue;
        public Type Renderer => typeof(FullScreenQuad);

        DisposeCollector _disposeCollector;
        Pipeline _pipeline;
        DeviceBuffer _ib;
        DeviceBuffer _vb;

        const string VertexShader = @"
#version 450
layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 TexCoords;

layout(location = 0) out vec2 fsin_0;

void main()
{
    fsin_0 = TexCoords;
    gl_Position = vec4(Position.x, Position.y, 0, 1);
}";

        const string FragmentShader = @"
#version 450

layout(set = 0, binding = 0) uniform texture2D SourceTexture;
layout(set = 0, binding = 1) uniform sampler SourceSampler;

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 0) out vec4 OutputColor;

void main()
{
    vec4 color = texture(sampler2D(SourceTexture, SourceSampler), fsin_TexCoords);
    OutputColor = color;
}";

        public FullScreenQuad() : base(Handlers) { }

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            var factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
            _disposeCollector = factory.DisposeCollector;

            var layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                ResourceLayoutH.Texture("vdspv_0_0"),
                ResourceLayoutH.Sampler("SourceSampler")));

            var shaderSet = new ShaderSetDescription(new[] { Vertex2DTextured.VertexLayout },
                factory.CreateFromSpirv(ShaderHelper.Vertex(VertexShader), ShaderHelper.Fragment(FragmentShader)));

            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, 
                true, false);

            var pd = new GraphicsPipelineDescription(
                new BlendStateDescription(RgbaFloat.Black, BlendAttachmentDescription.OverrideBlend),
                DepthStencilStateDescription.Disabled,
                rasterizerState,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { Vertex2DTextured.VertexLayout }, shaderSet.Shaders, ShaderHelper.GetSpecializations(gd)),
                new[] { layout },
                gd.SwapchainFramebuffer.OutputDescription);
            _pipeline = factory.CreateGraphicsPipeline(ref pd);
            _pipeline.Name = "P_FullScreenQuad";

            float[] verts = Util.GetFullScreenQuadVerts(gd);
            _vb = factory.CreateBuffer(new BufferDescription(verts.SizeInBytes() * sizeof(float), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, verts);

            _ib = factory.CreateBuffer(new BufferDescription(QuadIndices.SizeInBytes(), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, QuadIndices);
        }

        public IEnumerable<IRenderable> UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc, IEnumerable<IRenderable> renderables) => renderables;

        public void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass, IRenderable r)
        {
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, sc.DuplicatorTargetSet0);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.DrawIndexed(6, 1, 0, 0, 0);
        }

        public void DestroyDeviceObjects() { _disposeCollector.DisposeAll(); }
        public void Dispose() { DestroyDeviceObjects(); }
    }
}

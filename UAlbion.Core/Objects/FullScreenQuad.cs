using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.Utilities;

namespace UAlbion.Core.Objects
{
    static class ShaderH
    {
        public static ShaderDescription Vertex(string shader) => new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(shader), "main");
        public static ShaderDescription Fragment(string shader) => new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(shader), "main");
    }

    internal class FullScreenQuad : Renderable
    {
        static readonly ushort[] QuadIndices = { 0, 1, 2, 0, 2, 3 };

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

            layout(constant_id = 103) const bool OutputFormatSrgb = true;

            void main()
            {
                vec4 color = texture(sampler2D(SourceTexture, SourceSampler), fsin_TexCoords);
                OutputColor = color;
            }";

        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            var factory = new DisposeCollectorResourceFactory(gd.ResourceFactory);
            _disposeCollector = factory.DisposeCollector;

            var layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                ResourceLayoutH.Texture("SourceTexture"),
                ResourceLayoutH.Sampler("SourceSampler")));

            var shaderSet = new ShaderSetDescription(new[] { Vertex2DTextured.VertexLayout },
                factory.CreateFromSpirv(ShaderH.Vertex(VertexShader), ShaderH.Fragment(FragmentShader)));

            var pd = new GraphicsPipelineDescription(
                new BlendStateDescription(RgbaFloat.Black, BlendAttachmentDescription.OverrideBlend),
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(FaceCullMode.Back, PolygonFillMode.Solid, FrontFace.Clockwise, true, false),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(new[] { Vertex2DTextured.VertexLayout }, shaderSet.Shaders, ShaderHelper.GetSpecializations(gd)),
                new[] { layout },
                gd.SwapchainFramebuffer.OutputDescription);
            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            float[] verts = Util.GetFullScreenQuadVerts(gd);
            _vb = factory.CreateBuffer(new BufferDescription(verts.SizeInBytes() * sizeof(float), BufferUsage.VertexBuffer));
            cl.UpdateBuffer(_vb, 0, verts);

            _ib = factory.CreateBuffer(new BufferDescription(QuadIndices.SizeInBytes(), BufferUsage.IndexBuffer));
            cl.UpdateBuffer(_ib, 0, QuadIndices);
        }

        public override void DestroyDeviceObjects()
        {
            _disposeCollector.DisposeAll();
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition) { return new RenderOrderKey(); }

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, sc.DuplicatorTargetSet0);
            cl.SetVertexBuffer(0, _vb);
            cl.SetIndexBuffer(_ib, IndexFormat.UInt16);
            cl.DrawIndexed(6, 1, 0, 0, 0);
        }

        public override RenderPasses RenderPasses => RenderPasses.SwapchainOutput;
        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc) { }
    }
}

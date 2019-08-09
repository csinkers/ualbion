using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace UAlbion.Core
{
    public class SceneContext
    {
        public DeviceBuffer IdentityMatrixBuffer { get; private set; }
        public DeviceBuffer ProjectionMatrixBuffer { get; private set; }
        public DeviceBuffer ViewMatrixBuffer { get; private set; }
        public DeviceBuffer DepthLimitsBuffer { get; internal set; }
        public DeviceBuffer CameraInfoBuffer { get; private set; }


        // MainSceneView and Duplicator resource sets both use this.
        public ResourceLayout TextureSamplerResourceLayout { get; private set; }

        public Texture PaletteTexture { get; internal set; }
        public TextureView PaletteView { get; internal set; }
        public Texture MainSceneColorTexture { get; private set; }
        public Texture MainSceneDepthTexture { get; private set; }
        public Framebuffer MainSceneFramebuffer { get; private set; }
        public Texture MainSceneResolvedColorTexture { get; private set; }
        public TextureView MainSceneResolvedColorView { get; private set; }
        public ResourceSet MainSceneViewResourceSet { get; private set; }

        public Texture DuplicatorTarget0 { get; private set; }
        public TextureView DuplicatorTargetView0 { get; private set; }
        public ResourceSet DuplicatorTargetSet0 { get; internal set; }
        public Framebuffer DuplicatorFramebuffer { get; private set; }

        public ICamera Camera { get; set; }
        public TextureSampleCount MainSceneSampleCount { get; internal set; }

        public virtual void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            ResourceFactory factory = gd.ResourceFactory;
            ProjectionMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            ViewMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            IdentityMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            cl.UpdateBuffer(IdentityMatrixBuffer, 0, Matrix4x4.Identity);

            IdentityMatrixBuffer.Name = "M_Id";
            ProjectionMatrixBuffer.Name = "M_Projection";
            ViewMatrixBuffer.Name = "M_View";

            DepthLimitsBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<DepthCascadeLimits>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            CameraInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            DepthLimitsBuffer.Name = "B_DepthLimits";
            CameraInfoBuffer.Name = "B_CameraInfo";

            if (Camera != null)
            {
                UpdateCameraBuffers(cl);
            }

            TextureSamplerResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            TextureSamplerResourceLayout.Name = "RL_TextureSampler";

            RecreateWindowSizedResources(gd, cl);
        }

        public virtual void DestroyDeviceObjects()
        {
            IdentityMatrixBuffer.Dispose();
            ProjectionMatrixBuffer.Dispose();
            ViewMatrixBuffer.Dispose();
            DepthLimitsBuffer.Dispose();
            CameraInfoBuffer.Dispose();
            MainSceneColorTexture.Dispose();
            MainSceneResolvedColorTexture.Dispose();
            MainSceneResolvedColorView.Dispose();
            MainSceneDepthTexture.Dispose();
            MainSceneFramebuffer.Dispose();
            MainSceneViewResourceSet.Dispose();
            DuplicatorTarget0.Dispose();
            DuplicatorTargetView0.Dispose();
            DuplicatorTargetSet0.Dispose();
            DuplicatorFramebuffer.Dispose();
            TextureSamplerResourceLayout.Dispose();
            PaletteView?.Dispose();
            PaletteTexture?.Dispose();
        }

        public void SetCurrentScene(Scene scene)
        {
            Camera = scene.Camera;
        }

        public void UpdateCameraBuffers(CommandList cl)
        {
            cl.UpdateBuffer(ProjectionMatrixBuffer, 0, Camera.ProjectionMatrix);
            cl.UpdateBuffer(ViewMatrixBuffer, 0, Camera.ViewMatrix);
            cl.UpdateBuffer(CameraInfoBuffer, 0, Camera.GetCameraInfo());
        }

        internal void RecreateWindowSizedResources(GraphicsDevice gd, CommandList cl)
        {
            MainSceneColorTexture?.Dispose();
            MainSceneDepthTexture?.Dispose();
            MainSceneResolvedColorTexture?.Dispose();
            MainSceneResolvedColorView?.Dispose();
            MainSceneViewResourceSet?.Dispose();
            MainSceneFramebuffer?.Dispose();
            DuplicatorTarget0?.Dispose();
            DuplicatorTargetView0?.Dispose();
            DuplicatorTargetSet0?.Dispose();
            DuplicatorFramebuffer?.Dispose();

            ResourceFactory factory = gd.ResourceFactory;

            gd.GetPixelFormatSupport(
                PixelFormat.R16_G16_B16_A16_Float,
                TextureType.Texture2D,
                TextureUsage.RenderTarget,
                out PixelFormatProperties properties);

            TextureSampleCount sampleCount = MainSceneSampleCount;
            while (!properties.IsSampleCountSupported(sampleCount))
            {
                sampleCount = sampleCount - 1;
            }

            TextureDescription mainColorDesc = TextureDescription.Texture2D(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                PixelFormat.R16_G16_B16_A16_Float,
                TextureUsage.RenderTarget | TextureUsage.Sampled,
                sampleCount);

            MainSceneColorTexture = factory.CreateTexture(ref mainColorDesc);
            MainSceneColorTexture.Name = "T_MainSceneColor";

            if (sampleCount != TextureSampleCount.Count1)
            {
                mainColorDesc.SampleCount = TextureSampleCount.Count1;
                MainSceneResolvedColorTexture = factory.CreateTexture(ref mainColorDesc);
                MainSceneResolvedColorTexture.Name = "T_MainSceneResolvedColor";
            }
            else
            {
                MainSceneResolvedColorTexture = MainSceneColorTexture;
            }

            MainSceneResolvedColorView = factory.CreateTextureView(MainSceneResolvedColorTexture);
            MainSceneDepthTexture = factory.CreateTexture(TextureDescription.Texture2D(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                PixelFormat.R32_Float,
                TextureUsage.DepthStencil,
                sampleCount));
            MainSceneFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(MainSceneDepthTexture, MainSceneColorTexture));
            MainSceneViewResourceSet = factory.CreateResourceSet(new ResourceSetDescription(TextureSamplerResourceLayout, MainSceneResolvedColorView, gd.PointSampler));

            MainSceneResolvedColorView.Name = "TV_MainSceneResolvedColor";
            MainSceneDepthTexture.Name = "T_MainSceneDepth";
            MainSceneFramebuffer.Name = "FB_MainFrame";
            MainSceneViewResourceSet.Name = "RS_MainFrame";

            TextureDescription colorTargetDesc = TextureDescription.Texture2D(
                gd.SwapchainFramebuffer.Width,
                gd.SwapchainFramebuffer.Height,
                1,
                1,
                PixelFormat.R16_G16_B16_A16_Float,
                TextureUsage.RenderTarget | TextureUsage.Sampled);
            DuplicatorTarget0 = factory.CreateTexture(ref colorTargetDesc);
            DuplicatorTargetView0 = factory.CreateTextureView(DuplicatorTarget0);
            DuplicatorTargetSet0 = factory.CreateResourceSet(new ResourceSetDescription(TextureSamplerResourceLayout, DuplicatorTargetView0, gd.PointSampler));

            DuplicatorTarget0.Name = "T_DuplicatorTarget";
            DuplicatorTargetView0.Name = "TV_DuplicatorTarget";
            DuplicatorTargetSet0.Name = "RS_DuplicatorTarget";

            FramebufferDescription fbDesc = new FramebufferDescription(null, DuplicatorTarget0);
            DuplicatorFramebuffer = factory.CreateFramebuffer(ref fbDesc);
            DuplicatorFramebuffer.Name = "FB_Duplicator";
        }
   }
}

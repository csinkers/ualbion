using System;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Veldrid
{
    class WindowSizedSceneContext : IDisposable
    {
        readonly DisposeCollector _disposer = new DisposeCollector();

        // MainSceneView and Duplicator resource sets both use this.
        public ResourceLayout TextureSamplerResourceLayout { get; }
        public Texture MainSceneColorTexture { get; }
        public Texture MainSceneDepthTexture { get; }
        public Framebuffer MainSceneFramebuffer { get; }
        public Texture MainSceneResolvedColorTexture { get; }
        public TextureView MainSceneResolvedColorView { get; }
        public ResourceSet MainSceneViewResourceSet { get; }

        public Texture DuplicatorTarget0 { get; }
        public TextureView DuplicatorTargetView0 { get; }
        public ResourceSet DuplicatorTargetSet0 { get; }
        public Framebuffer DuplicatorFramebuffer { get; }

        public WindowSizedSceneContext(GraphicsDevice gd, TextureSampleCount sampleCount)
        {
            _disposer.DisposeAll();
            ResourceFactory factory = new DisposingResourceFactoryFacade(gd.ResourceFactory, _disposer);

            TextureSamplerResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "SourceTexture",
                    ResourceKind.TextureReadOnly,
                    ShaderStages.Fragment),

                new ResourceLayoutElementDescription(
                    "SourceSampler",
                    ResourceKind.Sampler,
                    ShaderStages.Fragment)));

            TextureSamplerResourceLayout.Name = "RL_TextureSampler";

            gd.GetPixelFormatSupport(
                PixelFormat.R16_G16_B16_A16_Float,
                TextureType.Texture2D,
                TextureUsage.RenderTarget,
                out PixelFormatProperties properties);

            while (!properties.IsSampleCountSupported(sampleCount))
            {
                sampleCount--;
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
            MainSceneViewResourceSet = factory.CreateResourceSet(
                new ResourceSetDescription(
                    TextureSamplerResourceLayout,
                    MainSceneResolvedColorView, gd.PointSampler));

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

        public void Dispose()
        {
            _disposer.DisposeAll();
        }
    }
}

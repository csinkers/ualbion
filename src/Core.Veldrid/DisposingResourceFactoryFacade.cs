using System;
using Veldrid;
using Veldrid.Utilities;

namespace UAlbion.Core.Veldrid
{
    class DisposingResourceFactoryFacade : ResourceFactory
    {
        readonly ResourceFactory _factory;
        readonly DisposeCollector _collector;

        public DisposingResourceFactoryFacade(ResourceFactory factory, DisposeCollector collector) : base(factory.Features)
        {
            _factory = factory;
            _collector = collector ?? throw new ArgumentNullException(nameof(collector));
        }

        T Track<T>(T disposable) where T : IDisposable
        {
            _collector.Add(disposable);
            return disposable;
        }

        public override GraphicsBackend BackendType => _factory.BackendType;

        protected override Pipeline CreateGraphicsPipelineCore(ref GraphicsPipelineDescription description)
            => Track(_factory.CreateGraphicsPipeline(description));

        public override Pipeline CreateComputePipeline(ref ComputePipelineDescription description)
            => Track(_factory.CreateComputePipeline(description));

        public override Framebuffer CreateFramebuffer(ref FramebufferDescription description)
            => Track(_factory.CreateFramebuffer(description));

        public override CommandList CreateCommandList(ref CommandListDescription description)
            => Track(_factory.CreateCommandList(description));

        public override ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
            => Track(_factory.CreateResourceLayout(description));

        public override ResourceSet CreateResourceSet(ref ResourceSetDescription description)
            => Track(_factory.CreateResourceSet(description));

        public override Fence CreateFence(bool signaled)
            => Track(_factory.CreateFence(signaled));

        public override Swapchain CreateSwapchain(ref SwapchainDescription description)
            => Track(_factory.CreateSwapchain(description));

        protected override Texture CreateTextureCore(ulong nativeTexture, ref TextureDescription description)
            => Track(_factory.CreateTexture(nativeTexture, description));

        protected override Texture CreateTextureCore(ref TextureDescription description)
            => Track(_factory.CreateTexture(description));

        protected override TextureView CreateTextureViewCore(ref TextureViewDescription description)
            => Track(_factory.CreateTextureView(description));

        protected override DeviceBuffer CreateBufferCore(ref BufferDescription description)
            => Track(_factory.CreateBuffer(description));

        protected override Sampler CreateSamplerCore(ref SamplerDescription description)
            => Track(_factory.CreateSampler(description));

        protected override Shader CreateShaderCore(ref ShaderDescription description)
            => Track(_factory.CreateShader(description));
    }
}

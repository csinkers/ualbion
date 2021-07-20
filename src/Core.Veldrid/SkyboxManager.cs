using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public sealed class SkyboxManager : Component, ISkyboxManager, IDisposable
    {
        readonly SamplerHolder _skyboxSampler;

        public SkyboxManager()
        {
            _skyboxSampler = new SamplerHolder
            {
                Name = "SkyboxSampler",
                AddressModeU = SamplerAddressMode.Wrap,
                AddressModeV = SamplerAddressMode.Clamp,
                AddressModeW = SamplerAddressMode.Clamp,
                Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
            };
            AttachChild(_skyboxSampler);
        }

        public void Collect(List<IRenderable> renderables)
        {
            if (renderables == null) throw new ArgumentNullException(nameof(renderables));
            foreach (var child in Children)
                if (child is Skybox skybox)
                    renderables.Add(skybox);
        }

        public Skybox CreateSkybox(ITexture texture)
        {
            var ts = Resolve<ITextureSource>();
            var textureHolder = ts.GetSimpleTexture(texture);
            return new Skybox(textureHolder, _skyboxSampler, this);
        }

        internal void DisposeSkybox(Skybox skybox)
        {
            if (skybox == null) throw new ArgumentNullException(nameof(skybox));
            RemoveChild(skybox);
        }

        public void Dispose()
        {
            foreach (var child in Children)
                if (child is Skybox skybox)
                    skybox.Dispose();

            _skyboxSampler?.Dispose();
        }
    }
}
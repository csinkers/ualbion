using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Skybox;

public sealed class SkyboxManager : ServiceComponent<ISkyboxManager>, ISkyboxManager, IDisposable
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
            // Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
            Filter = SamplerFilter.MinPoint_MagPoint_MipPoint,
        };
        AttachChild(_skyboxSampler);
    }

    public void Collect(List<IRenderable> renderables)
    {
        if (renderables == null) throw new ArgumentNullException(nameof(renderables));
        foreach (var child in Children)
            if (child is SkyboxRenderable skybox)
                renderables.Add(skybox);
    }

    public SkyboxRenderable CreateSkybox(ITexture texture, ICamera camera)
    {
        var ts = Resolve<ITextureSource>();
        var textureHolder = ts.GetSimpleTexture(texture);
        var renderable = new SkyboxRenderable(textureHolder, _skyboxSampler, this, camera);
        AttachChild(renderable);
        return renderable;
    }

    internal void DisposeSkybox(SkyboxRenderable skybox)
    {
        if (skybox == null) throw new ArgumentNullException(nameof(skybox));
        RemoveChild(skybox);
    }

    public void Dispose()
    {
        foreach (var child in Children.ToList())
            if (child is SkyboxRenderable skybox)
                skybox.Dispose();

        _skyboxSampler?.Dispose();
    }
}
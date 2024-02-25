using System;
using UAlbion.Api.Eventing;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Skybox;

public sealed class SkyboxRenderable : Component, ISkybox
{
    readonly SkyboxManager _manager;
    readonly SingleBuffer<SkyboxUniformInfo> _uniformBuffer;

    internal SkyboxRenderable(ITextureHolder texture, ISamplerHolder sampler, SkyboxManager manager, ICamera camera)
    {
        if (texture == null) throw new ArgumentNullException(nameof(texture));
        if (sampler == null) throw new ArgumentNullException(nameof(sampler));
        _manager = manager;

        _uniformBuffer = new SingleBuffer<SkyboxUniformInfo>(new SkyboxUniformInfo(), BufferUsage.UniformBuffer, "SpriteUniformBuffer");
        ResourceSet = new SkyboxResourceSet
        {
            Name = $"RS_Sky:{texture.Name}",
            Texture = texture,
            Sampler = sampler,
            Uniform = _uniformBuffer
        };

        AttachChild(_uniformBuffer);
        AttachChild(ResourceSet);

        On<EngineUpdateEvent>(_ =>
        {
            _uniformBuffer.Data = new SkyboxUniformInfo
            {
                uYaw = camera.Yaw,
                uPitch = camera.Pitch,
                uVisibleProportion = ReadVar(V.Core.Gfx.Skybox.VisibleProportion)
            };
        });

    }

    public string Name => ResourceSet.Texture.Name;
    public DrawLayer RenderOrder => DrawLayer.Background;
    internal SkyboxResourceSet ResourceSet { get; }

    public void Dispose()
    {
        _uniformBuffer?.Dispose();
        ResourceSet?.Dispose();
        _manager.DisposeSkybox(this);
    }
}
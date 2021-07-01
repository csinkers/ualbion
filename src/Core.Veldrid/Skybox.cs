using System;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    internal class Skybox : ServiceComponent<ISkybox>, ISkybox, IRenderable
    {
        readonly SingleBuffer<SkyboxUniformInfo> _uniformBuffer;

        public Skybox(ITextureHolder texture, ISamplerHolder sampler)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            if (sampler == null) throw new ArgumentNullException(nameof(sampler));

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
                var config = Resolve<CoreConfig>().Visual.Skybox;
                if (Resolve<ICamera>() is not PerspectiveCamera camera)
                    return;

                _uniformBuffer.Data = new SkyboxUniformInfo
                {
                    uYaw = camera.Yaw,
                    uPitch = camera.Pitch,
                    uVisibleProportion = config.VisibleProportion
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
        }
    }
}
using System;
using UAlbion.Api.Visual;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public class Skybox : ServiceComponent<ISkybox>, ISkybox, IRenderable
    {
        readonly SingleBuffer<SkyboxUniformInfo> _uniformBuffer;

        public Skybox(Texture2DHolder texture, SamplerHolder sampler)
        {
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            if (sampler == null) throw new ArgumentNullException(nameof(sampler));

            _uniformBuffer = AttachChild(new SingleBuffer<SkyboxUniformInfo>(new SkyboxUniformInfo(), BufferUsage.UniformBuffer, "SpriteUniformBuffer"));
            ResourceSet = AttachChild(new SkyboxResourceSet
            {
                Name = $"RS_Sky:{texture.Texture.Name}",
                Texture = texture,
                Sampler = sampler,
                Uniform = _uniformBuffer
            });

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

        public string Name => ResourceSet.Texture.Texture.Name;
        public DrawLayer RenderOrder => DrawLayer.Background;
        internal SkyboxResourceSet ResourceSet { get; }

        public void Dispose()
        {
            _uniformBuffer?.Dispose();
            ResourceSet?.Dispose();
        }
    }
}
using UAlbion.Api.Visual;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public class VeldridCoreFactory : ServiceComponent<ICoreFactory>, ICoreFactory
    {
        readonly SamplerHolder _skyboxSampler;

        public VeldridCoreFactory()
        {
            _skyboxSampler = AttachChild(new SamplerHolder
            {
                Name = "SkyboxSampler",
                AddressModeU = SamplerAddressMode.Wrap,
                AddressModeV = SamplerAddressMode.Clamp,
                AddressModeW = SamplerAddressMode.Clamp,
                Filter = SamplerFilter.MinLinear_MagLinear_MipLinear,
            });
        }

        public ISkybox CreateSkybox(ITexture texture)
        {
            var ts = Resolve<ITextureSource>();
            var textureHolder = ts.GetSimpleTexture(texture);
            return new Skybox(textureHolder, _skyboxSampler);
        }

        public SpriteBatch CreateSpriteBatch(SpriteKey key) 
            => new VeldridSpriteBatch(key);
    }
}
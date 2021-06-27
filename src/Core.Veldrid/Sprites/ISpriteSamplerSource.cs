using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid.Sprites
{
    public interface ISpriteSamplerSource
    {
        SamplerHolder Get(SpriteSampler sampler);
    }
}
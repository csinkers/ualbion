using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Sprites;

public interface ISpriteSamplerSource
{
    ISamplerHolder GetSampler(SpriteSampler sampler);
}
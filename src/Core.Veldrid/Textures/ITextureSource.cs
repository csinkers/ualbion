using UAlbion.Api.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public interface ITextureSource
    {
        ITextureHolder GetSimpleTexture(ITexture texture, int version = 0);
        ITextureArrayHolder GetArrayTexture(ITexture texture, int version = 0);
    }
}
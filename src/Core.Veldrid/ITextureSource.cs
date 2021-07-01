using UAlbion.Api.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public interface ITextureSource
    {
        ITextureHolder GetSimpleTexture(ITexture texture);
        ITextureArrayHolder GetArrayTexture(ITexture texture);
    }
}
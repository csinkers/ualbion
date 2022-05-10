using UAlbion.Api.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid.Textures;

public interface ITextureSource
{
    ITextureHolder GetSimpleTexture(ITexture texture);
    ITextureArrayHolder GetArrayTexture(ITexture texture);
    ITextureHolder GetDummySimpleTexture();
    ITextureArrayHolder GetDummyArrayTexture();
}
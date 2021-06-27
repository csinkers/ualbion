using UAlbion.Api.Visual;

namespace UAlbion.Core.Veldrid
{
    public interface ITextureSource
    {
        Texture2DHolder GetSimpleTexture(ITexture texture);
        Texture2DArrayHolder GetArrayTexture(ITexture texture);
    }
}
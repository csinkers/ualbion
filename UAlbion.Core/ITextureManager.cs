using UAlbion.Core.Objects;
using Veldrid;

namespace UAlbion.Core
{
    public interface ITextureManager
    {
        TextureView GetTexture(ITextureId id, GraphicsDevice gd);
    }
}
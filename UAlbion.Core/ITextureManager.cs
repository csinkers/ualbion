using System.Collections.Generic;
using Veldrid;

namespace UAlbion.Core
{
    public interface ITextureManager
    {
        TextureView GetTexture(ITexture texture);
        void PrepareTexture(ITexture texture, GraphicsDevice gd);
    }

    public class TextureManager : ITextureManager
    {
        readonly IDictionary<ITexture, Texture> _textures = new Dictionary<ITexture, Texture>();
        readonly IDictionary<ITexture, TextureView> _textureViews = new Dictionary<ITexture, TextureView>();

        public TextureView GetTexture(ITexture texture)
        {
            // TODO: Return test texture when not found
            return _textureViews[texture];
        }

        public void PrepareTexture(ITexture texture, GraphicsDevice gd)
        {
            if (_textures.ContainsKey(texture))
                return;

            var deviceTexture = texture.CreateDeviceTexture(gd, gd.ResourceFactory, TextureUsage.Sampled);
            _textures[texture] = deviceTexture;
            _textureViews[texture] = gd.ResourceFactory.CreateTextureView(deviceTexture);
        }

        // TODO: Handle flushing when backend changes
    }
}
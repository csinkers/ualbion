using System.Collections.Generic;
using Veldrid;

namespace UAlbion.Core.Textures
{
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
            var textureView = gd.ResourceFactory.CreateTextureView(deviceTexture);
            textureView.Name = "TV_" + texture.Name;
            CoreTrace.Log.CreatedDeviceTexture(textureView.Name, texture.Width, texture.Height, texture.ArrayLayers);

            _textures[texture] = deviceTexture;
            _textureViews[texture] = textureView;
        }

        public void DestroyDeviceObjects()
        {
            foreach(var view in _textureViews.Values) view.Dispose();
            foreach(var texture in _textures.Values) texture.Dispose();
            _textureViews.Clear();
            _textures.Clear();
        }
    }
}
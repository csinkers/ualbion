using System;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    class TextureCacheEntry : IDisposable
    {
        public TextureCacheEntry(Texture texture, TextureView textureView)
        {
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            TextureView = textureView ?? throw new ArgumentNullException(nameof(textureView));
            LastAccessDateTime = DateTime.Now;
        }

        public Texture Texture { get; }
        public TextureView TextureView { get; }
        public DateTime LastAccessDateTime { get; set; }

        public void Dispose()
        {
            Texture?.Dispose();
            TextureView?.Dispose();
        }
    }
}
using System;
using Veldrid;

namespace UAlbion.Game
{
    public class ManagedTexture : IDisposable
    {
        public ManagedTexture(TextureId id, Texture texture, TextureView textureView)
        {
            Id = id;
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            TextureView = textureView ?? throw new ArgumentNullException(nameof(textureView));
            LastAccess = DateTime.Now;
        }

        public TextureId Id { get; }
        public DateTime LastAccess { get; set; }
        public Texture Texture { get; }
        public TextureView TextureView { get; }

        public void Dispose()
        {
            TextureView?.Dispose();
            Texture?.Dispose();
        }
    }
}
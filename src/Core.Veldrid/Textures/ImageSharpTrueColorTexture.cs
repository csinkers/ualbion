using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;
using Veldrid;
using Veldrid.ImageSharp;
using PixelFormat = UAlbion.Core.Textures.PixelFormat;

namespace UAlbion.Core.Veldrid.Textures
{
    public class ImageSharpTrueColorTexture : IVeldridTexture
    {
        public IAssetId Id { get; }
        public string Name { get; }
        public PixelFormat Format => PixelFormat.Rgba32;
        public TextureType Type => TextureType.Texture2D;
        public int Width => (int)_texture.Width;
        public int Height => (int)_texture.Height;
        public int Depth => 1;
        public int MipLevels => 1;
        public int ArrayLayers => 1;
        public int SubImageCount => 1;
        public bool IsDirty { get; private set; }
        public int SizeInBytes => (int)(_texture.Width * _texture.Height * _texture.PixelSizeInBytes);
        public int FormatSize => (int)_texture.PixelSizeInBytes;
        readonly ImageSharpTexture _texture;
        readonly SubImage _subImage;

        public ImageSharpTrueColorTexture(IAssetId id, string name, Image<Rgba32> image)
        {
            Id = id;
            Name = name;
            ImageSharpTexture imageSharpTexture = new ImageSharpTexture(image, false);
            _texture = imageSharpTexture;
            IsDirty = true;

            _subImage = new SubImage(
                Vector2.Zero,
                new Vector2(_texture.Width, _texture.Height),
                new Vector2(_texture.Width, _texture.Height),
                0);
        }

        public ISubImage GetSubImage(int subImageId) => _subImage;
        public void Invalidate() => IsDirty = true;

        public Texture CreateDeviceTexture(GraphicsDevice gd, TextureUsage usage)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            IsDirty = false;
            var texture = _texture.CreateDeviceTexture(gd, gd.ResourceFactory);
            texture.Name = "T_" + Name;
            return texture;
        }
    }
}
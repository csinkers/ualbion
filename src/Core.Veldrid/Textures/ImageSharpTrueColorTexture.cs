using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Core.Textures;
using Veldrid;
using Veldrid.ImageSharp;
using PixelFormat = UAlbion.Core.Textures.PixelFormat;

namespace UAlbion.Core.Veldrid.Textures
{
    public class ImageSharpTrueColorTexture : IVeldridTexture
    {
        public string Name { get; }
        public PixelFormat Format => PixelFormat.Rgba32;
        public TextureType Type => TextureType.Texture2D;
        public uint Width => _texture.Width;
        public uint Height => _texture.Height;
        public uint Depth => 1;
        public uint MipLevels => 1;
        public uint ArrayLayers => 1;
        public int SubImageCount => 1;
        public bool IsDirty { get; private set; }
        public int SizeInBytes => (int)(_texture.Width * _texture.Height * _texture.PixelSizeInBytes);
        public uint FormatSize => _texture.PixelSizeInBytes;
        readonly ImageSharpTexture _texture;
        readonly SubImage _subImage;

        public ImageSharpTrueColorTexture(string name, Image<Rgba32> image)
        {
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

        public SubImage GetSubImageDetails(int subImageId) => _subImage;
        public void Invalidate() => IsDirty = true;

        public Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            IsDirty = false;
            var texture = _texture.CreateDeviceTexture(gd, rf);
            texture.Name = "T_" + Name;
            return texture;
        }
    }
}
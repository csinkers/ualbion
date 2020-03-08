using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;

namespace UAlbion.Core.Veldrid.Textures
{
    public class TrueColorTexture : IVeldridTexture
    {
        public string Name { get; }
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UNorm;
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

        public TrueColorTexture(string name, Image<Rgba32> image)
        {
            Name = name;
            ImageSharpTexture imageSharpTexture = new ImageSharpTexture(image, false);
            _texture = imageSharpTexture;
            IsDirty = true;
        }

        public void GetSubImageDetails(int subImage, out Vector2 size, out Vector2 texOffset, out Vector2 texSize, out uint layer)
        {
            size = new Vector2(Width, Height);
            texOffset = new Vector2(0,0);
            texSize = new Vector2(1.0f,1.0f);
            layer = 0;
        }

        public Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            IsDirty = false;
            var texture = _texture.CreateDeviceTexture(gd, rf);
            texture.Name = "T_" + Name;
            return texture;
        }
    }
}
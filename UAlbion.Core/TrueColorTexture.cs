using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;

namespace UAlbion.Core
{
    public class TrueColorTexture : ITexture
    {
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UNorm;
        public TextureType Type => TextureType.Texture2D;
        public uint Width => _texture.Width;
        public uint Height => _texture.Height;
        public uint Depth => 1;
        public uint MipLevels => 1;
        public uint ArrayLayers => 1;
        public string Name { get; }
        readonly ImageSharpTexture _texture;

        public TrueColorTexture(string name, Image<Rgba32> image)
        {
            Name = name;
            ImageSharpTexture imageSharpTexture = new ImageSharpTexture(image, false);
            _texture = imageSharpTexture;
        }

        public Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            var texture = _texture.CreateDeviceTexture(gd, rf);
            texture.Name = "T_" + Name;
            return texture;
        }
    }
}
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game
{
    public static class FontLoader
    {
        public static ITexture Load(MetaFontId id, ITexture regular, ITexture bold)
        {
            var texture = (EightBitTexture)((id.IsBold) ? bold : regular);
            var bytes = (byte[])texture.TextureData.Clone();
            for(int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                    continue;

                bytes[i] += (byte)id.Color;
            }

            return new EightBitTexture(
                $"Font{id.Color}{(id.IsBold ? "Bold" : "")}",
                texture.Width, texture.Height,
                texture.MipLevels, texture.ArrayLayers, 
                bytes, texture.SubImages);
        }
    }
}
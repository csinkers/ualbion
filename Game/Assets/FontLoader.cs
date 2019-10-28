using System.Collections.Generic;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public static class FontLoader
    {
        static readonly IDictionary<FontColor, IList<byte>> Mappings = new Dictionary<FontColor, IList<byte>>
        {
            { FontColor.White, new byte[] { 0, 194, 194, 195, 196, 197 } },
        };

        public static ITexture Load(MetaFontId id, ITexture regular, ITexture bold)
        {
            var texture = (EightBitTexture)(id.IsBold ? bold : regular);
            var bytes = (byte[])texture.TextureData.Clone();
            var mapping = Mappings[id.Color];
            for(int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                    continue;

                bytes[i] = mapping[bytes[i]];
            }

            return new EightBitTexture(
                $"Font{id.Color}{(id.IsBold ? "Bold" : "")}",
                texture.Width, texture.Height,
                texture.MipLevels, texture.ArrayLayers, 
                bytes, texture.SubImages);
        }
    }
}
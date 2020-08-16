using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Assets
{
    public static class FontLoader
    {
        static readonly IDictionary<FontColor, IList<byte>> Mappings = new Dictionary<FontColor, IList<byte>>
        {
            { FontColor.White, new byte[] { 0, 194, 194, 195, 196, 197 } }, // DataLiteral
            { FontColor.Yellow, new byte[] { 0, 194, 219, 220, 221, 222 } }, // DataLiteral
            { FontColor.YellowOrange, new byte[] { 0, 194, 208, 209, 210, 211 } }, // DataLiteral
            { FontColor.Gray, new byte[] { 0, 196, 197, 198, 199, 200 } }, // DataLiteral
        };

        public static ITexture Load(ICoreFactory factory, MetaFontId id, ITexture regular, ITexture bold)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (regular == null) throw new ArgumentNullException(nameof(regular));
            if (bold == null) throw new ArgumentNullException(nameof(bold));
            var texture = (EightBitTexture)(id.IsBold ? bold : regular);
            var bytes = texture.TextureData.ToArray();
            if (!Mappings.TryGetValue(id.Color, out var mapping))
                mapping = Mappings[FontColor.White];

            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                    continue;

                bytes[i] = mapping[bytes[i]];
            }

            return factory.CreateEightBitTexture(
                $"Font{id.Color}{(id.IsBold ? "Bold" : "")}",
                texture.Width, texture.Height,
                texture.MipLevels, texture.ArrayLayers,
                bytes, texture.SubImages);
        }
    }
}

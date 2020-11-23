using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class MetafontBuilder : ServiceComponent<IMetafontBuilder>, IMetafontBuilder
    {
        // TODO: Move to config
        static readonly IDictionary<FontColor, IList<byte>> Mappings = new Dictionary<FontColor, IList<byte>>
        {
            { FontColor.White, new byte[] { 0, 194, 194, 195, 196, 197 } }, // DataLiteral
            { FontColor.Yellow, new byte[] { 0, 194, 219, 220, 221, 222 } }, // DataLiteral
            { FontColor.YellowOrange, new byte[] { 0, 194, 208, 209, 210, 211 } }, // DataLiteral
            { FontColor.Gray, new byte[] { 0, 196, 197, 198, 199, 200 } }, // DataLiteral
        };

        readonly ICoreFactory _factory;
        public MetafontBuilder(ICoreFactory factory) => _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        public ITexture Build(MetaFontId id)
        {
            var assets = Resolve<IAssetManager>();
            var texture = (EightBitTexture)(id.IsBold
                ? assets.LoadTexture(Base.Font.RegularFont) ?? throw new InvalidOperationException("MetafontBuilder: Could not load regular font.")
                : assets.LoadTexture(Base.Font.BoldFont) ?? throw new InvalidOperationException("MetafontBuilder: Could not load bold font."));

            var bytes = texture.TextureData.ToArray();
            if (!Mappings.TryGetValue(id.Color, out var mapping))
                mapping = Mappings[FontColor.White];

            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                    continue;

                bytes[i] = mapping[bytes[i]];
            }

            return _factory.CreateEightBitTexture(
                $"Font{id.Color}{(id.IsBold ? "Bold" : "")}",
                texture.Width, texture.Height,
                texture.MipLevels, texture.ArrayLayers,
                bytes, texture.SubImages);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
            var textureId = (SpriteId)(id.IsBold ? Base.Font.BoldFont : Base.Font.RegularFont);
            var texture = (EightBitTexture)assets.LoadTexture(textureId);
            if (texture == null)
                throw new InvalidOperationException($"MetafontBuilder: Could not load font {textureId}");

            var bytes = texture.PixelData.ToArray();
            if (!Mappings.TryGetValue(id.Color, out var mapping))
                mapping = Mappings[FontColor.White];

            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0)
                    continue;

                bytes[i] = mapping[bytes[i]];
            }

            return _factory.CreateEightBitTexture(
                textureId,
                $"Font{id.Color}{(id.IsBold ? "Bold" : "")}",
                texture.Width, texture.Height,
                texture.MipLevels, texture.ArrayLayers,
                bytes, texture.SubImages);
        }
    }
}

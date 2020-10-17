using System.Collections.Generic;
using System.IO;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Font)]
    public class FontSpriteLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, AssetMapping mapping, AssetId id, AssetInfo config)
        {
            var font = (AlbionSprite)new FixedSizeSpriteLoader().Load(br, streamLength, mapping, id, config);
            var frames = new List<AlbionSpriteFrame>();

            // Fix up sub-images for variable size
            foreach (var oldFrame in font.Frames)
            {
                int width = 0;
                for (int j = oldFrame.Y; j < oldFrame.Y + oldFrame.Height; j++)
                {
                    for (int i = oldFrame.X; i < oldFrame.X + oldFrame.Width; i++)
                    {
                        if (i - oldFrame.X > width && font.PixelData[j * font.Width + i] != 0)
                            width = i - oldFrame.X;
                    }
                }

                frames.Add(new AlbionSpriteFrame(oldFrame.X, oldFrame.Y, width + 2, oldFrame.Height));
            }

            return new AlbionSprite(font.Name, font.Width, font.Height, false, font.PixelData, frames);
        }
    }
}

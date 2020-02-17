using System.IO;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;

namespace UAlbion.Formats.Parsers
{
    [AssetLoader(FileFormat.Slab)]
    public class SlabLoader : IAssetLoader
    {
        public object Load(BinaryReader br, long streamLength, string name, AssetInfo config)
        {
            var sprite = (AlbionSprite)new FixedSizeSpriteLoader().Load(br, streamLength, name, config);
            sprite.Frames = new[]
            {
                sprite.Frames[0],
                new AlbionSprite.Frame(0, sprite.Height - 48, sprite.Width, 48)
            };

            return sprite;
        }
    }
}
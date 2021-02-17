using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class SlabLoader : IAssetLoader<AlbionSprite>
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSprite) existing, config, mapping, s);

        public AlbionSprite Serdes(AlbionSprite existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            var sprite = (AlbionSprite)new FixedSizeSpriteLoader().Serdes(existing, config, mapping, s);
            var frames = new[] // Frame 0 = entire slab, Frame 1 = status bar only.
            {
                sprite.Frames[0],
                new AlbionSpriteFrame(0, sprite.Height - 48, sprite.Width, 48)
            };

            return new AlbionSprite(sprite.Name, sprite.Width, sprite.Height, sprite.UniformFrames, sprite.PixelData, frames);
        }
    }
}

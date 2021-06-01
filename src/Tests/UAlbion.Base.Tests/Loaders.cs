using UAlbion.Formats.Parsers;
using UAlbion.Game.Assets;

namespace UAlbion.Base.Tests
{
    public static class Loaders
    {
        public static readonly AlbionStringTableLoader AlbionStringTableLoader = new AlbionStringTableLoader();
        public static readonly AmorphousSpriteLoader AmorphousSpriteLoader = new AmorphousSpriteLoader();
        public static readonly AutomapLoader AutomapLoader = new AutomapLoader();
        public static readonly BlockListLoader BlockListLoader = new BlockListLoader();
        public static readonly CharacterSheetLoader CharacterSheetLoader = new CharacterSheetLoader();
        public static readonly ChestLoader ChestLoader = new ChestLoader();
        public static readonly EventSetLoader EventSetLoader = new EventSetLoader();
        public static readonly FixedSizeSpriteLoader FixedSizeSpriteLoader = new FixedSizeSpriteLoader();
        public static readonly FontSpriteLoader<FixedSizeSpriteLoader> FontSpriteLoader = new FontSpriteLoader<FixedSizeSpriteLoader>();
        public static readonly SingleHeaderSpriteLoader SingleHeaderSpriteLoader = new SingleHeaderSpriteLoader();
        public static readonly MultiHeaderSpriteLoader MultiHeaderSpriteLoader = new MultiHeaderSpriteLoader();
        // public static readonly InterlacedBitmapLoader InterlacedBitmapLoader = new InterlacedBitmapLoader();
        public static readonly ItemDataLoader ItemDataLoader = new ItemDataLoader();
        public static readonly ItemNameLoader ItemNameLoader = new ItemNameLoader();
        public static readonly LabyrinthDataLoader LabyrinthDataLoader = new LabyrinthDataLoader();
        public static readonly MerchantLoader MerchantLoader = new MerchantLoader();
        public static readonly MonsterGroupLoader MonsterGroupLoader = new MonsterGroupLoader();
        public static readonly PaletteLoader PaletteLoader = new PaletteLoader();
        public static readonly SampleLoader SampleLoader = new SampleLoader();
        // public static readonly ScriptLoader ScriptLoader = new ScriptLoader();
        public static readonly SlabLoader SlabLoader = new SlabLoader();
        public static readonly SongLoader SongLoader = new SongLoader();
        public static readonly SpellLoader SpellLoader = new SpellLoader();
        public static readonly TilesetLoader TilesetLoader = new TilesetLoader();
        public static readonly Utf8Loader Utf8Loader = new Utf8Loader();
        public static readonly WaveLibLoader WaveLibLoader = new WaveLibLoader();
        public static readonly WordListLoader WordListLoader = new WordListLoader();
    }
}
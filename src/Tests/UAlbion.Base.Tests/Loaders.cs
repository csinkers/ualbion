using UAlbion.Formats.Parsers;

namespace UAlbion.Base.Tests;

public static class Loaders
{
    public static readonly AlbionStringTableLoader AlbionStringTableLoader = new();
    public static readonly AmorphousSpriteLoader AmorphousSpriteLoader = new();
    public static readonly AutomapLoader AutomapLoader = new();
    public static readonly BlockListLoader BlockListLoader = new();
    public static readonly ChestLoader ChestLoader = new();
    public static readonly EventSetLoader EventSetLoader = new();
    public static readonly FixedSizeSpriteLoader FixedSizeSpriteLoader = new();
    public static readonly FontSpriteLoader<FixedSizeSpriteLoader> FontSpriteLoader = new();
    public static readonly SingleHeaderSpriteLoader SingleHeaderSpriteLoader = new();
    public static readonly MultiHeaderSpriteLoader MultiHeaderSpriteLoader = new();
    // public static readonly InterlacedBitmapLoader InterlacedBitmapLoader = new InterlacedBitmapLoader();
    public static readonly ItemNameLoader ItemNameLoader = new();
    public static readonly LabyrinthDataLoader LabyrinthDataLoader = new();
    public static readonly MerchantLoader MerchantLoader = new();
    public static readonly MonsterGroupLoader MonsterGroupLoader = new();
    public static readonly PaletteLoader PaletteLoader = new();
    public static readonly SampleLoader SampleLoader = new();
    // public static readonly ScriptLoader ScriptLoader = new ScriptLoader();
    public static readonly SlabLoader SlabLoader = new();
    public static readonly SongLoader SongLoader = new();
    public static readonly SpellLoader SpellLoader = new();
    public static readonly TilesetLoader TilesetLoader = new();
    public static readonly TilesetGraphicsLoader TilesetGraphicsLoader = new();
    public static readonly Utf8Loader Utf8Loader = new();
    public static readonly WaveLibLoader WaveLibLoader = new();
    public static readonly WordListLoader WordListLoader = new();
}
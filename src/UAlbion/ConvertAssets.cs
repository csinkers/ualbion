using System.IO;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Game.Assets;

namespace UAlbion
{
    static class ConvertAssets
    {
        static (ModApplier, EventExchange) BuildModApplier(string baseDir, string mod)
        {
            var config = GeneralConfig.Load(Path.Combine(baseDir, "data", "config.json"), baseDir);
            config.SetPath("LANG", GameLanguage.English.ToString().ToUpperInvariant()); // TODO: Allow language selection
            var applier = new ModApplier();
            var exchange = new EventExchange();
            exchange
                .Register<IGeneralConfig>(config)
                .Attach(new AssetLoaderRegistry())
                .Attach(new ContainerRegistry())
                .Attach(new AssetLocator())
                .Attach(applier)
                ;
            applier.LoadMods(config, new[] { mod });
            return (applier, exchange);
        }

        public static void Convert(string baseDir, string fromMod, string toMod)
        {
            var (from, fromExchange) = BuildModApplier(baseDir, fromMod);
            var (to, toExchange) = BuildModApplier(baseDir, toMod);
            toExchange.Attach(new PaletteHackAssetManager(x => (AlbionPalette)from.LoadAssetCached(x)));

            using (fromExchange)
            using (toExchange)
            {
                var paletteHints = PaletteHints.Load(Path.Combine(baseDir, "mods", "Base", "palette_hints.json"));
                to.SaveAssets(x => (from.LoadAsset(x), from.GetAssetInfo(x)), paletteHints);
            }
        }
    }
}
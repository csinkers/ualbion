using System;
using System.IO;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Formats.Config;
using UAlbion.Game.Assets;
using UAlbion.Game.Settings;
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Debugging;

namespace UAlbion
{
    public static class AssetSystem
    {
        public static async Task<(EventExchange, IContainer)> SetupAsync(string baseDir)
        {
            var generalConfigTask = Task.Run(() => LoadGeneralConfig(baseDir));
            var settingsTask = Task.Run(() => LoadSettings(baseDir));
            var coreConfigTask = Task.Run(() => LoadCoreConfig(baseDir));
            var gameConfigTask = Task.Run(() => LoadGameConfig(baseDir));
            return await SetupCore(generalConfigTask, settingsTask, coreConfigTask, gameConfigTask).ConfigureAwait(false);
        }

        public static async Task<(EventExchange, IContainer)> SetupCore(
            Task<GeneralConfig> generalConfigTask,
            Task<GeneralSettings> settingsTask,
            Task<CoreConfig> coreConfigTask,
            Task<GameConfig> gameConfigTask)
        {
            if (generalConfigTask == null) throw new ArgumentNullException(nameof(generalConfigTask));
            if (settingsTask == null) throw new ArgumentNullException(nameof(settingsTask));
            if (coreConfigTask == null) throw new ArgumentNullException(nameof(coreConfigTask));
            if (gameConfigTask == null) throw new ArgumentNullException(nameof(gameConfigTask));

            var assets = new AssetManager();
            var factory = new VeldridCoreFactory();
            var loaderRegistry = new AssetLoaderRegistry();
            var containerRegistry = new ContainerRegistry();
            var postProcessorRegistry = new PostProcessorRegistry();
            var modApplier = new ModApplier();

            var settings = await settingsTask.ConfigureAwait(false);
            var settingsManager = new SettingsManager(settings);
            var services = new Container("Services", settingsManager, // Need to register settings first, as the AssetLocator relies on it.
                loaderRegistry, containerRegistry, postProcessorRegistry, new MetafontBuilder(factory), new StdioConsoleLogger(),
                // new ClipboardManager(),
                new ImGuiConsoleLogger(), new WordLookup(), new AssetLocator(), modApplier, assets);

            var generalConfig = await generalConfigTask.ConfigureAwait(false);

#pragma warning disable CA2000 // Dispose objects before losing scope
            var exchange = new EventExchange(new LogExchange())
                .Register<IGeneralConfig>(generalConfig)
                .Register<ICoreFactory>(factory)
                .Attach(services);
#pragma warning restore CA2000 // Dispose objects before losing scope

            PerfTracker.StartupEvent("Registered asset services");

            Engine.GlobalExchange = exchange;

            modApplier.LoadMods(generalConfig, settings.ActiveMods);
            AssetMapping.Global.ConsistencyCheck();
            PerfTracker.StartupEvent("Loaded mods");

            var coreConfig = await coreConfigTask.ConfigureAwait(false);
            var gameConfig = await gameConfigTask.ConfigureAwait(false);
            exchange // Need to load game config after mods so asset ids can be parsed.
                .Register(coreConfig)
                .Register(gameConfig);
            PerfTracker.StartupEvent("Loaded core and game config");
            return (exchange, services);
        }

        public static GeneralConfig LoadGeneralConfig(string baseDir)
        {
            var result = GeneralConfig.Load(Path.Combine(baseDir, "data", "config.json"), baseDir);
            PerfTracker.StartupEvent("Loaded general config");
            return result;
        }

        public static GeneralSettings LoadSettings(string baseDir)
        {
            var result = GeneralSettings.Load(Path.Combine(baseDir, "data", "settings.json"));
            PerfTracker.StartupEvent("Loaded settings");
            return result;
        }

        public static CoreConfig LoadCoreConfig(string baseDir)
        {
            var result = CoreConfig.Load(Path.Combine(baseDir, "data", "core.json"));
            PerfTracker.StartupEvent("Loaded core config");
            return result;
        }

        public static GameConfig LoadGameConfig(string baseDir)
        {
            var result = GameConfig.Load(Path.Combine(baseDir, "data", "game.json"));
            PerfTracker.StartupEvent("Loaded game config");
            return result;
        }
    }
}

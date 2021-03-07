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
        public static async Task<(EventExchange, IContainer)> SetupAsync(string baseDir, IFileSystem disk)
        {
            var generalConfigTask = Task.Run(() => LoadGeneralConfig(baseDir, disk));
            var settingsTask = Task.Run(() => LoadSettings(baseDir, disk));
            var coreConfigTask = Task.Run(() => LoadCoreConfig(baseDir, disk));
            var gameConfigTask = Task.Run(() => LoadGameConfig(baseDir, disk));
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

            var factory = new VeldridCoreFactory();
            var modApplier = new ModApplier();

            var settings = await settingsTask.ConfigureAwait(false);
            var settingsManager = new SettingsManager(settings);
            var services = new Container("Services", settingsManager, // Need to register settings first, as the AssetLocator relies on it.
                new AssetLoaderRegistry(),
                new ContainerRegistry(),
                new PostProcessorRegistry(),
                new MetafontBuilder(factory),
                new StdioConsoleLogger(),
                // new ClipboardManager(),
                new ImGuiConsoleLogger(),
                new WordLookup(),
                new AssetLocator(),
                modApplier,
                new AssetManager());

            var generalConfig = await generalConfigTask.ConfigureAwait(false);

#pragma warning disable CA2000 // Dispose objects before losing scope
            var exchange = new EventExchange(new LogExchange())
                .Register<IGeneralConfig>(generalConfig)
                .Register<ICoreFactory>(factory)
                .Register<IFileSystem>(new FileSystem())
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

        public static GeneralConfig LoadGeneralConfig(string baseDir, IFileSystem disk)
        {
            var result = GeneralConfig.Load(Path.Combine(baseDir, "data", "config.json"), baseDir, disk);
            PerfTracker.StartupEvent("Loaded general config");
            return result;
        }

        public static GeneralSettings LoadSettings(string baseDir, IFileSystem disk)
        {
            var result = GeneralSettings.Load(Path.Combine(baseDir, "data", "settings.json"), disk);
            PerfTracker.StartupEvent("Loaded settings");
            return result;
        }

        public static CoreConfig LoadCoreConfig(string baseDir, IFileSystem disk)
        {
            var result = CoreConfig.Load(Path.Combine(baseDir, "data", "core.json"), disk);
            PerfTracker.StartupEvent("Loaded core config");
            return result;
        }

        public static GameConfig LoadGameConfig(string baseDir, IFileSystem disk)
        {
            var result = GameConfig.Load(Path.Combine(baseDir, "data", "game.json"), disk);
            PerfTracker.StartupEvent("Loaded game config");
            return result;
        }
    }
}

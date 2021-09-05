using System;
using System.IO;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Assets;
using UAlbion.Game.Settings;
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Debugging;

namespace UAlbion
{
    public static class AssetSystem
    {
        public static async Task<(EventExchange, IContainer)> SetupAsync(string baseDir, IFileSystem disk, IJsonUtil jsonUtil)
        {
            var configAndSettingsTask = Task.Run(() =>
            {
                var config = LoadGeneralConfig(baseDir, disk, jsonUtil);
                var settings = LoadSettings(config, disk, jsonUtil);
                return (config, settings);
            });
            var coreConfigTask = Task.Run(() => LoadCoreConfig(baseDir, disk, jsonUtil));
            var gameConfigTask = Task.Run(() => LoadGameConfig(baseDir, disk, jsonUtil));
            return await SetupCore(disk, jsonUtil, configAndSettingsTask, coreConfigTask, gameConfigTask).ConfigureAwait(false);
        }

        public static EventExchange Setup(
            IFileSystem disk,
            IJsonUtil jsonUtil,
            GeneralConfig generalConfig,
            GeneralSettings settings,
            CoreConfig coreConfig,
            GameConfig gameConfig)
        {
            var configAndSettingsTask = Task.FromResult((generalConfig, settings));
            var coreConfigTask = Task.FromResult(coreConfig);
            var gameConfigTask = Task.FromResult(gameConfig);
            var task = SetupCore(disk, jsonUtil, configAndSettingsTask, coreConfigTask, gameConfigTask);
            return task.Result.Item1;
        }

        static async Task<(EventExchange, IContainer)> SetupCore(
            IFileSystem disk,
            IJsonUtil jsonUtil,
            Task<(GeneralConfig, GeneralSettings)> configAndSettingsTask,
            Task<CoreConfig> coreConfigTask,
            Task<GameConfig> gameConfigTask)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
            if (configAndSettingsTask == null) throw new ArgumentNullException(nameof(configAndSettingsTask));
            if (coreConfigTask == null) throw new ArgumentNullException(nameof(coreConfigTask));
            if (gameConfigTask == null) throw new ArgumentNullException(nameof(gameConfigTask));

            IModApplier modApplier = new ModApplier();

            var (generalConfig, settings) = await configAndSettingsTask.ConfigureAwait(false);
            var settingsManager = new SettingsManager(settings);
            var services = new Container("Services", settingsManager, // Need to register settings first, as the AssetLocator relies on it.
                new AssetLoaderRegistry(),
                new ContainerRegistry(),
                new PostProcessorRegistry(),
                new MetafontBuilder(),
                new StdioConsoleLogger(),
                // new ClipboardManager(),
                new ImGuiConsoleLogger(),
                new WordLookup(),
                new AssetLocator(),
                modApplier,
                new AssetManager());

#pragma warning disable CA2000 // Dispose objects before losing scope
            var exchange = new EventExchange(new LogExchange())
                .Register<IGeneralConfig>(generalConfig)
                .Register(disk)
                .Register(jsonUtil)
                .Attach(services);
#pragma warning restore CA2000 // Dispose objects before losing scope

            PerfTracker.StartupEvent("Registered asset services");

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

        public static GeneralConfig LoadGeneralConfig(string baseDir, IFileSystem disk, IJsonUtil jsonUtil)
        {
            var result = GeneralConfig.Load(Path.Combine(baseDir, "data", "config.json"), baseDir, disk, jsonUtil);
            PerfTracker.StartupEvent("Loaded general config");
            return result;
        }

        static GeneralSettings LoadSettings(IGeneralConfig config, IFileSystem disk, IJsonUtil jsonUtil)
        {
            var result = GeneralSettings.Load(config, disk, jsonUtil);
            PerfTracker.StartupEvent("Loaded settings");
            return result;
        }

        static CoreConfig LoadCoreConfig(string baseDir, IFileSystem disk, IJsonUtil jsonUtil)
        {
            var result = CoreConfig.Load(Path.Combine(baseDir, "data", "core.json"), disk, jsonUtil);
            PerfTracker.StartupEvent("Loaded core config");
            return result;
        }

        public static GameConfig LoadGameConfig(string baseDir, IFileSystem disk, IJsonUtil jsonUtil)
        {
            var result = GameConfig.Load(Path.Combine(baseDir, "data", "game.json"), disk, jsonUtil);
            PerfTracker.StartupEvent("Loaded game config");
            return result;
        }
    }
}

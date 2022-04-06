using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Config;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Magic;
using UAlbion.Game.Settings;
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Assets;
using UAlbion.Game.Veldrid.Debugging;

namespace UAlbion;

public static class AssetSystem
{
    public static void LoadEvents()
    {
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Api.Event)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Core.Events.HelpEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Core.Veldrid.Events.InputEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Editor.EditorSetPropertyEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Formats.ScriptEvents.PartyMoveEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Game.Events.StartEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Game.Veldrid.Debugging.HideDebugWindowEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(IsoYawEvent)));
    }

    public static async Task<(EventExchange, IContainer)> SetupAsync(
        string baseDir,
        AssetMapping mapping,
        IFileSystem disk,
        IJsonUtil jsonUtil,
        List<string> mods)
    {
        var configAndSettingsTask = Task.Run(() =>
        {
            var config = LoadGeneralConfig(baseDir, disk, jsonUtil);
            var settings = LoadSettings(config, disk, jsonUtil);
            return (config, settings);
        });

        var configTask = Task.Run(() => LoadConfig(baseDir, disk, jsonUtil));
        return await SetupCore(mapping, disk, jsonUtil, configAndSettingsTask, configTask, mods).ConfigureAwait(false);
    }

    public static EventExchange Setup(
        AssetMapping mapping,
        IFileSystem disk,
        IJsonUtil jsonUtil,
        GeneralConfig generalConfig,
        GeneralSettings settings,
        IConfigProvider configProvider,
        List<string> mods)
    {
        var configAndSettingsTask = Task.FromResult((generalConfig, settings));
        var configTask = Task.FromResult(configProvider);
        var task = SetupCore(mapping, disk, jsonUtil, configAndSettingsTask, configTask, mods);
        return task.Result.Item1;
    }

    static async Task<(EventExchange, IContainer)> SetupCore(
        AssetMapping mapping,
        IFileSystem disk,
        IJsonUtil jsonUtil,
        Task<(GeneralConfig, GeneralSettings)> configAndSettingsTask,
        Task<IConfigProvider> configTask,
        List<string> mods)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        if (configAndSettingsTask == null) throw new ArgumentNullException(nameof(configAndSettingsTask));
        if (configTask == null) throw new ArgumentNullException(nameof(configTask));

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
            new AssetManager(),
            new SpellManager());

#pragma warning disable CA2000 // Dispose objects before losing scope
        var exchange = new EventExchange(new LogExchange())
            .Register<IGeneralConfig>(generalConfig)
            .Register(disk)
            .Register(jsonUtil)
            .Attach(services);
#pragma warning restore CA2000 // Dispose objects before losing scope

        PerfTracker.StartupEvent("Registered asset services");

        modApplier.LoadMods(mapping, generalConfig, mods ?? settings.ActiveMods);
        mapping.ConsistencyCheck();
        PerfTracker.StartupEvent("Loaded mods");

        var configProvider = await configTask.ConfigureAwait(false);
        exchange // Need to load game config after mods so asset ids can be parsed.
            .Register(configProvider)
            .Register<IGameConfigProvider>(configProvider)
            .Register<ICoreConfigProvider>(configProvider)
            .Register<IInputConfigProvider>(configProvider);

        PerfTracker.StartupEvent("Loaded configs");
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

    public static IConfigProvider LoadConfig(string baseDir, IFileSystem disk, IJsonUtil jsonUtil)
    {
        var result = new ConfigProvider(baseDir, disk, jsonUtil);
        PerfTracker.StartupEvent("Loaded game config");
        return result;
    }

    public static EventExchange SetupSimple(IFileSystem disk, AssetMapping mapping, params string[] mods)
    {
        var baseDir = ConfigUtil.FindBasePath(disk);
        var jsonUtil = new FormatJsonUtil();
        return Setup(
            mapping,
            disk,
            jsonUtil,
            LoadGeneralConfig(baseDir, disk, jsonUtil),
            new GeneralSettings(),
            LoadConfig(baseDir, disk, jsonUtil),
            mods.Length == 0 ? null : mods.ToList());
    }
}
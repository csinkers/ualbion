using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Magic;
using UAlbion.Game.Settings;
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Assets;

namespace UAlbion;

public static class AssetSystem
{
    public static void LoadEvents()
    {
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Api.Eventing.Event)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Core.Events.HelpEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Core.Veldrid.Events.PreviewInputEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Editor.EditorSetPropertyEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Formats.ScriptEvents.PartyMoveEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Game.Events.StartEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Game.Veldrid.Assets.IsoModeEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(IsoYawEvent)));
    }

#pragma warning disable CA2000 // Dispose objects before losing scope
    public static EventExchange Setup(
        string baseDir,
        string appName,
        AssetMapping mapping,
        IFileSystem disk,
        IJsonUtil jsonUtil,
        IReadOnlyList<string> mods)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));

        var pathResolver = new PathResolver(baseDir, appName);
        var settings = new SettingsManager();

        var assetServices = new Container("AssetServices");
        var exchange = new EventExchange(new LogExchange())
            .Register<IPathResolver>(pathResolver)
            .Register(disk)
            .Register(jsonUtil)
            .Attach(assetServices);

        assetServices
            .Add(settings)
            .Add(new AssetLoaderRegistry())
            .Add(new ContainerRegistry())
            .Add(new PostProcessorRegistry())
            .Add(new VarRegistry())
            .Add(new AssetLocator());
        PerfTracker.StartupEvent("Registered asset services");

        IModApplier modApplier = new ModApplier();
        exchange.Attach(modApplier);
        modApplier.LoadMods(mapping, pathResolver, mods ?? UserVars.Gameplay.ActiveMods.Read(settings));
        mapping.ConsistencyCheck();

        exchange.Attach(new Container("Logging",
            new LogHistory(),
            new StdioConsoleLogger()));

        assetServices
            .Add(new WordLookup())
            .Add(new AssetManager())
            .Add(new SpellManager());

        settings.Reload();
        PerfTracker.StartupEvent("Loaded mods");

        return exchange;
    }
#pragma warning restore CA2000 // Dispose objects before losing scope

    public static EventExchange SetupSimple(IFileSystem disk, AssetMapping mapping, params string[] mods) =>
        Setup(
            ConfigUtil.FindBasePath(disk),
            Program.AppName,
            mapping,
            disk,
            new FormatJsonUtil(),
            mods.Length == 0 ? null : mods.ToList());
}

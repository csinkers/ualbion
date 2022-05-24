﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
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
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(Event)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Core.Events.HelpEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Core.Veldrid.Events.InputEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Editor.EditorSetPropertyEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Formats.ScriptEvents.PartyMoveEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Game.Events.StartEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Game.Veldrid.Debugging.HideDebugWindowEvent)));
        Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(IsoYawEvent)));
    }

    public static (EventExchange Exchange, Container Services) Setup(
        string baseDir,
        AssetMapping mapping,
        IFileSystem disk,
        IJsonUtil jsonUtil,
        List<string> mods)
    {
        if (mapping == null) throw new ArgumentNullException(nameof(mapping));
        if (disk == null) throw new ArgumentNullException(nameof(disk));
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));

        IModApplier modApplier = new ModApplier();
        var pathResolver = new PathResolver(baseDir);

        var settings = new SettingsManager();
        var services = new Container("Services", // Need to register settings first, as the AssetLocator relies on it.
            settings,
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
            .Register<IPathResolver>(pathResolver)
            .Register(disk)
            .Register(jsonUtil)
            .Attach(services);
#pragma warning restore CA2000 // Dispose objects before losing scope

        PerfTracker.StartupEvent("Registered asset services");

        modApplier.LoadMods(mapping, pathResolver, mods ?? UserVars.Gameplay.ActiveMods.Read(settings));
        mapping.ConsistencyCheck();
        settings.Reload();
        PerfTracker.StartupEvent("Loaded mods");
        return (exchange, services);
    }

    public static EventExchange SetupSimple(IFileSystem disk, AssetMapping mapping, params string[] mods) =>
        Setup(
            ConfigUtil.FindBasePath(disk),
            mapping,
            disk,
            new FormatJsonUtil(),
            mods.Length == 0 ? null : mods.ToList()).Exchange;
}
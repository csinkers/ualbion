using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Reflection;
using UAlbion.Formats;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Text;

// args for testing isometric map export: -b Base Unpacked -t "Labyrinth Map" -id "Labyrinth.Jirinaar Map.Jirinaar"
// args for full asset export: -b Base Unpacked
// args for re-pack of exported assets: -b Unpacked Repacked
// args for combining mods for original: -b "Base SomeMod SomeOtherMod" Repacked

#pragma warning disable CA2000 // Dispose objects before losing scopes
namespace UAlbion;

static class Program
{
    internal const string AppName = "ualbion"; // Determines the AppData directory name where config settings, saved games etc will be stored.
    static void Main(string[] args)
    {
        CultureInfo.CurrentCulture
            = CultureInfo.CurrentUICulture
            = CultureInfo.DefaultThreadCurrentCulture
            = CultureInfo.DefaultThreadCurrentUICulture
            = CultureInfo.InvariantCulture;

        PerfTracker.IsTracing = true;
        PerfTracker.StartupEvent("Entered main");
        AssetSystem.LoadEvents();
        PerfTracker.StartupEvent("Built event parsers");

        var commandLine = new CommandLineOptions(args);
        if (commandLine.Mode == ExecutionMode.Exit)
            return;

        PerfTracker.StartupEvent($"Running as {commandLine.Mode}");
        var disk = new FileSystem(Directory.GetCurrentDirectory());
        var jsonUtil = new FormatJsonUtil();

        var baseDir = ConfigUtil.FindBasePath(disk);
        if (baseDir == null)
            throw new InvalidOperationException("No base directory could be found.");

        PerfTracker.StartupEvent($"Found base directory {baseDir}");

        if (commandLine.Mode == ExecutionMode.ConvertAssets)
        {
            using var converter = new AssetConverter(
                AppName,
                AssetMapping.Global,
                disk,
                jsonUtil,
                commandLine.ConvertFrom,
                commandLine.ConvertTo);

            var languages = commandLine.DumpLanguages ?? converter.DiscoverLanguages();

            var parsedIds = commandLine.DumpIds?.Select(AssetId.Parse).ToHashSet();
            converter.Convert(
                parsedIds,
                commandLine.DumpAssetTypes,
                commandLine.ConvertFilePattern,
                null,
                languages);

            return;
        }

        var exchange = AssetSystem.Setup(
            baseDir,
            AppName,
            AssetMapping.Global,
            disk,
            jsonUtil,
            commandLine.Mods);

        if (commandLine.NeedsEngine)
            BuildEngine(commandLine, exchange);

        exchange.Attach(new StdioConsoleReader()); // TODO: Only add this if running with a console window

        var assets = exchange.Resolve<IAssetManager>();
        AutodetectLanguage(exchange, assets);

        switch (commandLine.Mode) // ConvertAssets handled above as it requires a specialised asset system setup
        {
            case ExecutionMode.Game: Albion.RunGame(exchange, commandLine); break;
            //case ExecutionMode.BakeIsometric:
            //    {
            //        using var test = new IsometricTest(commandLine);
            //        exchange.Attach(test);
            //        exchange.Resolve<IEngine>().Run();
            //        break;
            //    }

            case ExecutionMode.DumpData:
                PerfTracker.BeginFrame(); // Don't need to show verbose startup logging while dumping
                var tf = new TextFormatter();
                exchange.Attach(tf);
                var parsedIds = commandLine.DumpIds?.Select(AssetId.Parse).ToArray();

                if ((commandLine.DumpFormats & DumpFormats.Json) != 0)
                {
                    var dumper = new DumpJson();
                    exchange.Attach(dumper);
                    dumper.Dump(baseDir, commandLine.DumpAssetTypes, parsedIds);
                    dumper.Remove();
                }

                if ((commandLine.DumpFormats & DumpFormats.Text) != 0)
                {
                    var dumper = new DumpText();
                    exchange.Attach(dumper);
                    dumper.Dump(baseDir, commandLine.DumpAssetTypes, parsedIds);
                    dumper.Remove();
                }

                if ((commandLine.DumpFormats & DumpFormats.Png) != 0)
                {
                    var dumper = new DumpGraphics(commandLine.DumpFormats);
                    exchange.Attach(dumper);
                    dumper.Dump(baseDir, commandLine.DumpAssetTypes, parsedIds);
                    dumper.Remove();
                }

                if ((commandLine.DumpFormats & DumpFormats.Annotated) != 0)
                {
                    var dumper = new DumpAnnotated();
                    exchange.Attach(dumper);
                    dumper.Dump(baseDir, commandLine.DumpAssetTypes, parsedIds);
                    dumper.Remove();
                }

                //if ((commandLine.DumpFormats & DumpFormats.Tiled) != 0)
                //    DumpTiled.Dump(baseDir, assets, commandLine.DumpAssetTypes, parsedIds);
                break;

            case ExecutionMode.Exit: break;
        }

        Console.WriteLine("Exiting");

        var reflectorMetadataStore = exchange.Resolve<ReflectorMetadataStore>();
        reflectorMetadataStore.SaveOverrides();

        exchange.Dispose();
    }

    static void AutodetectLanguage(EventExchange exchange, IAssetManager assets)
    {
        // Check the language saved in settings.json first
        if (assets.IsStringDefined(Base.SystemText.MainMenu_MainMenu, null)) 
            return;

        // Otherwise just use the first one we can find
        var modApplier = exchange.Resolve<IModApplier>();
        foreach (var language in modApplier.Languages.Keys)
        {
            if (assets.IsStringDefined(Base.SystemText.MainMenu_MainMenu, language))
            {
                exchange.Raise(new SetLanguageEvent(language), null);
                return;
            }
        }
    }

    static void BuildEngine(CommandLineOptions commandLine, EventExchange exchange)
    {
        PerfTracker.StartupEvent("Creating engine");
        var engine = new Engine(
            commandLine.Backend,
            commandLine.UseRenderDoc,
            true)
        {
            StartupOnly = commandLine.StartupOnly
        };

#pragma warning disable CA2000 // Dispose objects before losing scopes
        var disk = exchange.Resolve<IFileSystem>();
        var jsonUtil = exchange.Resolve<IJsonUtil>();
        var pathResolver = exchange.Resolve<IPathResolver>();
        var shaderCache = new ShaderCache(pathResolver.ResolvePath("$(CACHE)/ShaderCache"));
        var shaderLoader = new ShaderLoader();

        foreach (var shaderPath in exchange.Resolve<IModApplier>().ShaderPaths)
            shaderLoader.AddShaderDirectory(shaderPath);
#pragma warning restore CA2000 // Dispose objects before losing scopes

        var engineServices = new Container("Engine", 
            shaderCache,
            shaderLoader,
            engine,
            new ResourceLayoutSource());

        exchange.Attach(engineServices);

        var reflectorOverridePath = pathResolver.ResolvePath("$(CONFIG)/reflectorOverrides.json");
        var reflectorMetadataStore = new ReflectorMetadataStore(disk, jsonUtil, reflectorOverridePath);
        reflectorMetadataStore.LoadOverrides();

        var reflectorManager = new ReflectorManager(reflectorMetadataStore);
        exchange.Register(reflectorManager);
    }
}
#pragma warning restore CA2000 // Dispose objects before losing scopes

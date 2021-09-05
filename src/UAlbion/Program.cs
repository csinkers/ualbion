using System;
using System.Linq;
using System.Reflection;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Etm;
using UAlbion.Core.Veldrid.Sprites;
using UAlbion.Formats;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Assets;

// args for testing isometric map export: -b Base Unpacked -t "Labyrinth Map" -id "Labyrinth.Jirinaar Map.Jirinaar"
// args for full asset export: -b Base Unpacked
// args for re-pack of exported assets: -b Unpacked Repacked
// args for combining mods for original: -b "Base SomeMod SomeOtherMod" Repacked

#pragma warning disable CA2000 // Dispose objects before losing scopes
namespace UAlbion
{
    static class Program
    {
        static void Main(string[] args)
        {
            PerfTracker.StartupEvent("Entered main");
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Api.Event)));
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Core.Events.HelpEvent)));
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Core.Veldrid.Events.InputEvent)));
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Editor.EditorSetPropertyEvent)));
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Formats.ScriptEvents.PartyMoveEvent)));
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Game.Events.StartEvent)));
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(UAlbion.Game.Veldrid.Debugging.HideDebugWindowEvent)));
            Event.AddEventsFromAssembly(Assembly.GetAssembly(typeof(IsoYawEvent)));
            PerfTracker.StartupEvent("Built event parsers");

            var commandLine = new CommandLineOptions(args);
            if (commandLine.Mode == ExecutionMode.Exit)
                return;

            PerfTracker.StartupEvent($"Running as {commandLine.Mode}");
            var disk = new FileSystem();
            var jsonUtil = new FormatJsonUtil();

            var baseDir = ConfigUtil.FindBasePath(disk);
            if (baseDir == null)
                throw new InvalidOperationException("No base directory could be found.");

            PerfTracker.StartupEvent($"Found base directory {baseDir}");

            if (commandLine.Mode == ExecutionMode.ConvertAssets)
            {
                ConvertAssets.Convert(
                    disk,
                    jsonUtil,
                    commandLine.ConvertFrom,
                    commandLine.ConvertTo,
                    commandLine.DumpIds,
                    commandLine.DumpAssetTypes,
                    commandLine.ConvertFilePattern);
                return;
            }

            var (exchange, services) = AssetSystem.SetupAsync(baseDir, disk, jsonUtil).Result;
            if (commandLine.NeedsEngine)
                BuildEngine(commandLine, exchange);
            services.Add(new StdioConsoleReader());

            var assets = exchange.Resolve<IAssetManager>();
            AutodetectLanguage(exchange, assets);

            switch (commandLine.Mode) // ConvertAssets handled above as it requires a specialised asset system setup
            {
                case ExecutionMode.Game: Albion.RunGame(exchange, services, baseDir, commandLine); break;
                case ExecutionMode.BakeIsometric: IsometricTest.Run(exchange, commandLine); break;

                case ExecutionMode.DumpData:
                    PerfTracker.BeginFrame(); // Don't need to show verbose startup logging while dumping
                    var tf = new TextFormatter();
                    exchange.Attach(tf);
                    var parsedIds = commandLine.DumpIds?.Select(AssetId.Parse).ToArray();

                    if ((commandLine.DumpFormats & DumpFormats.Json) != 0)
                        DumpJson.Dump(baseDir, assets, commandLine.DumpAssetTypes, parsedIds);

                    if ((commandLine.DumpFormats & DumpFormats.Text) != 0)
                        DumpText.Dump(assets, baseDir, tf, commandLine.DumpAssetTypes, parsedIds);

                    if ((commandLine.DumpFormats & DumpFormats.Png) != 0)
                    {
                        var dumper = new DumpGraphics();
                        exchange.Attach(dumper);
                        dumper.Dump(baseDir, commandLine.DumpAssetTypes, commandLine.DumpFormats, parsedIds);
                    }

                    //if ((commandLine.DumpFormats & DumpFormats.Tiled) != 0)
                    //    DumpTiled.Dump(baseDir, assets, commandLine.DumpAssetTypes, parsedIds);
                    break;

                case ExecutionMode.Exit: break;
            }

            Console.WriteLine("Exiting");
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
            var framebuffer = new MainFramebuffer();
            var sceneRenderer = new SceneRenderer("MainRenderer", framebuffer);
            sceneRenderer // TODO: Populate from json so mods can add new render methods
                .AddRenderer(new SpriteRenderer(framebuffer), typeof(VeldridSpriteBatch))
                .AddRenderer(new EtmRenderer(framebuffer), typeof(EtmWindow))
                .AddRenderer(new SkyboxRenderer(framebuffer), typeof(Skybox))
                .AddRenderer(new DebugGuiRenderer(framebuffer), typeof(DebugGuiRenderable))
                ;

            var engine = new Engine(commandLine.Backend, commandLine.UseRenderDoc, commandLine.StartupOnly, true, sceneRenderer);
#pragma warning disable CA2000 // Dispose objects before losing scopes
            var config = exchange.Resolve<IGeneralConfig>();
            var shaderCache = new ShaderCache(config.ResolvePath("$(CACHE)/ShaderCache"));

            foreach (var shaderPath in exchange.Resolve<IModApplier>().ShaderPaths)
                shaderCache.AddShaderPath(shaderPath);
#pragma warning restore CA2000 // Dispose objects before losing scopes

            exchange
                .Attach(shaderCache)
                .Attach(framebuffer)
                .Attach(sceneRenderer)
                .Attach(engine)
                .Attach(new ResourceLayoutSource())
                ;
        }
    }
}
#pragma warning restore CA2000 // Dispose objects before losing scopes

using System;
using System.Linq;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Game;
using UAlbion.Game.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Visual;

#pragma warning disable CA2000 // Dispose objects before losing scopes
namespace UAlbion
{
    static class Program
    {
        static void Main(string[] args)
        {
            PerfTracker.StartupEvent("Entered main");
            Task.Run(() => new LogEvent(LogEvent.Level.Verbose, "Preheat Event Metadata").ToString());

            var commandLine = new CommandLineOptions(args);
            if (commandLine.Mode == ExecutionMode.Exit)
                return;

            PerfTracker.StartupEvent($"Running as {commandLine.Mode}");
            var disk = new FileSystem();
            var factory = new VeldridCoreFactory();

            var baseDir = ConfigUtil.FindBasePath(disk);
            if (baseDir == null)
                throw new InvalidOperationException("No base directory could be found.");

            PerfTracker.StartupEvent($"Found base directory {baseDir}");

            if (commandLine.Mode == ExecutionMode.ConvertAssets)
            {
                ConvertAssets.Convert(
                    disk,
                    factory,
                    commandLine.ConvertFrom,
                    commandLine.ConvertTo,
                    commandLine.DumpIds,
                    commandLine.DumpAssetTypes,
                    commandLine.ConvertFilePattern);
                return;
            }

            var setupAssetSystem = Task.Run(() => AssetSystem.SetupAsync(baseDir, disk, factory));
            using var engine = commandLine.NeedsEngine ? BuildEngine(commandLine) : null;
            var (exchange, services) = setupAssetSystem.Result;
            services.Add(new StdioConsoleReader());

            var assets = exchange.Resolve<IAssetManager>();
            AutodetectLanguage(exchange, assets);

            switch (commandLine.Mode) // ConvertAssets handled above as it requires a specialised asset system setup
            {
                case ExecutionMode.Game: Albion.RunGame(engine, exchange, services, baseDir, commandLine); break;
                case ExecutionMode.BakeIsometric: IsoBake.Bake(exchange, services, baseDir, commandLine); break;

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
                        DumpGraphics.Dump(assets, baseDir, commandLine.DumpAssetTypes, commandLine.DumpFormats, parsedIds);

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

        static VeldridEngine BuildEngine(CommandLineOptions commandLine)
        {
            PerfTracker.StartupEvent("Creating engine");
            var engine =
                new VeldridEngine(commandLine.Backend, commandLine.UseRenderDoc, commandLine.StartupOnly) { WindowTitle = "UAlbion" }
                    .AddRenderer(new SkyboxRenderer())
                    .AddRenderer(new SpriteRenderer())
                    .AddRenderer(new ExtrudedTileMapRenderer())
                    .AddRenderer(new InfoOverlayRenderer())
                    .AddRenderer(new DebugGuiRenderer());
            engine.ChangeBackend();
            return engine;
        }
    }
}
#pragma warning restore CA2000 // Dispose objects before losing scopes

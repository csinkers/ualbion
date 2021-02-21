using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Formats;
using UAlbion.Game;
using UAlbion.Game.Events;
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Audio;
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

            var baseDir = ConfigUtil.FindBasePath();
            if (baseDir == null)
                throw new InvalidOperationException("No base directory could be found.");
            PerfTracker.StartupEvent($"Found base directory {baseDir}");

            var setupAssetSystem = Task.Run(() => AssetSystem.SetupAsync(baseDir));
            PerfTracker.StartupEvent("Creating engine");
            using var engine = commandLine.NeedsEngine
                ? new VeldridEngine(commandLine.Backend, commandLine.UseRenderDoc)
                    .AddRenderer(new SkyboxRenderer())
                    .AddRenderer(new SpriteRenderer())
                    .AddRenderer(new ExtrudedTileMapRenderer())
                    .AddRenderer(new InfoOverlayRenderer())
                    .AddRenderer(new DebugGuiRenderer())
                : null;
            engine?.ChangeBackend();

            // PerfTracker.StartupEvent("Running asset tests...");
            // AssetTest(assets);
            // PerfTracker.StartupEvent("Asset tests done");

            PerfTracker.StartupEvent($"Running as {commandLine.Mode}");
            var (exchange, services) = setupAssetSystem.Result;

            // Auto-detect language
            var assets = exchange.Resolve<IAssetManager>();
            if (!assets.IsStringDefined(Base.SystemText.MainMenu_MainMenu, null))
            {
                foreach (var language in Enum.GetValues(typeof(GameLanguage)).Cast<GameLanguage>())
                {
                    if (assets.IsStringDefined(Base.SystemText.MainMenu_MainMenu, language))
                    {
                        exchange.Raise(new SetLanguageEvent(language), null);
                        break;
                    }
                }
            }

            switch (commandLine.Mode)
            {
                case ExecutionMode.Game:
                case ExecutionMode.GameWithSlavedAudio:
                    Albion.RunGame(engine, exchange, services, baseDir, commandLine);
                    break;

                case ExecutionMode.AudioSlave: 
                    exchange.Attach(new AudioManager(true));
                    break;

                case ExecutionMode.Editor: break; // TODO
                case ExecutionMode.SavedGameTests: SavedGameTests.RoundTripTest(baseDir); break;

                case ExecutionMode.DumpData:
                    PerfTracker.BeginFrame(); // Don't need to show verbose startup logging while dumping
                    var tf = new TextFormatter();
                    exchange.Attach(tf);

                    AssetId[] dumpIds = null;
                    if (commandLine.DumpIds != null)
                        dumpIds = commandLine.DumpIds.Select(AssetId.Parse).ToArray();

                    if ((commandLine.DumpFormats & DumpFormats.Json) != 0)
                        DumpJson.Dump(baseDir, assets, commandLine.DumpAssetTypes, dumpIds);

                    if ((commandLine.DumpFormats & DumpFormats.Text) != 0)
                        DumpText.Dump(assets, baseDir, tf, commandLine.DumpAssetTypes, dumpIds);

                    if ((commandLine.DumpFormats & DumpFormats.GraphicsMask) != 0)
                        DumpGraphics.Dump(assets, baseDir, commandLine.DumpAssetTypes, commandLine.DumpFormats & DumpFormats.GraphicsMask, dumpIds);

                    if ((commandLine.DumpFormats & DumpFormats.Tiled) != 0)
                        DumpTiled.Dump(baseDir, assets, commandLine.DumpAssetTypes, dumpIds);
                    break;

                case ExecutionMode.Exit: break;
            }

            Console.WriteLine("Exiting");
            exchange.Dispose();
        }
    }
}
#pragma warning restore CA2000 // Dispose objects before losing scopes

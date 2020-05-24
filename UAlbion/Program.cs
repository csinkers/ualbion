using System;
using System.Text;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Formats;
using UAlbion.Game.Assets;
using UAlbion.Game.Settings;
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Assets;
using UAlbion.Game.Veldrid.Audio;
using UAlbion.Game.Veldrid.Debugging;

namespace UAlbion
{
    static class Program
    {
        static void Main(string[] args)
        {
            /*
            Console.WriteLine("Entry point reached. Press enter to continue");
            Console.ReadLine(); //*/

            PerfTracker.StartupEvent("Entered main");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core
            PerfTracker.StartupEvent("Registered encodings");

            var commandLine = new CommandLineOptions(args);

            var baseDir = FormatUtil.FindBasePath();
            if (baseDir == null)
                throw new InvalidOperationException("No base directory could be found.");

            PerfTracker.StartupEvent($"Found base directory {baseDir}");
            PerfTracker.StartupEvent("Registering asset manager");
            var factory = new VeldridCoreFactory();
            using var assets = new AssetManager()
                .AddAssetLocator(new StandardAssetLocator())
                .AddAssetLocator(new AssetConfigLocator())
                .AddAssetLocator(new CoreSpriteLocator())
                .AddAssetLocator(new MetaFontLocator(factory))
                .AddAssetLocator(new NewStringLocator())
                .AddAssetLocator(new SoundBankLocator())
                .AddAssetPostProcessor(new AlbionSpritePostProcessor())
                .AddAssetPostProcessor(new ImageSharpPostProcessor())
                .AddAssetPostProcessor(new InterlacedBitmapPostProcessor())
                ;

            var coreServices = new Container("Core",
                        new StdioConsoleLogger(),
                        new ImGuiConsoleLogger(),
                        Settings.Load(baseDir),
                        assets); // Need to register settings first, as the AssetConfigLocator relies on it.

            var services = new Container("Services", coreServices);

            using var exchange = new EventExchange(new LogExchange())
                .Register<ICoreFactory>(factory)
                .Attach(services);

            Engine.GlobalExchange = exchange;

            PerfTracker.StartupEvent("Registered asset manager");
            PerfTracker.StartupEvent($"Running as {commandLine.Mode}");

            switch(commandLine.Mode)
            {
                case ExecutionMode.Game:
                case ExecutionMode.GameWithSlavedAudio:
                    Albion.RunGame(exchange, services, coreServices, baseDir, commandLine);
                    break;

                case ExecutionMode.AudioSlave: 
                    exchange.Attach(new AudioManager(true));
                    break;

                case ExecutionMode.Editor: break; // TODO
                case ExecutionMode.SavedGameTests: SavedGameTests.RoundTripTest(baseDir); break;

                case ExecutionMode.DumpData:
                    var tf = new TextFormatter();
                    exchange.Attach(tf);
                    DumpType dumpTypes = DumpType.All;
                    if (commandLine.GameModeArgument != null)
                    {
                        dumpTypes = 0;
                        foreach(var t in commandLine.GameModeArgument.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                            dumpTypes |= Enum.Parse<DumpType>(t);
                    }

                    if ((dumpTypes & DumpType.Characters) != 0) Dump.CharacterSheets(assets, tf, baseDir);
                    if ((dumpTypes & DumpType.Chests) != 0) Dump.Chests(assets, baseDir);
                    if ((dumpTypes & DumpType.CoreSprites) != 0) Dump.CoreSprites(assets, baseDir);
                    if ((dumpTypes & DumpType.EventSets) != 0) Dump.EventSets(assets, baseDir);
                    if ((dumpTypes & DumpType.Items) != 0) Dump.ItemData(assets, baseDir);
                    if ((dumpTypes & DumpType.MapEvents) != 0) Dump.MapEvents(assets, baseDir);
                    if ((dumpTypes & DumpType.Maps) != 0) Dump.MapData(assets, tf, baseDir);
                    if ((dumpTypes & DumpType.Spells) != 0) Dump.Spells(assets, tf, baseDir);
                    if ((dumpTypes & DumpType.ThreeDMaps) != 0) Dump.ThreeDMapAndLabInfo(assets, baseDir);
                    break;

                case ExecutionMode.Exit: break;
            }
        }
    }
}

﻿using System;
using System.Text;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Parsers;
using UAlbion.Game.Assets;
using UAlbion.Game.Settings;
using UAlbion.Game.Text;
using UAlbion.Game.Veldrid.Assets;
using UAlbion.Game.Veldrid.Audio;
using UAlbion.Game.Veldrid.Debugging;

#pragma warning disable CA2000 // Dispose objects before losing scopes
namespace UAlbion
{
    static class Program
    {
        static void Main(string[] args)
        {
            PerfTracker.StartupEvent("Entered main");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core
            PerfTracker.StartupEvent("Registered encodings");

            var commandLine = new CommandLineOptions(args);
            if (commandLine.Mode == ExecutionMode.Exit)
                return;

            var baseDir = FormatUtil.FindBasePath();
            if (baseDir == null)
                throw new InvalidOperationException("No base directory could be found.");

            PerfTracker.StartupEvent($"Found base directory {baseDir}");
            PerfTracker.StartupEvent("Registering asset manager");
            var factory = new VeldridCoreFactory();
            var loaderRegistry = new AssetLoaderRegistry()
                .AddLoader(new AlbionStringTableLoader())
                .AddLoader(new AmorphousSpriteLoader())
                .AddLoader(new BlockListLoader())
                .AddLoader(new ChestLoader())
                .AddLoader(new CharacterSheetLoader())
                .AddLoader(new EventSetLoader())
                .AddLoader(new FixedSizeSpriteLoader())
                .AddLoader(new FlicLoader())
                .AddLoader(new FontSpriteLoader())
                .AddLoader(new HeaderBasedSpriteLoader())
                .AddLoader(new InterlacedBitmapLoader())
                .AddLoader(new ItemDataLoader())
                .AddLoader(new ItemNameLoader())
                .AddLoader(new LabyrinthDataLoader())
                .AddLoader(new MapLoader())
                .AddLoader(new MonsterGroupLoader())
                .AddLoader(new PaletteLoader())
                .AddLoader(new SampleLoader())
                .AddLoader(new SavedGameLoader())
                .AddLoader(new ScriptLoader())
                .AddLoader(new SlabLoader())
                .AddLoader(new SongLoader())
                .AddLoader(new SpellLoader())
                .AddLoader(new SystemTextLoader())
                .AddLoader(new TilesetDataLoader())
                .AddLoader(new WordListLoader());

            var locatorRegistry = new AssetLocatorRegistry()
                .AddAssetLocator(new StandardAssetLocator(loaderRegistry), true)
                .AddAssetLocator(new AssetConfigLocator(commandLine.Mode == ExecutionMode.DumpData))
                .AddAssetLocator(new CoreSpriteLocator())
                .AddAssetLocator(new MetaFontLocator(factory))
                .AddAssetLocator(new NewStringLocator())
                .AddAssetLocator(new SoundBankLocator())
                .AddAssetLocator(new SavedGameLocator(loaderRegistry))
                .AddAssetPostProcessor(new AlbionSpritePostProcessor())
                .AddAssetPostProcessor(new ImageSharpPostProcessor())
                .AddAssetPostProcessor(new InterlacedBitmapPostProcessor())
                .AddAssetPostProcessor(new InventoryPostProcessor())
                ;

            var assets = new AssetManager();
            var services = new Container("Services", 
                new StdioConsoleLogger(),
                // new ClipboardManager(),
                new ImGuiConsoleLogger(),
                GeneralSettings.Load(baseDir), // Need to register settings first, as the AssetConfigLocator relies on it.
                loaderRegistry,
                locatorRegistry,
                assets);

            using var exchange = new EventExchange(new LogExchange())
                .Register<ICoreFactory>(factory)
                .Attach(services);

            Engine.GlobalExchange = exchange;

            PerfTracker.StartupEvent("Registered asset manager");
            PerfTracker.StartupEvent($"Running as {commandLine.Mode}");

            switch (commandLine.Mode)
            {
                case ExecutionMode.Game:
                case ExecutionMode.GameWithSlavedAudio:
                    Albion.RunGame(exchange, services, baseDir, commandLine);
                    break;

                case ExecutionMode.AudioSlave: 
                    exchange.Attach(new AudioManager(true));
                    break;

                case ExecutionMode.Editor: break; // TODO
                case ExecutionMode.SavedGameTests: SavedGameTests.RoundTripTest(baseDir, loaderRegistry); break;

                case ExecutionMode.DumpData:
                    PerfTracker.BeginFrame(); // Don't need to show verbose startup logging while dumping
                    var tf = new TextFormatter();
                    exchange.Attach(tf);

                    if ((commandLine.DumpFormats & DumpFormats.Json) != 0) DumpJson.Dump(baseDir, assets, commandLine.DumpAssetTypes);
                    if ((commandLine.DumpFormats & DumpFormats.Text) != 0) DumpText.Dump(assets, baseDir, tf, commandLine.DumpAssetTypes);
                    if ((commandLine.DumpFormats & DumpFormats.GraphicsMask) != 0)
                        DumpGraphics.Dump(assets, baseDir, commandLine.DumpAssetTypes, commandLine.DumpFormats & DumpFormats.GraphicsMask);
                    break;

                case ExecutionMode.Exit: break;
            }
        }
    }
}
#pragma warning restore CA2000 // Dispose objects before losing scopes

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using UAlbion.Formats.Containers;
using UAlbion.Game;
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
            Task.Run(() => new LogEvent(LogEvent.Level.Verbose, "Preheat Event Metadata").ToString());

            var commandLine = new CommandLineOptions(args);
            if (commandLine.Mode == ExecutionMode.Exit)
                return;

            var baseDir = ConfigUtil.FindBasePath();
            if (baseDir == null)
                throw new InvalidOperationException("No base directory could be found.");
            PerfTracker.StartupEvent($"Found base directory {baseDir}");

            var setupAssetSystem = Task.Run(() => SetupAssetSystem(baseDir));
            PerfTracker.StartupEvent("Creating engine");
            using var engine = commandLine.NeedsEngine
                ? new VeldridEngine(commandLine.Backend, commandLine.UseRenderDoc)
                    .AddRenderer(new SkyboxRenderer())
                    .AddRenderer(new SpriteRenderer())
                    .AddRenderer(new ExtrudedTileMapRenderer())
                    .AddRenderer(new DebugGuiRenderer())
                : null;
            engine?.ChangeBackend();

            // PerfTracker.StartupEvent("Running asset tests...");
            // AssetTest(assets);
            // PerfTracker.StartupEvent("Asset tests done");

            PerfTracker.StartupEvent($"Running as {commandLine.Mode}");
            var (exchange, services) = setupAssetSystem.Result;
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
                    var assets = exchange.Resolve<IAssetManager>();
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

            Console.WriteLine("Exiting");
        }

        static async Task<(EventExchange, IContainer)> SetupAssetSystem(string baseDir)
        {
            var generalConfigTask =  Task.Run(() =>
            {
                var result = GeneralConfig.Load(Path.Combine(baseDir, "data", "config.json"), baseDir);
                PerfTracker.StartupEvent("Loaded general config");
                return result;
            });
            var settingsTask = Task.Run(() =>
            {
                var result = GeneralSettings.Load(Path.Combine(baseDir, "data", "settings.json"));
                PerfTracker.StartupEvent("Loaded settings");
                return result;
            });
            var coreConfigTask = Task.Run(() =>
            {
                var result = CoreConfig.Load(Path.Combine(baseDir, "data", "core.json"));
                PerfTracker.StartupEvent("Loaded core config");
                return result;
            });
            var gameConfigTask = Task.Run(() =>
            {
                var result = GameConfig.Load(Path.Combine("data", "game.json"));
                PerfTracker.StartupEvent("Loaded game config");
                return result;
            });

            var assets = new AssetManager();
            var factory = new VeldridCoreFactory();
            var loaderRegistry = new AssetLoaderRegistry();
            var locatorRegistry = new AssetLocatorRegistry();
            var containerLoaderRegistry = new ContainerLoaderRegistry().AddLoader(new RawContainerLoader())
                .AddLoader(new XldContainerLoader())
                .AddLoader(new BinaryOffsetContainerLoader())
                .AddLoader(new ItemListContainerLoader())
                .AddLoader(new SpellListContainerLoader());

            var modApplier = new ModApplier()
                // Register post-processors for handling transformations of asset data that can't be done by UAlbion.Formats alone.
                .AddAssetPostProcessor(new AlbionSpritePostProcessor())
                .AddAssetPostProcessor(new ImageSharpPostProcessor())
                .AddAssetPostProcessor(new InterlacedBitmapPostProcessor())
                .AddAssetPostProcessor(new InventoryPostProcessor())
                .AddAssetPostProcessor(new ItemNamePostProcessor());

            var settings = await settingsTask.ConfigureAwait(false);
            var services = new Container("Services", settings, // Need to register settings first, as the AssetLocator relies on it.
                loaderRegistry, locatorRegistry, containerLoaderRegistry, new MetafontBuilder(factory), new StdioConsoleLogger(),
                // new ClipboardManager(),
                new ImGuiConsoleLogger(), new WordLookup(), new AssetLocator(), modApplier, assets);

            var generalConfig = await generalConfigTask.ConfigureAwait(false);
            using var exchange = new EventExchange(new LogExchange()).Register<IGeneralConfig>(generalConfig)
                .Register<ICoreFactory>(factory)
                .Attach(services);
            PerfTracker.StartupEvent("Registered asset services");

            Engine.GlobalExchange = exchange;
            generalConfig.SetPath("LANG", settings.Language.ToString()
                .ToUpperInvariant()); // Ensure that the LANG path is set before resolving any assets
            modApplier.LoadMods(generalConfig);
            PerfTracker.StartupEvent("Loaded mods");

            var coreConfig = await coreConfigTask.ConfigureAwait(false);
            var gameConfig = await gameConfigTask.ConfigureAwait(false);
            exchange // Need to load game config after mods so asset ids can be parsed.
                .Register(coreConfig)
                .Register(gameConfig);
            PerfTracker.StartupEvent("Loaded core and game config");
            return (exchange, services);
        }

        static void AssetTest(AssetManager assets)
        {
            var item = assets.LoadItem(Base.Item.LughsShield);
            var itemName = assets.LoadString(item.Name);
            var fontTex = assets.LoadTexture(Base.Font.RegularFont);
            var font = assets.LoadFont(FontColor.White, false);
            var blocklist = assets.LoadBlockList(Base.BlockList.Toronto);
            var chest = assets.LoadInventory(AssetId.From(Base.Chest.Unknown121));
            var combatBackground = assets.LoadTexture(Base.CombatBackground.Toronto);
            var combatGraphics = assets.LoadTexture(Base.CombatGraphics.Unknown27);
            var coreSprite = assets.LoadTexture(Base.CoreSprite.Cursor);
            var dungeonBackground = assets.LoadTexture(Base.DungeonBackground.EarlyGameL);
            var dungeonObject = assets.LoadTexture(Base.DungeonObject.Barrel);
            var floor = assets.LoadTexture(Base.Floor.Water);
            var fullBodyPicture = assets.LoadTexture(Base.FullBodyPicture.Tom);
            var itemGraphics = assets.LoadTexture(Base.ItemGraphics.ItemSprites);
            var labyrinthData = assets.LoadLabyrinthData(Base.LabyrinthData.Unknown125);
            var largeNpc = assets.LoadTexture(Base.LargeNpc.Christine);
            var largePartyMember = assets.LoadTexture(Base.LargePartyMember.Tom);
            var mapText = assets.LoadString(Base.MapText.TorontoBegin);
            var merchant = assets.LoadInventory(AssetId.From(Base.Merchant.Unknown109));
            var monster = assets.LoadSheet(Base.Monster.Krondir1);
            var monsterGraphics = assets.LoadTexture(Base.MonsterGraphics.Krondir);
            var monsterGroup = assets.LoadMonsterGroup(Base.MonsterGroup.TwoSkrinn1OneKrondir1);
            var npc = assets.LoadSheet(Base.Npc.Christine);
            var paletteNull = assets.LoadPalette(Base.Palette.CommonPalette);
            var palette = assets.LoadPalette(Base.Palette.Toronto2D);
            var partyMember = assets.LoadSheet(Base.PartyMember.Tom);
            var picture = assets.LoadTexture(Base.Picture.OpenChestWithGold);
            var portrait = assets.LoadTexture(Base.Portrait.Tom);
            var sample = assets.LoadSample(Base.Sample.IllTemperedLlama);
            var script = assets.LoadScript(Base.Script.Unknown1);
            var smallNpc = assets.LoadTexture(Base.SmallNpc.Krondir);
            var smallPartyMember = assets.LoadTexture(Base.SmallPartyMember.Tom);
            var song = assets.LoadSong(Base.Song.Toronto);
            var spell = assets.LoadSpell(Base.Spell.FrostAvalanche);
            var systemText = assets.LoadString(Base.SystemText.MainMenu_MainMenu);
            var tacticalGraphics = assets.LoadTexture(Base.TacticalGraphics.Unknown1);
            var tilesetData = assets.LoadTileData(Base.TilesetData.Toronto);
            var tilesetGraphics = assets.LoadTexture(Base.TilesetGraphics.Toronto);
            var uAlbionString = assets.LoadString(Base.UAlbionString.TakeAll);
            var uiBackground = assets.LoadTexture(Base.UiBackground.SLAB);
            var video = assets.LoadVideo(Base.Video.MagicDemonstration);
            var wall = assets.LoadTexture(Base.Wall.TorontoPanelling);
            var wallOverlay = assets.LoadTexture(Base.WallOverlay.JiriWindow);
            var word = assets.LoadString(Base.Word.Unknown0);
            var map = assets.LoadMap(Base.Map.TorontoBegin);
            var eventSet = assets.LoadEventSet(Base.EventSet.Frill);
            var eventText = assets.LoadString(Base.EventText.Frill);
            var waveLibrary = assets.LoadWaveLib(Base.WaveLibrary.TorontoAmbient);
            var automapTiles = assets.LoadTexture(Base.AutomapTiles.Set1);
            var automap = assets.LoadAutomap(Base.Automap.Jirinaar);
        }
    }
}
#pragma warning restore CA2000 // Dispose objects before losing scopes

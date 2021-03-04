using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Config;
using Veldrid;

namespace UAlbion
{
    class CommandLineOptions
    {
        public ExecutionMode Mode { get; }
        public GameMode GameMode { get; }
        public string GameModeArgument { get; }

        public GraphicsBackend Backend { get; }
        public bool DebugMenus { get; }
        public bool Mute { get; }
        public bool NeedsEngine => Mode == ExecutionMode.Game;
        public bool StartupOnly { get; }
        public bool UseRenderDoc { get; }
        public string ConvertFrom { get; }
        public string ConvertTo { get; }

        public string[] Commands { get; }
        public string[] DumpIds { get; }
        public DumpFormats DumpFormats { get; } = DumpFormats.Json;
        public ISet<AssetType> DumpAssetTypes { get; }

        public CommandLineOptions(string[] args)
        {
            // Defaults
            Mode = ExecutionMode.Game;
            GameMode = GameMode.MainMenu;
            Backend = GraphicsBackend.Vulkan;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i].ToUpperInvariant();

                // Mode
                if (arg == "--GAME") Mode = ExecutionMode.Game;
                if (arg == "--DUMP" || arg == "-D") Mode = ExecutionMode.DumpData;
                if (arg == "--CONVERT" || arg == "--BUILD" || arg == "-B")
                {
                    if (i +2 >= args.Length)
                        throw new FormatException("\"--convert\" requires two parameters: the mod to convert from and the mod to convert to");
                    ConvertFrom = args[++i];
                    ConvertTo = args[++i];
                    Mode = ExecutionMode.ConvertAssets;
                }

                if (arg == "-H" || arg == "--HELP" || arg == "/?" || arg == "HELP")
                {
                    DisplayUsage();
                    Mode = ExecutionMode.Exit;
                    return;
                }

                // Options
                if (arg == "-GL" || arg == "--OPENGL") Backend = GraphicsBackend.OpenGL;
                if (arg == "-GLES" || arg == "--OPENGLES") Backend = GraphicsBackend.OpenGLES;
                if (arg == "-VK" || arg == "--VULKAN") Backend = GraphicsBackend.Vulkan;
                if (arg == "-METAL" || arg == "--METAL") Backend = GraphicsBackend.Metal;
                if (arg == "-D3D" || arg == "--DIRECT3D") Backend = GraphicsBackend.Direct3D11;

                if (arg == "--MENUS") DebugMenus = true;
                if (arg == "--NO-AUDIO") Mute = true;
                if (arg == "--STARTUPONlY" || arg == "-S") StartupOnly = true;
                if (arg == "--RENDERDOC" || arg == "-RD") UseRenderDoc = true;

                if (arg == "--COMMANDS" || arg == "-C")
                {
                    i++;
                    if (i == args.Length)
                    {
                        Console.WriteLine("\"-c\" requires an argument specifying the commands to run");
                        Mode = ExecutionMode.Exit;
                        return;
                    }

                    Commands = args[i].Split(';').Select(x => x.Trim()).ToArray();
                }

                if (arg == "--TYPE" || arg == "-T")
                {
                    i++;
                    if (i == args.Length)
                    {
                        Console.WriteLine("\"-type\" requires an argument specifying the asset types to process");
                        Mode = ExecutionMode.Exit;
                        return;
                    }

                    DumpAssetTypes = new HashSet<AssetType>();
                    foreach (var type in args[i].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        DumpAssetTypes.Add(Enum.Parse<AssetType>(type, true));
                }

                if (arg == "--ID" || arg == "-ID" || arg == "-IDS" || arg == "--IDS")
                {
                    i++;
                    if (i == args.Length)
                    {
                        Console.WriteLine("\"-id\" requires an argument specifying the ids to process");
                        Mode = ExecutionMode.Exit;
                        return;
                    }
                    DumpIds = args[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                }

                if (arg == "--FORMATS" || arg == "--FORMAT" || arg == "-F")
                {
                    i++;
                    if (i == args.Length)
                    {
                        Console.WriteLine("\"--formats\" requires an argument specifying the formats to process");
                        Mode = ExecutionMode.Exit;
                        return;
                    }
                    DumpFormats = 0;
                    foreach (var type in args[i].Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        DumpFormats |= Enum.Parse<DumpFormats>(type, true);
                }

                // SubModes
                if (arg == "--MAIN-MENU") GameMode = GameMode.MainMenu;
                if (arg == "--NEW-GAME") GameMode = GameMode.NewGame;
                if (arg == "--INVENTORY") GameMode = GameMode.Inventory;

                if (arg == "--LOAD" || arg == "-L")
                {
                    i++;
                    if (i == args.Length)
                    {
                        Console.WriteLine("\"--load-game\" requires an argument specifying the saved game to load");
                        Mode = ExecutionMode.Exit;
                        return;
                    }
                    GameMode = GameMode.LoadGame;
                    GameModeArgument = args[i];
                }

                if (arg == "--LOAD-MAP" || arg == "-MAP" || arg == "--MAP" || arg == "-M")
                {
                    i++;
                    if (i == args.Length)
                    {
                        Console.WriteLine("\"--load-map\" requires an argument specifying the map to load");
                        Mode = ExecutionMode.Exit;
                        return;
                    }
                    GameMode = GameMode.LoadMap;
                    GameModeArgument = args[i];
                }
            }
        }

        static void DisplayUsage()
        {
            var formats = string.Join(" ", 
                Enum.GetValues(typeof(DumpFormats))
                    .Cast<DumpFormats>()
                    .Select(x => x.ToString()));

            var dumpTypes = string.Join(" ",
                Enum.GetValues(typeof(AssetType))
                    .Cast<AssetType>()
                    .Select(x => x.ToString()));

            Console.WriteLine($@"UAlbion
Command Line Options:
(Note: To specify multiple values for any argument use double quotes and separate values with spaces)

Execution Mode:
    --game            : Runs the game (default)
    --help -h /? help : Display this help
    --dump -d         : Dump assets to the data/exported directory
    --convert --build -b <FromMod> <ToMod> : Convert all assets from one mod's asset formats to another (e.g. Base->Unpacked, Unpacked->Repacked etc)

Rendering Backend:
    --opengl    -gl    : Use OpenGL
    --opengles  -gles  : Use OpenGLES
    --vulkan    -vk    : Use Vulkan
    --metal     -metal : Use Metal
    --direct3d  -d3d   : Use Direct3D11

Options:
    --commands -c <Commands> : Raise the given events (semicolon separated list) on startup.
    --menus          : Show debug menus
    --no-audio       : Runs the game without audio
    --startuponly -s : Exit immediately after the first frame (for profiling startup time etc)
    --renderdoc -rd  : Load the RenderDoc plugin on startup

Dump / Convert options:
    --formats -f <Formats>    : Specifies the formats for the dumped data (defaults to JSON, valid formats: {formats})
    --id --ids -id -ids <Ids> : Dump specific asset ids (space separated list)
    --type -t <Types>         : Dump specific types of game data (space separated list, valid types: {dumpTypes})

Game Mode: (if running as game)
    --main-menu (default)              : Show the main menu on startup
    --new-game                         : Begin a new game immediately
    --load -l <slotNumber> : Load the specified saved game
    --load-map --map -map <mapId>      : Start a new game on the specified map
    --inventory                        : Start a new game and load the inventory screen
");
        }
    }
}

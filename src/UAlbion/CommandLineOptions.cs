using System;
using System.Linq;
using Veldrid;

namespace UAlbion
{
    class CommandLineOptions
    {
        public GraphicsBackend Backend { get; }
        public bool StartupOnly { get; }
        public bool UseRenderDoc { get; }
        public bool DebugMenus { get; }
        public ExecutionMode Mode { get; }
        public AudioMode AudioMode { get; }
        public GameMode GameMode { get; }
        public string GameModeArgument { get; }
        public string[] Commands { get; }

        public CommandLineOptions(string[] args)
        {
            Mode = ExecutionMode.Game;
            Backend = GraphicsBackend.Vulkan;
            StartupOnly = args.Contains("--startuponly");
            UseRenderDoc = args.Contains("--renderdoc") || args.Contains("-rd");
            DebugMenus = args.Contains("--menus");
            if (args.Contains("-gl") || args.Contains("--opengl")) Backend = GraphicsBackend.OpenGL;
            if (args.Contains("-gles") || args.Contains("--opengles")) Backend = GraphicsBackend.OpenGLES;
            if (args.Contains("-vk") || args.Contains("--vulkan")) Backend = GraphicsBackend.Vulkan;
            if (args.Contains("-metal") || args.Contains("--metal")) Backend = GraphicsBackend.Metal;
            if (args.Contains("-d3d") || args.Contains("--direct3d")) Backend = GraphicsBackend.Direct3D11;

            var commandString = args.SkipWhile(x => x != "-c").Skip(1).FirstOrDefault();
            if (commandString != null)
                Commands = commandString.Split(';').Select(x => x.Trim()).ToArray();

            if (args.Contains("-h") || args.Contains("--help") || args.Contains("/?") || args.Contains("help"))
            {
                DisplayUsage();
                Mode = ExecutionMode.Exit;
                return;
            }

            Mode = ExecutionMode.Game;
            GameMode = GameMode.MainMenu;
            AudioMode = AudioMode.InProcess;

            if (args.Contains("--no-audio"))
                AudioMode = AudioMode.None;
            if (args.Contains("--external-audio"))
                AudioMode = AudioMode.ExternalProcess;
            if (args.Contains("--audio"))
                Mode = ExecutionMode.AudioSlave;
            if (args.Contains("--editor"))
                Mode = ExecutionMode.Editor;
            if (args.Contains("--save-tests"))
                Mode = ExecutionMode.SavedGameTests;

            if (args.Contains("--dump-all"))
                Mode = ExecutionMode.DumpData;

            var index = FindArgIndex("--dump", args);
            if (index != -1)
            {
                Mode = ExecutionMode.DumpData;
                GameModeArgument = args[index + 1];
            }

            if (Mode == ExecutionMode.Game)
            {
                if (args.Contains("--new-game"))
                    GameMode = GameMode.NewGame;

                if (args.Contains("--inventory"))
                    GameMode = GameMode.Inventory;

                index = FindArgIndex("--load-game", args);
                if (index > -1)
                {
                    GameMode = GameMode.LoadGame;
                    GameModeArgument = args[index + 1];
                }

                index = FindArgIndex("--load-map", args);
                if (index > -1)
                {
                    GameMode = GameMode.LoadMap;
                    GameModeArgument = args[index + 1];
                }
            }
        }

        int FindArgIndex(string argument, string[] arguments)
        {
            for (int i = 0; i < arguments.Length; i++)
                if (arguments[i] == argument)
                    return i;

            return -1;
        }

        void DisplayUsage()
        {
            var dumpTypes = string.Join(" ",
                Enum.GetValues(typeof(DumpTypes))
                    .Cast<DumpTypes>()
                    .Select(x => x.ToString()));

            Console.WriteLine($@"UAlbion
Command Line Options:

    -h --help /? help  : Display this help
    --startuponly      : Exit immediately after the first frame (for profiling startup time etc)
    -rd    --renderdoc : Load the RenderDoc plugin on startup
    -c ""commands""    : Raise the given events (semicolon separated list) on startup.

Set Rendering Backend:
    -gl    --opengl     Use OpenGL
    -gles  --opengles   Use OpenGLES
    -vk    --vulkan     Use Vulkan
    -metal --metal      Use Metal
    -d3d   --direct3d   Use Direct3D11

Execution Mode:
    --game : Runs the game (default)
    --no-audio: Runs the game without audio
    --external-audio: Runs the game with audio delegated to a worker process
    --audio : Runs as an audio worker process
    --editor : Runs the editor (TODO)
    --save-tests : Runs the saved game tests
    --dump-all : Dumps a variety of game data to text files
    --dump ""[dumpTypes]"": Dump specific types of game data (space separated list, valid types: {dumpTypes})

Game Mode: (if running as game)
        --main-menu (default)
        --new-game
        --load-game <slotNumber>
        --load-map <mapId>
        --inventory
");
        }
    }
}

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
        public ExecutionMode Mode { get; }

        public CommandLineOptions(string[] args)
        {
            Mode = ExecutionMode.Game;
            Backend = GraphicsBackend.Direct3D11;
            StartupOnly = args.Contains("--startuponly");
            UseRenderDoc = args.Contains("--renderdoc") || args.Contains("-rd");
            if (args.Contains("-gl") || args.Contains("--opengl")) Backend = GraphicsBackend.OpenGL;
            if (args.Contains("-gles") || args.Contains("--opengles")) Backend = GraphicsBackend.OpenGLES;
            if (args.Contains("-vk") || args.Contains("--vulkan")) Backend = GraphicsBackend.Vulkan;
            if (args.Contains("-metal") || args.Contains("--metal")) Backend = GraphicsBackend.Metal;
            if (args.Contains("-d3d") || args.Contains("--direct3d")) Backend = GraphicsBackend.Direct3D11;

            if (args.Contains("-h") || args.Contains("--help") || args.Contains("/?") || args.Contains("help"))
            {
                DisplayUsage();
                Mode = ExecutionMode.Exit;
                return;
            }

            Mode = ExecutionMode.Game;
            if (args.Contains("--game-no-audio"))
                Mode = ExecutionMode.GameWithSlavedAudio;
            if (args.Contains("--audio"))
                Mode = ExecutionMode.AudioSlave;
            if (args.Contains("--editor"))
                Mode = ExecutionMode.Editor;
            if (args.Contains("--save-tests"))
                Mode = ExecutionMode.SavedGameTests;
            if (args.Contains("--dump-data"))
                Mode = ExecutionMode.DumpData;
        }

        void DisplayUsage()
        {
            Console.WriteLine(@"UAlbion
Command Line Options:

    -h --help /? help  : Display this help
    --startuponly      : Exit immediately after the first frame (for profiling startup time etc)
    -rd    --renderdoc : Load the RenderDoc plugin on startup

Set Rendering Backend:
    -gl    --opengl     Use OpenGL
    -gles  --opengles   Use OpenGLES
    -vk    --vulkan     Use Vulkan
    -metal --metal      Use Metal
    -d3d   --direct3d   Use Direct3D11

Execution Mode:
    --game : Runs the game (default)
    --game-no-audio: Runs the game with audio delegated to a worker process
    --audio : Runs an audio worker process
    --editor : Runs the editor (TODO)
    --save-tests : Runs the saved game tests
    --dump-data : Dumps a variety of game data to text files
");
        }
    }
}

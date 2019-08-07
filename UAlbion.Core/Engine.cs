using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ImGuiNET;
using UAlbion.Core.Events;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using UAlbion.Core.Objects;
using UAlbion.Core.Textures;

namespace UAlbion.Core
{
    /*
    class CameraControls : Component
    {
        public CameraControls() : base(Handlers)
        {
        }

        public static IList<Handler> Handlers = new Handler[] { new Handler<CameraControls,>(), }
    } */

    public class Engine : Component, IDisposable
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Engine, ToggleFullscreenEvent>((x, _) => x.ToggleFullscreenState()),
            new Handler<Engine, LoadRenderDocEvent>((x, _) =>
            {
                if (_renderDoc == null && RenderDoc.Load(out _renderDoc))
                    x.ChangeBackend(x.GraphicsDevice.BackendType, true);
            }),
            new Handler<Engine, ToggleResizableEvent>((x, _) => x.Window.Resizable = !x.Window.Resizable),
            new Handler<Engine, ToggleVisibleBorderEvent>((x, _) => x.Window.BorderVisible = !x.Window.BorderVisible),
            new Handler<Engine, QuitEvent>((x, e) => x.Window.Close())
        };

        static RenderDoc _renderDoc;

        public ITextureManager TextureManager { get; }
        public bool LimitFrameRate { get; set; } = true;
        readonly double _desiredFrameLengthSeconds = 1.0 / 60.0;
        readonly FrameTimeAverager _frameTimeAverager = new FrameTimeAverager(0.5);
        readonly FullScreenQuad _fullScreenQuad;
        readonly ScreenDuplicator _duplicator;
        readonly DebugGuiRenderer _igRenderable;
        readonly SceneContext _sceneContext = new SceneContext();
        readonly DebugMenus _debugMenus;

        static Engine _instance;

        Scene _scene;
        CommandList _frameCommands;
        TextureSampleCount? _newSampleCount;
        bool _windowResized;
        bool _recreateWindow = true;

        internal GraphicsDevice GraphicsDevice { get; private set; }
        internal Sdl2Window Window { get; private set; }
        internal RenderDoc RenderDoc => _renderDoc;
        internal string FrameTimeText => _frameTimeAverager.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _frameTimeAverager.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms");

        public Scene Create2DScene()
        {
            var camera = new OrthographicCamera(Window);
            var scene = new Scene(camera);
            scene.AddRenderer(_igRenderable);
            scene.AddRenderer(_duplicator);
            scene.AddRenderer(_fullScreenQuad);

            scene.AddComponent(this);
            scene.AddComponent(camera);
            scene.AddComponent(_igRenderable);
            scene.AddComponent(_duplicator);
            scene.AddComponent(_fullScreenQuad);
            scene.AddComponent(_debugMenus);
            return scene;
        }

        public Scene Create3DScene()
        {
            var camera = new PerspectiveCamera(GraphicsDevice, Window);
            var scene = new Scene(camera);
            scene.AddRenderer(_igRenderable);
            scene.AddRenderer(_duplicator);
            scene.AddRenderer(_fullScreenQuad);

            scene.AddComponent(this);
            scene.AddComponent(camera);
            scene.AddComponent(_igRenderable);
            scene.AddComponent(_duplicator);
            scene.AddComponent(_fullScreenQuad);
            scene.AddComponent(_debugMenus);
            return scene;
        }

        public void SetScene(Scene scene)
        {
            _scene = scene;
            _sceneContext.SetCurrentScene(_scene);
            CreateAllObjects();
            ImGui.StyleColorsClassic();
        }

        public Engine() : base(Handlers)
        {
            _instance = this;
            TextureManager = new TextureManager();
            ChangeBackend(
                //VeldridStartup.GetPlatformDefaultBackend()
                //GraphicsBackend.Metal
                //GraphicsBackend.Vulkan
                //GraphicsBackend.OpenGL
                //GraphicsBackend.OpenGLES
                GraphicsBackend.Direct3D11
                );

            _igRenderable = new DebugGuiRenderer(Window.Width, Window.Height);
            _duplicator = new ScreenDuplicator();
            _fullScreenQuad = new FullScreenQuad();
            _debugMenus = new DebugMenus(this);
            Engine.CheckForErrors();

            Sdl2Native.SDL_Init(SDLInitFlags.GameController);
            Engine.CheckForErrors();
        }

        public void Run()
        {
            if (_scene == null)
                throw new InvalidOperationException("The scene must be set before the main loop can be run.");

            long previousFrameTicks = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (Window.Exists)
            {
                long currentFrameTicks = sw.ElapsedTicks;
                double deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

                while (LimitFrameRate && deltaSeconds < _desiredFrameLengthSeconds)
                {
                    var millisecondsToSleep = (int)((_desiredFrameLengthSeconds - deltaSeconds) * 1000);
                    if (millisecondsToSleep > 10)
                        Thread.Sleep(millisecondsToSleep - 1);
                    currentFrameTicks = sw.ElapsedTicks;
                    deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;
                }

                previousFrameTicks = currentFrameTicks;

                InputSnapshot snapshot = null;
                Sdl2Events.ProcessEvents();
                snapshot = Window.PumpEvents();
                InputTracker.UpdateFrameInput(snapshot, Window);
                Update((float)deltaSeconds);
                if (!Window.Exists)
                    break;

                Draw();
            }

            DestroyAllObjects();
            GraphicsDevice.Dispose();
        }

        void Update(float deltaSeconds)
        {
            _frameTimeAverager.AddTime(deltaSeconds);
            _scene.Exchange.Raise(new EngineUpdateEvent(deltaSeconds), this);
            Window.Title = GraphicsDevice.BackendType.ToString();
        }

        internal void ChangeMsaa(int msaaOption)
        {
            TextureSampleCount sampleCount = (TextureSampleCount)msaaOption;
            _newSampleCount = sampleCount;
        }

        internal void RefreshDeviceObjects(int numTimes)
        {
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < numTimes; i++)
            {
                DestroyAllObjects();
                CreateAllObjects();
            }
            sw.Stop();
            Console.WriteLine($"Refreshing resources {numTimes} times took {sw.Elapsed.TotalSeconds} seconds.");
        }

        void ToggleFullscreenState()
        {
            bool isFullscreen = Window.WindowState == WindowState.BorderlessFullScreen;
            Window.WindowState = isFullscreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
        }

        void Draw()
        {
            Debug.Assert(Window.Exists);
            int width = Window.Width;
            int height = Window.Height;

            if (_windowResized)
            {
                _windowResized = false;

                GraphicsDevice.ResizeMainWindow((uint)width, (uint)height);
                _scene.Exchange.Raise(new WindowResizedEvent(width, height), this);
                CommandList cl = GraphicsDevice.ResourceFactory.CreateCommandList();
                cl.Begin();
                _sceneContext.RecreateWindowSizedResources(GraphicsDevice, cl);
                cl.End();
                GraphicsDevice.SubmitCommands(cl);
                cl.Dispose();
            }

            if (_newSampleCount != null)
            {
                _sceneContext.MainSceneSampleCount = _newSampleCount.Value;
                _newSampleCount = null;
                DestroyAllObjects();
                CreateAllObjects();
            }

            _frameCommands.Begin();
            _scene.RenderAllStages(GraphicsDevice, _frameCommands, _sceneContext);
            GraphicsDevice.SwapBuffers();
        }

        internal void ChangeBackend(GraphicsBackend backend) => ChangeBackend(backend, false);

        internal void ChangeBackend(GraphicsBackend backend, bool forceRecreateWindow)
        {
            if (GraphicsDevice != null)
            {
                DestroyAllObjects();
                GraphicsDevice.Dispose();
            }

            if (Window == null || _recreateWindow || forceRecreateWindow)
            {
                Window?.Close();

                WindowCreateInfo windowCI = new WindowCreateInfo
                {
                    X = Window?.X ?? 684,
                    Y = Window?.Y ?? 456,
                    WindowWidth = Window?.Width ?? 684,
                    WindowHeight = Window?.Height ?? 456,
                    WindowInitialState = Window?.WindowState ?? WindowState.Normal,
                    WindowTitle = "UAlbion"
                };

                Window = VeldridStartup.CreateWindow(ref windowCI);
                Window.BorderVisible = false;
                Window.Resized += () => _windowResized = true;
            }

            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(false, null, false,
                ResourceBindingModel.Improved, true, true, false, true)
            { Debug = true };

            GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, gdOptions, backend);

            if (_scene != null)
            {
                _scene.Camera.UpdateBackend(GraphicsDevice);
                CreateAllObjects();
            }
        }

        void CreateAllObjects()
        {
            _frameCommands = GraphicsDevice.ResourceFactory.CreateCommandList();
            _frameCommands.Name = "Frame Commands List";

            CommandList initCL = GraphicsDevice.ResourceFactory.CreateCommandList();
            initCL.Name = "Recreation Initialization Command List";
            initCL.Begin();
            _sceneContext.CreateDeviceObjects(GraphicsDevice, initCL, _sceneContext);
            CheckForErrors();
            _scene.CreateAllDeviceObjects(GraphicsDevice, initCL, _sceneContext);
            initCL.End();
            GraphicsDevice.SubmitCommands(initCL);
            initCL.Dispose();
        }

        void DestroyAllObjects()
        {
            GraphicsDevice.WaitForIdle();
            _frameCommands.Dispose();
            _sceneContext.DestroyDeviceObjects();
            _scene.DestroyAllDeviceObjects();
            StaticResourceCache.DestroyAllDeviceObjects();
            TextureManager.DestroyDeviceObjects();
            GraphicsDevice.WaitForIdle();
        }

        public void Dispose()
        {
            _igRenderable?.Dispose();
            _frameCommands?.Dispose();
            _fullScreenQuad?.Dispose();
            //_graphicsDevice?.Dispose();
        }

        [Conditional("DEBUG")]
        [DebuggerNonUserCode]
        public static void CheckForErrors()
        {
            /*
            if (_instance.GraphicsDevice.GetOpenGLInfo(out BackendInfoOpenGL opengl))
            {
                opengl.CheckForErrors();
            }
            */
        }
    }
}


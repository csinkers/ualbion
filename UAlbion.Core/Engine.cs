using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using UAlbion.Core.Objects;

namespace UAlbion.Core
{
    public class Engine : Component, IDisposable
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<Engine, ToggleFullscreenEvent>((x, _) => x.ToggleFullscreenState()),
            new Handler<Engine, LoadRenderDocEvent>((x, _) => { if (RenderDoc.Load(out _renderDoc)) { x.ChangeBackend(x._graphicsDevice.BackendType, forceRecreateWindow: true); } }),
            new Handler<Engine, ToggleResizableEvent>((x, _) => x._window.Resizable = !x._window.Resizable),
            new Handler<Engine, ToggleVisibleBorderEvent>((x, _) => x._window.BorderVisible = !x._window.BorderVisible)
        };

        static RenderDoc _renderDoc;

        public ITextureManager TextureManager { get; }
        readonly double _desiredFrameLengthSeconds = 1.0 / 60.0;
        readonly bool _limitFrameRate = true;
        readonly FrameTimeAverager _frameTimeAverager = new FrameTimeAverager(0.666);
        readonly FullScreenQuad _fullScreenQuad;
        readonly ScreenDuplicator _duplicator;
        readonly ImGuiRenderable _igRenderable;
        readonly SceneContext _sceneContext = new SceneContext();
        readonly DebugMenus _debugMenus;

        static Engine _instance;

        Sdl2Window _window;
        Scene _scene;
        GraphicsDevice _graphicsDevice;
        CommandList _frameCommands;
        TextureSampleCount? _newSampleCount;
        bool _windowResized;
        bool _recreateWindow = true;

        internal GraphicsDevice GraphicsDevice => _graphicsDevice;
        internal Sdl2Window Window => _window;
        internal RenderDoc RenderDoc => _renderDoc;
        internal string FrameTimeText => _frameTimeAverager.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _frameTimeAverager.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms");

        public Scene Create2DScene()
        {
            var camera = new OrthographicCamera(_window);
            var scene = new Scene(camera);
            scene.AddRenderer(_igRenderable);
            scene.AddRenderer(_duplicator);
            scene.AddRenderer(_fullScreenQuad);

            scene.AddComponent(this);
            scene.AddComponent(_igRenderable);
            scene.AddComponent(_duplicator);
            scene.AddComponent(_fullScreenQuad);
            scene.AddComponent(_debugMenus);
            return scene;
        }

        public Scene Create3DScene()
        {
            var camera = new PerspectiveCamera(_graphicsDevice, _window);
            var scene = new Scene(camera);
            scene.AddRenderer(_igRenderable);
            scene.AddRenderer(_duplicator);
            scene.AddRenderer(_fullScreenQuad);

            scene.AddComponent(this);
            scene.AddComponent(_igRenderable);
            scene.AddComponent(_duplicator);
            scene.AddComponent(_fullScreenQuad);
            scene.AddComponent(_debugMenus);
            return scene;
        }

        public void SetScene(Scene scene)
        {
            _scene = scene;
            Engine.CheckForErrors();
            _sceneContext.SetCurrentScene(_scene);
            Engine.CheckForErrors();
            CreateAllObjects();
            Engine.CheckForErrors();
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
                GraphicsBackend.OpenGL
                //GraphicsBackend.OpenGLES
                //GraphicsBackend.Direct3D11
                );


            _igRenderable = new ImGuiRenderable(_window.Width, _window.Height);
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
            while (_window.Exists)
            {
                long currentFrameTicks = sw.ElapsedTicks;
                double deltaSeconds = (currentFrameTicks - previousFrameTicks) / (double)Stopwatch.Frequency;

                while (_limitFrameRate && deltaSeconds < _desiredFrameLengthSeconds)
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
                snapshot = _window.PumpEvents();
                InputTracker.UpdateFrameInput(snapshot, _window);
                Update((float)deltaSeconds);
                if (!_window.Exists)
                    break;

                Draw();
            }

            DestroyAllObjects();
            _graphicsDevice.Dispose();
        }

        void Update(float deltaSeconds)
        {
            _frameTimeAverager.AddTime(deltaSeconds);
            _scene.Exchange.Raise(new EngineUpdateEvent(deltaSeconds), this);
            _window.Title = _graphicsDevice.BackendType.ToString();
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
            Engine.CheckForErrors();
                CreateAllObjects();
            Engine.CheckForErrors();
            }
            sw.Stop();
            Console.WriteLine($"Refreshing resources {numTimes} times took {sw.Elapsed.TotalSeconds} seconds.");
        }

        void ToggleFullscreenState()
        {
            bool isFullscreen = _window.WindowState == WindowState.BorderlessFullScreen;
            _window.WindowState = isFullscreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
        }

        void Draw()
        {
            Debug.Assert(_window.Exists);
            int width = _window.Width;
            int height = _window.Height;

            if (_windowResized)
            {
                _windowResized = false;

                _graphicsDevice.ResizeMainWindow((uint)width, (uint)height);
                _scene.Exchange.Raise(new WindowResizedEvent(width, height), this);
                CommandList cl = _graphicsDevice.ResourceFactory.CreateCommandList();
                cl.Begin();
                _sceneContext.RecreateWindowSizedResources(_graphicsDevice, cl);
                cl.End();
                _graphicsDevice.SubmitCommands(cl);
                cl.Dispose();
            }

            if (_newSampleCount != null)
            {
                _sceneContext.MainSceneSampleCount = _newSampleCount.Value;
                _newSampleCount = null;
                DestroyAllObjects();
            Engine.CheckForErrors();
                CreateAllObjects();
            Engine.CheckForErrors();
            }

            _frameCommands.Begin();
            _scene.RenderAllStages(_graphicsDevice, _frameCommands, _sceneContext);
            _graphicsDevice.SwapBuffers();
        }

        internal void ChangeBackend(GraphicsBackend backend) => ChangeBackend(backend, false);

        internal void ChangeBackend(GraphicsBackend backend, bool forceRecreateWindow)
        {
            if (_graphicsDevice != null)
            {
                DestroyAllObjects();
                _graphicsDevice.Dispose();
            }

            if (_window == null || _recreateWindow || forceRecreateWindow)
            {
                _window?.Close();

                WindowCreateInfo windowCI = new WindowCreateInfo
                {
                    X = _window?.X ?? 50,
                    Y = _window?.Y ?? 50,
                    WindowWidth = _window?.Width ?? 960,
                    WindowHeight = _window?.Height ?? 540,
                    WindowInitialState = _window?.WindowState ?? WindowState.Normal,
                    WindowTitle = "UAlbion"
                };

                _window = VeldridStartup.CreateWindow(ref windowCI);
                _window.Resized += () => _windowResized = true;
            }

            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(false, null, false,
                ResourceBindingModel.Improved, true, true, false, true)
            { Debug = true };

            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, gdOptions, backend);

            if (_scene != null)
            {
                _scene.Camera.UpdateBackend(_graphicsDevice);
                CreateAllObjects();
            }
        }

        void CreateAllObjects()
        {
            _frameCommands = _graphicsDevice.ResourceFactory.CreateCommandList();
            _frameCommands.Name = "Frame Commands List";

            CommandList initCL = _graphicsDevice.ResourceFactory.CreateCommandList();
            initCL.Name = "Recreation Initialization Command List";
            initCL.Begin();
            _sceneContext.CreateDeviceObjects(_graphicsDevice, initCL, _sceneContext);
            CheckForErrors();
            _scene.CreateAllDeviceObjects(_graphicsDevice, initCL, _sceneContext);
            initCL.End();
            _graphicsDevice.SubmitCommands(initCL);
            initCL.Dispose();
        }

        void DestroyAllObjects()
        {
            _graphicsDevice.WaitForIdle();
            _frameCommands.Dispose();
            _sceneContext.DestroyDeviceObjects();
            _scene.DestroyAllDeviceObjects();
            StaticResourceCache.DestroyAllDeviceObjects();
            TextureManager.DestroyDeviceObjects();
            _graphicsDevice.WaitForIdle();
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
            if (_instance.GraphicsDevice.GetOpenGLInfo(out BackendInfoOpenGL opengl))
            {
                opengl.CheckForErrors();
            }
        }
    }
}


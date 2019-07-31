using System;
using System.Diagnostics;
using System.Threading;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using UAlbion.Core.Objects;

namespace UAlbion.Core
{
    public class Engine : IDisposable
    {
        public ITextureManager TextureManager { get; }

        readonly string[] _msaaOptions = { "Off", "2x", "4x", "8x", "16x", "32x" };

        static readonly double s_desiredFrameLengthSeconds = 1.0 / 60.0;
        static readonly bool s_limitFrameRate = true;
        static readonly FrameTimeAverager s_frameTimeAverager = new FrameTimeAverager(0.666);
        static RenderDoc _renderDoc;

        readonly FullScreenQuad _fullScreenQuad;
        readonly ScreenDuplicator _duplicator;
        readonly ImGuiRenderable _igRenderable;
        readonly SceneContext _sceneContext = new SceneContext();

        Sdl2Window _window;
        Scene _scene;
        GraphicsDevice _graphicsDevice;
        CommandList _frameCommands;
        TextureSampleCount? _newSampleCount;
        bool _windowResized;
        bool _recreateWindow = true;
        int _msaaOption = 0;

        public Scene Create2DScene()
        {
            var camera = new OrthographicCamera(_window);
            var scene = new Scene(camera);
            scene.AddRenderer(_igRenderable);
            scene.AddRenderer(_duplicator);
            scene.AddRenderer(_fullScreenQuad);

            scene.AddComponent(_igRenderable);
            scene.AddComponent(_duplicator);
            scene.AddComponent(_fullScreenQuad);
            return scene;
        }

        public Scene Create3DScene()
        {
            var camera = new PerspectiveCamera(_graphicsDevice, _window);
            var scene = new Scene(camera);
            scene.AddRenderer(_igRenderable);
            scene.AddRenderer(_duplicator);
            scene.AddRenderer(_fullScreenQuad);

            scene.AddComponent(_igRenderable);
            scene.AddComponent(_duplicator);
            scene.AddComponent(_fullScreenQuad);
            return scene;
        }

        public void SetScene(Scene scene)
        {
            _scene = scene;
            _sceneContext.SetCurrentScene(_scene);
            CreateAllObjects();
            ImGui.StyleColorsClassic();
        }

        public Engine()
        {
            TextureManager = new TextureManager();
            WindowCreateInfo windowCi = new WindowCreateInfo
            {
                X = 50,
                Y = 50,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowInitialState = WindowState.Normal,
                WindowTitle = "UAlbion"
            };
            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(false, null, false, ResourceBindingModel.Improved, true, true, false);
#if DEBUG
            gdOptions.Debug = true;
#endif
            VeldridStartup.CreateWindowAndGraphicsDevice(
                windowCi,
                gdOptions,
                //VeldridStartup.GetPlatformDefaultBackend(),
                //GraphicsBackend.Metal,
                //GraphicsBackend.Vulkan,
                GraphicsBackend.OpenGL,
                //GraphicsBackend.OpenGLES,
                //GraphicsBackend.Direct3D11,
                out _window,
                out _graphicsDevice);
            _window.Resized += () => _windowResized = true;

            _igRenderable = new ImGuiRenderable(_window.Width, _window.Height);
            _duplicator = new ScreenDuplicator();
            _fullScreenQuad = new FullScreenQuad();

            Sdl2Native.SDL_Init(SDLInitFlags.GameController);
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

                while (s_limitFrameRate && deltaSeconds < s_desiredFrameLengthSeconds)
                {
                    var millisecondsToSleep = (int)((s_desiredFrameLengthSeconds - deltaSeconds) * 1000);
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
                {
                    break;
                }

                Draw();
            }

            DestroyAllObjects();
            _graphicsDevice.Dispose();
        }

        void Update(float deltaSeconds)
        {
            s_frameTimeAverager.AddTime(deltaSeconds);
            _scene.Exchange.Raise(new EngineUpdateEvent(deltaSeconds), this);
            RenderDebugMenu();
/*
            if (InputTracker.GetKeyDown(Key.F11))
            {
                ToggleFullscreenState();
            }
            if (InputTracker.GetKeyDown(Key.Keypad6))
            {
                _window.X += 10;
            }
            if (InputTracker.GetKeyDown(Key.Keypad4))
            {
                _window.X -= 10;
            }
            if (InputTracker.GetKeyDown(Key.Keypad8))
            {
                _window.Y += 10;
            }
            if (InputTracker.GetKeyDown(Key.Keypad2))
            {
                _window.Y -= 10;
            }
*/
            _window.Title = _graphicsDevice.BackendType.ToString();
        }

        void RenderDebugMenu()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("Settings"))
                {
                    if (ImGui.BeginMenu("Graphics Backend"))
                    {

                        if (ImGui.MenuItem("Vulkan", string.Empty, _graphicsDevice.BackendType == GraphicsBackend.Vulkan, GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan)))
                        {
                            ChangeBackend(GraphicsBackend.Vulkan);
                        }
                        if (ImGui.MenuItem("OpenGL", string.Empty, _graphicsDevice.BackendType == GraphicsBackend.OpenGL, GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGL)))
                        {
                            ChangeBackend(GraphicsBackend.OpenGL);
                        }
                        if (ImGui.MenuItem("OpenGL ES", string.Empty, _graphicsDevice.BackendType == GraphicsBackend.OpenGLES, GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGLES)))
                        {
                            ChangeBackend(GraphicsBackend.OpenGLES);
                        }
                        if (ImGui.MenuItem("Direct3D 11", string.Empty, _graphicsDevice.BackendType == GraphicsBackend.Direct3D11, GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11)))
                        {
                            ChangeBackend(GraphicsBackend.Direct3D11);
                        }
                        if (ImGui.MenuItem("Metal", string.Empty, _graphicsDevice.BackendType == GraphicsBackend.Metal, GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal)))
                        {
                            ChangeBackend(GraphicsBackend.Metal);
                        }
                        ImGui.EndMenu();
                    }
                    if (ImGui.BeginMenu("MSAA"))
                    {
                        if (ImGui.Combo("MSAA", ref _msaaOption, _msaaOptions, _msaaOptions.Length))
                        {
                            ChangeMsaa(_msaaOption);
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Window"))
                {
                    bool isFullscreen = _window.WindowState == WindowState.BorderlessFullScreen;
                    if (ImGui.MenuItem("Fullscreen", "F11", isFullscreen, true))
                    {
                        ToggleFullscreenState();
                    }
                    if (ImGui.MenuItem("Always Recreate Sdl2Window", string.Empty, _recreateWindow, true))
                    {
                        _recreateWindow = !_recreateWindow;
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(
                            "Causes a new OS window to be created whenever the graphics backend is switched. This is much safer, and is the default.");
                    }

                    bool vsync = _graphicsDevice.SyncToVerticalBlank;
                    if (ImGui.MenuItem("VSync", string.Empty, vsync, true))
                    {
                        _graphicsDevice.SyncToVerticalBlank = !_graphicsDevice.SyncToVerticalBlank;
                    }
                    bool resizable = _window.Resizable;
                    if (ImGui.MenuItem("Resizable Window", string.Empty, resizable))
                    {
                        _window.Resizable = !_window.Resizable;
                    }
                    bool bordered = _window.BorderVisible;
                    if (ImGui.MenuItem("Visible Window Border", string.Empty, bordered))
                    {
                        _window.BorderVisible = !_window.BorderVisible;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Debug"))
                {
                    if (ImGui.MenuItem("Refresh Device Objects"))
                    {
                        RefreshDeviceObjects(1);
                    }
                    if (ImGui.MenuItem("Refresh Device Objects (10 times)"))
                    {
                        RefreshDeviceObjects(10);
                    }
                    if (ImGui.MenuItem("Refresh Device Objects (100 times)"))
                    {
                        RefreshDeviceObjects(100);
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("RenderDoc"))
                {
                    if (_renderDoc == null)
                    {
                        if (ImGui.MenuItem("Load"))
                        {
                            if (RenderDoc.Load(out _renderDoc))
                            {
                                ChangeBackend(_graphicsDevice.BackendType, forceRecreateWindow: true);
                            }
                        }
                    }
                    else
                    {
                        if (ImGui.MenuItem("Trigger Capture"))
                        {
                            _renderDoc.TriggerCapture();
                        }
                        if (ImGui.BeginMenu("Options"))
                        {
                            bool allowVsync = _renderDoc.AllowVSync;
                            if (ImGui.Checkbox("Allow VSync", ref allowVsync))
                            {
                                _renderDoc.AllowVSync = allowVsync;
                            }
                            bool validation = _renderDoc.APIValidation;
                            if (ImGui.Checkbox("API Validation", ref validation))
                            {
                                _renderDoc.APIValidation = validation;
                            }
                            int delayForDebugger = (int)_renderDoc.DelayForDebugger;
                            if (ImGui.InputInt("Debugger Delay", ref delayForDebugger))
                            {
                                delayForDebugger = Math.Clamp(delayForDebugger, 0, int.MaxValue);
                                _renderDoc.DelayForDebugger = (uint)delayForDebugger;
                            }
                            bool verifyBufferAccess = _renderDoc.VerifyBufferAccess;
                            if (ImGui.Checkbox("Verify Buffer Access", ref verifyBufferAccess))
                            {
                                _renderDoc.VerifyBufferAccess = verifyBufferAccess;
                            }
                            bool overlayEnabled = _renderDoc.OverlayEnabled;
                            if (ImGui.Checkbox("Overlay Visible", ref overlayEnabled))
                            {
                                _renderDoc.OverlayEnabled = overlayEnabled;
                            }
                            bool overlayFrameRate = _renderDoc.OverlayFrameRate;
                            if (ImGui.Checkbox("Overlay Frame Rate", ref overlayFrameRate))
                            {
                                _renderDoc.OverlayFrameRate = overlayFrameRate;
                            }
                            bool overlayFrameNumber = _renderDoc.OverlayFrameNumber;
                            if (ImGui.Checkbox("Overlay Frame Number", ref overlayFrameNumber))
                            {
                                _renderDoc.OverlayFrameNumber = overlayFrameNumber;
                            }
                            bool overlayCaptureList = _renderDoc.OverlayCaptureList;
                            if (ImGui.Checkbox("Overlay Capture List", ref overlayCaptureList))
                            {
                                _renderDoc.OverlayCaptureList = overlayCaptureList;
                            }
                            ImGui.EndMenu();
                        }
                        if (ImGui.MenuItem("Launch Replay UI"))
                        {
                            _renderDoc.LaunchReplayUI();
                        }
                    }
                    ImGui.EndMenu();
                }

                ImGui.Text(s_frameTimeAverager.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + s_frameTimeAverager.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms"));

                ImGui.EndMainMenuBar();
            }
        }

        void ChangeMsaa(int msaaOption)
        {
            TextureSampleCount sampleCount = (TextureSampleCount)msaaOption;
            _newSampleCount = sampleCount;
        }

        void RefreshDeviceObjects(int numTimes)
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
                CreateAllObjects();
            }

            _frameCommands.Begin();

            _scene.RenderAllStages(_graphicsDevice, _frameCommands, _sceneContext);
            _graphicsDevice.SwapBuffers();
        }

        void ChangeBackend(GraphicsBackend backend) => ChangeBackend(backend, false);

        void ChangeBackend(GraphicsBackend backend, bool forceRecreateWindow)
        {
            DestroyAllObjects();
            bool syncToVBlank = _graphicsDevice.SyncToVerticalBlank;
            _graphicsDevice.Dispose();

            if (_recreateWindow || forceRecreateWindow)
            {
                WindowCreateInfo windowCI = new WindowCreateInfo
                {
                    X = _window.X,
                    Y = _window.Y,
                    WindowWidth = _window.Width,
                    WindowHeight = _window.Height,
                    WindowInitialState = _window.WindowState,
                    WindowTitle = "UAlbion"
                };

                _window.Close();

                _window = VeldridStartup.CreateWindow(ref windowCI);
                _window.Resized += () => _windowResized = true;
            }

            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(false, null, syncToVBlank, ResourceBindingModel.Improved, true, true, false);
#if DEBUG
            gdOptions.Debug = true;
#endif
            _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, gdOptions, backend);
            _scene.Camera.UpdateBackend(_graphicsDevice);
            CreateAllObjects();
        }

        void CreateAllObjects()
        {
            _frameCommands = _graphicsDevice.ResourceFactory.CreateCommandList();
            _frameCommands.Name = "Frame Commands List";
            CommandList initCL = _graphicsDevice.ResourceFactory.CreateCommandList();
            initCL.Name = "Recreation Initialization Command List";
            initCL.Begin();
            _sceneContext.CreateDeviceObjects(_graphicsDevice, initCL, _sceneContext);
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
            _graphicsDevice.WaitForIdle();
        }

        public void Dispose()
        {
            _igRenderable?.Dispose();
            _frameCommands?.Dispose();
            _fullScreenQuad?.Dispose();
            //_graphicsDevice?.Dispose();
        }
    }
}

/*
        const string FragmentCode = @"
#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 0) out vec4 fsout_color;

layout(set = 0, binding = 0) uniform texture2D Palette;
layout(set = 0, binding = 1) uniform sampler PaletteSampler;

layout(set = 1, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 1, binding = 1) uniform sampler SurfaceSampler;

void main()
{
    int index = int(255.0 * texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords * vec2(640, 480))[0]);
    vec4 color = texture(sampler2D(Palette, PaletteSampler), vec2(fsin_texCoords[0], 0));
    fsout_color = color;
    // fsout_color = vec4(index / 256.0, index / 256.0, index / 256.0, 1.0);
    // fsout_color = texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords);//color;
    // fsout_color = vec4(fsin_texCoords[0], fsin_texCoords[1], 0.0, 1.0);
}";
    }
*/

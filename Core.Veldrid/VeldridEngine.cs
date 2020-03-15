using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace UAlbion.Core.Veldrid
{
    public class VeldridEngine : Engine, IDisposable
    {
        const int DefaultWidth = 720;
        const int DefaultHeight = 480;

        static readonly HandlerSet Handlers = new HandlerSet
        (
            H<VeldridEngine, LoadRenderDocEvent>((x, _) =>
            {
                if (_renderDoc == null && RenderDoc.Load(out _renderDoc))
                {
                    x._newBackend = x.GraphicsDevice.BackendType;
                    x._recreateWindow = true;
                }
            }),
            H<VeldridEngine, GarbageCollectionEvent>((x, _) => GC.Collect()),
            H<VeldridEngine, QuitEvent>((x, e) => x._done = true),
            H<VeldridEngine, RunRenderDocEvent>((x, _) => _renderDoc?.LaunchReplayUI()),
            H<VeldridEngine, SetCursorPositionEvent>((x, e) => x._pendingCursorUpdate = new Vector2(e.X, e.Y)),
            H<VeldridEngine, ToggleFullscreenEvent>((x, _) => x.ToggleFullscreenState()),
            H<VeldridEngine, ToggleHardwareCursorEvent>((x, _) => x._window.CursorVisible = !x._window.CursorVisible),
            H<VeldridEngine, ToggleResizableEvent>((x, _) => x._window.Resizable = !x._window.Resizable),
            H<VeldridEngine, ToggleVisibleBorderEvent>((x, _) => x._window.BorderVisible = !x._window.BorderVisible),
            H<VeldridEngine, SetMsaaLevelEvent>((x, e) => x._newSampleCount = e.SampleCount switch
            {
                1 => TextureSampleCount.Count1,
                2 => TextureSampleCount.Count2,
                4 => TextureSampleCount.Count4,
                8 => TextureSampleCount.Count8,
                16 => TextureSampleCount.Count16,
                32 => TextureSampleCount.Count32,
                _ => throw new InvalidOperationException($"Invalid sample count {e.SampleCount}")
            }),
            H<VeldridEngine, RefreshDeviceObjectsEvent>((x, e) => x.RefreshDeviceObjects(e.Count ?? 1)),
            H<VeldridEngine, RecreateWindowEvent>((x, e) => { x._recreateWindow = true; x._newBackend = x.GraphicsDevice.BackendType; }),
            H<VeldridEngine, SetBackendEvent>((x, e) => x._newBackend = e.Value),
            H<VeldridEngine, SetVSyncEvent>((x, e) =>
            {
                if (x._vsync == e.Value) return;
                x._vsync = e.Value;
                x._newBackend = x.GraphicsDevice.BackendType;
            })
        );

        static RenderDoc _renderDoc;
        Sdl2Window _window;

        readonly IList<IRenderer> _renderers = new List<IRenderer>();
        readonly FrameTimeAverager _frameTimeAverager = new FrameTimeAverager(0.5);
        readonly SceneContext _sceneContext = new SceneContext();
        readonly WindowManager _windowManager = new WindowManager();
        readonly VeldridCoreFactory _coreFactory;

        CommandList _frameCommands;
        TextureSampleCount? _newSampleCount;
        bool _windowResized;
        bool _done;
        bool _recreateWindow;
        bool _vsync = true;
        Vector2? _pendingCursorUpdate;
        GraphicsBackend? _newBackend;

        internal GraphicsDevice GraphicsDevice { get; private set; }
        internal RenderDoc RenderDoc => _renderDoc;

        public override ICoreFactory Factory { get; } = new VeldridCoreFactory();

        public override string FrameTimeText => _frameTimeAverager.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") +
                                                _frameTimeAverager.CurrentAverageFrameTimeMilliseconds.ToString("#00.00 ms");

        public VeldridEngine(GraphicsBackend backend, bool useRenderDoc) : base(Handlers)
        {
            _coreFactory = new VeldridCoreFactory();
            _newBackend = backend;
            if (useRenderDoc)
                using(PerfTracker.InfrequentEvent("Loading renderdoc"))
                    RenderDoc.Load(out _renderDoc);
        }

        public override void Subscribed()
        {
            var shaderCache = Resolve<IShaderCache>();
            if(shaderCache == null)
                throw new InvalidOperationException("An instance of IShaderCache must be registered.");
            shaderCache.ShadersUpdated += (sender, args) => _newBackend = GraphicsDevice?.BackendType;

            Exchange
                .Register<IWindowManager>(_windowManager)
                //.Attach(new DebugMenus(this))
                ;
        }

        public VeldridEngine AddRenderer(IRenderer renderer)
        {
            _renderers.Add(renderer);
            if (renderer is IComponent component)
                AttachChild(component);

            return this;
        }

        public void Initialise()
        {
            ChangeBackend();
        }

        public void Run()
        {
            ChangeBackend();
            PerfTracker.StartupEvent("Set up backend");
            Sdl2Native.SDL_Init(SDLInitFlags.GameController);
            ImGui.StyleColorsClassic();
            Raise(new WindowResizedEvent(_window.Width, _window.Height));
            Raise(new BeginFrameEvent());

            var frameCounter = new FrameCounter();
            PerfTracker.StartupEvent("Startup done, rendering first frame");
            while (!_done)
            {
                ChangeBackend();

                PerfTracker.BeginFrame();
                double deltaSeconds = frameCounter.StartFrame();
                using(PerfTracker.FrameEvent("1 Raising begin frame"))
                    Raise(new BeginFrameEvent());

                InputSnapshot snapshot;
                using (PerfTracker.FrameEvent("2 Processing SDL events"))
                {
                    Sdl2Events.ProcessEvents();
                    snapshot = _window.PumpEvents();
                }

                if (!_window.Exists)
                    break;

                if (_pendingCursorUpdate.HasValue)
                {
                    using (PerfTracker.FrameEvent("3 Warping mouse"))
                    {
                        Sdl2Native.SDL_WarpMouseInWindow(
                            _window.SdlWindowHandle,
                            (int) _pendingCursorUpdate.Value.X,
                            (int) _pendingCursorUpdate.Value.Y);

                        _pendingCursorUpdate = null;
                    }
                }

                using (PerfTracker.FrameEvent("4 Raising input event"))
                    Raise(new InputEvent(deltaSeconds, snapshot, _window.MouseDelta));

                using (PerfTracker.FrameEvent("5 Performing update"))
                    Update((float)deltaSeconds);

                using (PerfTracker.FrameEvent("5.1 Flushing queued events"))
                    Exchange.FlushQueuedEvents();

                if (!_window.Exists)
                    break;

                using (PerfTracker.FrameEvent("6 Drawing"))
                    Draw();

                var flags = Resolve<IEngineSettings>().Flags;
                if (GraphicsDevice.SyncToVerticalBlank != ((flags & EngineFlags.VSync) != 0))
                    GraphicsDevice.SyncToVerticalBlank = (flags & EngineFlags.VSync) != 0;

                using (PerfTracker.FrameEvent("7 Swap buffers"))
                {
                    CoreTrace.Log.Info("Engine", "Swapping buffers...");
                    GraphicsDevice.SwapBuffers();
                    CoreTrace.Log.Info("Engine", "Draw complete");
                }
            }

            DestroyAllObjects();
            GraphicsDevice.Dispose();
            _window.Close();
        }

        void Update(float deltaSeconds)
        {
            _frameTimeAverager.AddTime(deltaSeconds);
            Raise(new EngineUpdateEvent(deltaSeconds));
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
            bool isFullscreen = _window.WindowState == WindowState.BorderlessFullScreen;
            _window.WindowState = isFullscreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
        }

        void Draw()
        {
            ApiUtil.Assert(_window.Exists);
            int width = _window.Width;
            int height = _window.Height;

            CoreTrace.Log.Info("Engine", "Start draw");
            if (_windowResized)
            {
                _windowResized = false;

                GraphicsDevice.ResizeMainWindow((uint)width, (uint)height);
                Raise(new WindowResizedEvent(width, height));
                _sceneContext.RecreateWindowSizedResources(GraphicsDevice);
                CoreTrace.Log.Info("Engine", "Resize finished");
            }

            if (_newSampleCount != null)
            {
                _sceneContext.MainSceneSampleCount = _newSampleCount.Value;
                _newSampleCount = null;
                DestroyAllObjects();
                CreateAllObjects();
            }

            using (PerfTracker.FrameEvent("6.1 Render scenes"))
            {
                _frameCommands.Begin();

                var scenes = new List<Scene>();
                Raise(new CollectScenesEvent(scenes.Add));

                var context = new VeldridRendererContext(GraphicsDevice, _frameCommands, _sceneContext, _coreFactory);
                foreach (var scene in scenes)
                    scene.RenderAllStages(context, _renderers);

                _frameCommands.End();
            }

            using (PerfTracker.FrameEvent("6.2 Submit commandlist"))
            {
                CoreTrace.Log.Info("Scene", "Submitting Commands");
                GraphicsDevice.SubmitCommands(_frameCommands);
                CoreTrace.Log.Info("Scene", "Submitted commands");
                GraphicsDevice.WaitForIdle();
            }
        }

        void ChangeBackend()
        {
            if (_newBackend == null) return;
            var backend = _newBackend.Value;
            _newBackend = null;

            using (PerfTracker.InfrequentEvent($"change backend to {backend}"))
            {
                bool firstCreate = GraphicsDevice == null;
                if (GraphicsDevice != null)
                {
                    DestroyAllObjects();
                    if(GraphicsDevice.BackendType != backend)
                        Resolve<IShaderCache>().DestroyAllDeviceObjects();
                    GraphicsDevice.Dispose();
                }

                if (_window == null || _recreateWindow)
                {
                    _recreateWindow = false;
                    _window?.Close();

                    WindowCreateInfo windowInfo = new WindowCreateInfo
                    {
                        X = _window?.X ?? 648,
                        Y = _window?.Y ?? 431,
                        WindowWidth = _window?.Width ?? DefaultWidth,
                        WindowHeight = _window?.Height ?? DefaultHeight,
                        WindowInitialState = _window?.WindowState ?? WindowState.Normal,
                        WindowTitle = "UAlbion"
                    };

                    _window = VeldridStartup.CreateWindow(ref windowInfo);
                    _windowManager.Window = new VeldridSdlWindow(_window);
                    //Window.BorderVisible = false;
                    _window.CursorVisible = false;
                    _window.Resized += () => _windowResized = true;
                }

                GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(
                    _renderDoc != null, PixelFormat.R32_Float, false,
                    ResourceBindingModel.Improved, true,
                    true, false)
                {
                    SyncToVerticalBlank = _vsync,
                };

                // Currently this field only exists in my local build of veldrid, so set it via reflection.
                var singleThreadedProperty = typeof(GraphicsDeviceOptions).GetField("SingleThreaded");
                if(singleThreadedProperty != null)
                    singleThreadedProperty.SetValueDirect(__makeref(gdOptions), true);

                GraphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, gdOptions, backend);
                GraphicsDevice.WaitForIdle();
                _window.Title = GraphicsDevice.BackendType.ToString();

                Raise(new BackendChangedEvent(GraphicsDevice.IsDepthRangeZeroToOne, GraphicsDevice.IsClipSpaceYInverted));
                CreateAllObjects();

                if (!firstCreate)
                    Raise(new EngineFlagEvent(FlagOperation.Toggle, EngineFlags.FlipDepthRange));
            }
        }

        void CreateAllObjects()
        {
            using(PerfTracker.InfrequentEvent("Create objects"))
            {
                _frameCommands = GraphicsDevice.ResourceFactory.CreateCommandList();
                _frameCommands.Name = "Frame Commands List";

                CommandList initList = GraphicsDevice.ResourceFactory.CreateCommandList();
                initList.Name = "Recreation Initialization Command List";
                initList.Begin();
                _sceneContext.CreateDeviceObjects(GraphicsDevice, initList);

                var context = new VeldridRendererContext(GraphicsDevice, initList, _sceneContext, _coreFactory);
                foreach (var r in _renderers)
                    r.CreateDeviceObjects(context);

                initList.End();
                GraphicsDevice.SubmitCommands(initList);
                GraphicsDevice.WaitForIdle();
                initList.Dispose();
                GraphicsDevice.WaitForIdle();
                Resolve<IShaderCache>().CleanupOldFiles();
            }
        }

        void DestroyAllObjects()
        {
            using (PerfTracker.InfrequentEvent("Destroying objects"))
            {
                GraphicsDevice.WaitForIdle();
                _frameCommands.Dispose();
                _sceneContext.DestroyDeviceObjects();
                foreach (var r in _renderers)
                    r.DestroyDeviceObjects();

                Resolve<ITextureManager>()?.DestroyDeviceObjects();
                Resolve<IDeviceObjectManager>()?.DestroyDeviceObjects();
                GraphicsDevice.WaitForIdle();
            }
        }

        public void Dispose()
        {
            _frameCommands?.Dispose();
            foreach(var renderer in _renderers)
                if (renderer is IDisposable disposable)
                    disposable.Dispose();
        }
    }
}


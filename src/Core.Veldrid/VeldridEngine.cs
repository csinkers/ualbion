using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Veldrid.Visual;
using UAlbion.Core.Visual;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using PixelFormat = Veldrid.PixelFormat;

#pragma warning disable CA2213
namespace UAlbion.Core.Veldrid
{
    public sealed class VeldridEngine : Engine, IDisposable
    {
        static RenderDoc _renderDoc;
        Sdl2Window _window;

        readonly FrameTimeAverager _frameTimeAverager = new FrameTimeAverager(0.5);
        readonly WindowManager _windowManager;
        readonly VeldridCoreFactory _coreFactory;
        readonly IDictionary<Type, IRenderer> _renderers = new Dictionary<Type, IRenderer>();
        readonly IDictionary<IRenderer, List<IRenderable>> _renderables = new Dictionary<IRenderer, List<IRenderable>>();
        readonly bool _startupOnly;
        readonly bool _useRenderDoc;
        readonly int _defaultWidth = 720;
        readonly int _defaultHeight = 480;
        readonly int _defaultX = 648;
        readonly int _defaultY = 431;
        readonly bool _showWindow;

        SceneContext _sceneContext;
        CommandList _frameCommands;
        bool _windowResized;
        bool _done;
        bool _recreateWindow;
        bool _objectsCreated;
        bool _vsync = true;
        Vector2? _pendingCursorUpdate;
        GraphicsBackend? _newBackend;
        DateTime _lastTitleUpdateTime;
        int _frameCounter;

        internal GraphicsDevice GraphicsDevice { get; private set; }
        internal static RenderDoc RenderDoc => _renderDoc;

        public string WindowTitle { get; set; }

        public override ICoreFactory Factory { get; } = new VeldridCoreFactory();

        public override string FrameTimeText =>
            $"{_frameTimeAverager.CurrentAverageFramesPerSecond:000.0 fps} / {_frameTimeAverager.CurrentAverageFrameTimeMilliseconds:#00.00 ms}";

        public override bool IsDepthRangeZeroToOne => GraphicsDevice?.IsDepthRangeZeroToOne ?? false;
        public override bool IsClipSpaceYInverted => GraphicsDevice?.IsClipSpaceYInverted ?? false;

        public VeldridEngine(GraphicsBackend backend, bool useRenderDoc, bool startupOnly, bool showWindow, Rectangle? windowRect = null)
        {
            On<LoadRenderDocEvent>(e =>
            {
                if (_renderDoc != null || !RenderDoc.Load(out _renderDoc)) return;
                _newBackend = GraphicsDevice.BackendType;
                _recreateWindow = true;
            });
            On<GarbageCollectionEvent>(e => GC.Collect());
            On<QuitEvent>(e => _done = true);
            On<RunRenderDocEvent>(e => _renderDoc?.LaunchReplayUI());
            On<SetCursorPositionEvent>(e => _pendingCursorUpdate = new Vector2(e.X, e.Y));
            On<ToggleFullscreenEvent>(e => ToggleFullscreenState());
            On<ToggleHardwareCursorEvent>(e => { if (_window != null) _window.CursorVisible = !_window.CursorVisible; });
            On<ToggleResizableEvent>(e => { if (_window != null) _window.Resizable = !_window.Resizable; });
            On<ToggleVisibleBorderEvent>(e => { if (_window != null) _window.BorderVisible = !_window.BorderVisible; });
            On<RecreateWindowEvent>(e => { _recreateWindow = true; _newBackend = GraphicsDevice.BackendType; });
            On<SetBackendEvent>(e => _newBackend = e.Value);
            On<TriggerRenderDocEvent>(e => _renderDoc?.TriggerCapture());
            On<ConfineMouseToWindowEvent>(e => { if (_window != null) Sdl2Native.SDL_SetWindowGrab(_window.SdlWindowHandle, e.Enabled); });
            On<SetRelativeMouseModeEvent>(e =>
            {
                if (_window == null) return;
                Sdl2Native.SDL_SetRelativeMouseMode(e.Enabled);
                if (!e.Enabled)
                    Sdl2Native.SDL_WarpMouseInWindow(_window.SdlWindowHandle, _window.Width / 2, _window.Height / 2);
            });
            On<SetVSyncEvent>(e =>
            {
                if (_vsync == e.Value) return;
                _vsync = e.Value;
                _newBackend = GraphicsDevice.BackendType;
            });

            _coreFactory = new VeldridCoreFactory();
            _windowManager = AttachChild(new WindowManager());
            _newBackend = backend;
            _useRenderDoc = useRenderDoc;
            _startupOnly = startupOnly;
            _showWindow = showWindow;

            if (windowRect.HasValue)
            {
                _defaultX = windowRect.Value.X;
                _defaultY = windowRect.Value.Y;
                _defaultWidth = windowRect.Value.Width;
                _defaultHeight = windowRect.Value.Height;
            }
        }

        protected override void Subscribed()
        {
            var shaderCache = Resolve<IShaderCache>();
            if(shaderCache == null)
                throw new InvalidOperationException("An instance of IShaderCache must be registered.");
            shaderCache.ShadersUpdated += (sender, args) => _newBackend = GraphicsDevice?.BackendType;
            base.Subscribed();
        }

        public VeldridEngine AddRenderer(IRenderer renderer)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            foreach(var type in renderer.RenderableTypes)
                _renderers[type] = renderer;

            if (renderer is IComponent component)
                AttachChild(component);

            return this;
        }

        public override void RegisterRenderable(IRenderable renderable)
        {
            if (renderable == null) return;
            var type = renderable.GetType();
            if (!_renderers.TryGetValue(type, out var renderer))
                throw new InvalidOperationException($"Renderable of type {type.Name} is not handled by any registered renderer");

            if (!_renderables.TryGetValue(renderer, out var list))
            {
                list = new List<IRenderable>();
                _renderables[renderer] = list;
            }

            list.Add(renderable);
        }

        public override void UnregisterRenderable(IRenderable renderable)
        {
            if (renderable == null) return;
            var type = renderable.GetType();
            if (!_renderers.TryGetValue(type, out var renderer))
                throw new InvalidOperationException($"Renderable of type {type.Name} is not handled by any registered renderer");

            if (!_renderables.TryGetValue(renderer, out var list))
                return;

            list.Remove(renderable);
        }

        public override void Run()
        {
            if (!_showWindow)
                throw new InvalidOperationException("Cannot run as main game loop without a window");

            VeldridRendererContext context = BuildContext(null);
            PerfTracker.StartupEvent("Set up backend");
            Sdl2Native.SDL_Init(SDLInitFlags.GameController);

            if (ImGui.GetCurrentContext() != IntPtr.Zero)
            {
                ImGui.StyleColorsClassic();

                // Turn on ImGui docking if it's supported
                if (Enum.TryParse(typeof(ImGuiConfigFlags), "DockingEnable", out var dockingFlag))
                    ImGui.GetIO().ConfigFlags |= (ImGuiConfigFlags) dockingFlag;
            }

            Raise(new WindowResizedEvent(_window.Width, _window.Height));
            Raise(BeginFrameEvent.Instance);

            var frameCounter = new FrameCounter();
            PerfTracker.StartupEvent("Startup done, rendering first frame");
            while (!_done)
            {
                //if (_frameCounter < 3) _renderDoc.TriggerCapture(); // Capture first few frames for transient rendering issues
                if (!_objectsCreated)
                    context = BuildContext(null);

                PerfTracker.BeginFrame();
                double deltaSeconds = frameCounter.StartFrame();
                using(PerfTracker.FrameEvent("1 Raising begin frame"))
                    Raise(BeginFrameEvent.Instance);

                InputSnapshot snapshot;
                using (PerfTracker.FrameEvent("2 Processing SDL events"))
                {
                    Sdl2Events.ProcessEvents();
                    snapshot = _window.PumpEvents();
                }

                if (!_window.Exists)
                    break;

                SetTitle();
                if (_window != null)
                {
                    if (_pendingCursorUpdate.HasValue && _window.Focused)
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
                }

                using (PerfTracker.FrameEvent("5 Performing update"))
                    Update((float)deltaSeconds);

                using (PerfTracker.FrameEvent("5.1 Flushing queued events"))
                    Exchange.FlushQueuedEvents();

                using (PerfTracker.FrameEvent("5.2 Calculating UI layout"))
                    Raise(new LayoutEvent());

                if (!_window.Exists)
                    break;

                using (PerfTracker.FrameEvent("6 Drawing"))
                    Draw(context);

                var flags = Resolve<IEngineSettings>().Flags;
                if (GraphicsDevice.SyncToVerticalBlank != ((flags & EngineFlags.VSync) != 0))
                    GraphicsDevice.SyncToVerticalBlank = (flags & EngineFlags.VSync) != 0;

                using (PerfTracker.FrameEvent("7 Swap buffers"))
                {
                    CoreTrace.Log.Info("Engine", "Swapping buffers...");
                    GraphicsDevice.SwapBuffers();
                    CoreTrace.Log.Info("Engine", "Draw complete");
                }

                if (_startupOnly)
                    _done = true;

                _frameCounter++;
            }

            Resolve<IShaderCache>()?.CleanupOldFiles();
            DestroyAllObjects();
            GraphicsDevice.Dispose();
            _window.Close();
            GraphicsDevice = null;
            _window = null;
        }

        public unsafe Image<Bgra32> RenderFrame(VeldridRendererContext context, bool captureWithRenderDoc)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (captureWithRenderDoc)
                _renderDoc.TriggerCapture();

            Draw(context);

            var framebuffer = (FramebufferSource)context.Framebuffer ;
            var color = framebuffer.Framebuffer.ColorTargets[0].Target;
            var stagingDesc = new TextureDescription(color.Width, color.Height, 1, 1, 1, color.Format, TextureUsage.Staging, TextureType.Texture2D);
            var staging = GraphicsDevice.ResourceFactory.CreateTexture(ref stagingDesc);

            var cl = GraphicsDevice.ResourceFactory.CreateCommandList();
            cl.Name = "CL:RetrieveFramebuffer";
            cl.Begin();
            cl.CopyTexture(color, staging);
            cl.End();
            GraphicsDevice.SubmitCommands(cl);
            GraphicsDevice.WaitForIdle();
            cl.Dispose();

            var mapped = GraphicsDevice.Map(staging, MapMode.Read);
            var result = new Image<Bgra32>((int)color.Width, (int)color.Height);
            var sourceSpan = new Span<uint>(mapped.Data.ToPointer(), (int)mapped.SizeInBytes);
            var stride = (int)mapped.RowPitch / sizeof(uint);

            for (int j = 0; j < color.Height; j++)
            {
                var sourceRow = sourceSpan.Slice(stride * j, (int)color.Width);
                var destRow = MemoryMarshal.Cast<Bgra32, uint>(result.GetPixelRowSpan(j));
                sourceRow.CopyTo(destRow);
            }

            GraphicsDevice.Unmap(staging);
            staging.Dispose();
            return result;
        }

        void Update(float deltaSeconds)
        {
            _frameTimeAverager.AddTime(deltaSeconds);
            Raise(new EngineUpdateEvent(deltaSeconds));
        }

        void ToggleFullscreenState()
        {
            if (_window == null)
                return;
            bool isFullscreen = _window.WindowState == WindowState.BorderlessFullScreen;
            _window.WindowState = isFullscreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
        }

        void Draw(VeldridRendererContext context)
        {
            var camera = Resolve<ICamera>();
            context.Framebuffer?.CreateDeviceObjects(context);
            if (_window != null)
            {
                CoreTrace.Log.Info("Engine", "Start draw");
                int width = _window.Width;
                int height = _window.Height;

                if (_windowResized)
                {
                    GraphicsDevice.ResizeMainWindow((uint)width, (uint)height);
                    Raise(new WindowResizedEvent(width, height));
                    CoreTrace.Log.Info("Engine", "Resize finished");
                    _windowResized = false;
                }

                camera.Viewport = new Vector2(width, height);
            }
            else if (context.Framebuffer != null)
            {
                camera.Viewport = new Vector2(context.Framebuffer.Width, context.Framebuffer.Height);
            }

            var scenes = new List<Scene>();
            Raise(new CollectScenesEvent(scenes.Add));

            using (PerfTracker.FrameEvent("6.1 Prepare scenes"))
            {
                _frameCommands.Begin();
                Raise(RenderEvent.Instance);
                foreach (var scene in scenes)
                    scene.UpdatePerFrameResources(context, _renderables);
                _frameCommands.End();
                GraphicsDevice.SubmitCommands(_frameCommands);
            }

            using (PerfTracker.FrameEvent("6.2 Render scenes"))
            {
                _frameCommands.Begin();
                foreach (var scene in scenes)
                    scene.RenderAllStages(context, _renderers);
                _frameCommands.End();
            }

            using (PerfTracker.FrameEvent("6.3 Submit commandlist"))
            {
                CoreTrace.Log.Info("Scene", "Submitting commands");
                GraphicsDevice.SubmitCommands(_frameCommands);
                CoreTrace.Log.Info("Scene", "Submitted commands");
                GraphicsDevice.WaitForIdle();
            }
        }

        public override void ChangeBackend()
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
                    if (GraphicsDevice.BackendType != backend)
                        Resolve<IShaderCache>().DestroyAllDeviceObjects();
                    GraphicsDevice.Dispose();
                }

                if (_useRenderDoc)
                {
                    using (PerfTracker.InfrequentEvent("Loading renderdoc"))
                    {
                        if (!RenderDoc.Load(out _renderDoc))
                            throw new InvalidOperationException("Failed to load renderdoc");
                    }

                    _renderDoc.APIValidation = true;
                }

                if (_showWindow && (_window == null || _recreateWindow))
                {
                    _recreateWindow = false;
                    _window?.Close();

                    var windowInfo = new WindowCreateInfo
                    {
                        X = _window?.X ?? _defaultX,
                        Y = _window?.Y ?? _defaultY,
                        WindowWidth = _window?.Width ?? _defaultWidth,
                        WindowHeight = _window?.Height ?? _defaultHeight,
                        WindowInitialState = _window?.WindowState ?? WindowState.Normal,
                        WindowTitle = "UAlbion"
                    };

                    _window = VeldridStartup.CreateWindow(ref windowInfo);
                    _windowManager.Window = new VeldridSdlWindow(_window);
                    //Window.BorderVisible = false;
                    _window.CursorVisible = false;
                    _window.Resized += () => _windowResized = true;
                    _window.FocusGained += () => Raise(new FocusGainedEvent());
                    _window.FocusLost += () => Raise(new FocusLostEvent());
                    _windowResized = true;
                }

                GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(
                    _renderDoc != null, PixelFormat.R32_Float, false,
                    ResourceBindingModel.Improved, true,
                    true, false)
                {
                    SyncToVerticalBlank = _vsync,
#if DEBUG
                    Debug = true
#endif
                };

                // Currently this field only exists in my local build of veldrid, so set it via reflection.
                var singleThreadedProperty = typeof(GraphicsDeviceOptions).GetField("SingleThreaded");
                if (singleThreadedProperty != null)
                    singleThreadedProperty.SetValueDirect(__makeref(gdOptions), true);

                if (_showWindow)
                {
                    GraphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, gdOptions, backend);
                }
                else
                {
                    /* This switch statement causes a compiler crash \o/
                    GraphicsDevice = backend switch
                    {
                        GraphicsBackend.Direct3D11 => GraphicsDevice.CreateD3D11(gdOptions),
                        GraphicsBackend.Metal => GraphicsDevice.CreateMetal(gdOptions),
                        GraphicsBackend.Vulkan => GraphicsDevice.CreateVulkan(gdOptions),
                        GraphicsBackend.OpenGL => throw new InvalidOperationException( "Headless rendering is not supported on the OpenGL backend"),
                        GraphicsBackend.OpenGLES => throw new InvalidOperationException( "Headless rendering is not supported on the OpenGL ES backend"),
                        _ => throw new InvalidOperationException($"Unhandled backend: {backend}")
                    }; */

                    switch (backend)
                    {
                        case GraphicsBackend.Direct3D11: GraphicsDevice = GraphicsDevice.CreateD3D11(gdOptions); break;
                        case GraphicsBackend.Metal: GraphicsDevice = GraphicsDevice.CreateMetal(gdOptions); break;
                        case GraphicsBackend.Vulkan: GraphicsDevice = GraphicsDevice.CreateVulkan(gdOptions); break;
                        default: GraphicsDevice = null; break;
                    }

                    if (GraphicsDevice == null)
                        throw new InvalidOperationException($"Unsupported backend for headless rendering: {backend}");
                }

                GraphicsDevice.WaitForIdle();
                SetTitle();

                Raise(new BackendChangedEvent());
            }
        }

        void SetTitle()
        {
            if (!_showWindow) return;
            if (DateTime.UtcNow - _lastTitleUpdateTime <= TimeSpan.FromSeconds(1)) return;

            _window.Title = $"{WindowTitle} - {GraphicsDevice.BackendType} ({FrameTimeText})";
            _lastTitleUpdateTime = DateTime.UtcNow;
        }

        public VeldridRendererContext BuildContext(FramebufferSource framebuffer)
        {
            ChangeBackend();
            using (PerfTracker.InfrequentEvent("Create objects"))
            {
                _frameCommands = GraphicsDevice.ResourceFactory.CreateCommandList();
                _frameCommands.Name = "Frame Commands List";

                CommandList initList = GraphicsDevice.ResourceFactory.CreateCommandList();
                initList.Name = "Recreation Initialization Command List";
                initList.Begin();
                _sceneContext = new SceneContext();
                AttachChild(_sceneContext);
                _sceneContext.CreateDeviceObjects(GraphicsDevice, initList);

                var initContext = new VeldridRendererContext(GraphicsDevice, initList, _sceneContext, _coreFactory, framebuffer);
                var renderContext = new VeldridRendererContext(GraphicsDevice, _frameCommands, _sceneContext, _coreFactory, framebuffer);
                framebuffer?.CreateDeviceObjects(initContext);
                foreach (var r in _renderers.Values.Distinct())
                    r.CreateDeviceObjects(initContext);

                initList.End();
                GraphicsDevice.SubmitCommands(initList);
                GraphicsDevice.WaitForIdle();
                initList.Dispose();
                _objectsCreated = true;

                return renderContext;
            }
        }

        void DestroyAllObjects()
        {
            if (!_objectsCreated)
                return;

            using (PerfTracker.InfrequentEvent("Destroying objects"))
            {
                GraphicsDevice.WaitForIdle();
                _frameCommands?.Dispose();

                if (_sceneContext != null)
                {
                    _sceneContext.Dispose();
                    RemoveChild(_sceneContext);
                    _sceneContext = null;
                }

                _frameCommands = null;

                foreach (var r in _renderers.Values.Distinct())
                    r.DestroyDeviceObjects();

                Resolve<ITextureManager>()?.DestroyDeviceObjects();
                Resolve<IDeviceObjectManager>()?.DestroyDeviceObjects();
                GraphicsDevice.WaitForIdle();
                _objectsCreated = false;
            }
        }

        public void Dispose()
        {
            foreach(var renderer in _renderers.Values.Distinct())
                if (renderer is IDisposable disposable)
                    disposable.Dispose();
            _renderers.Clear();
        }
    }
}
#pragma warning restore CA2213

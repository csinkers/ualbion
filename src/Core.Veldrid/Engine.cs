using System;
using System.Runtime.InteropServices;
using ImGuiNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using UAlbion.Api;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;

namespace UAlbion.Core.Veldrid
{
    public sealed class Engine : ServiceComponent<IEngine>, IEngine, IDisposable
    {
        static RenderDoc _renderDoc;

        readonly FrameTimeAverager _frameTimeAverager = new(0.5);
        readonly WindowHolder _windowHolder;
        readonly bool _useRenderDoc;
        readonly bool _startupOnly;
        readonly int _defaultWidth = 720;
        readonly int _defaultHeight = 480;
        readonly int _defaultX = 648;
        readonly int _defaultY = 431;
        readonly ISceneRenderer _sceneRenderer;

        GraphicsDevice _graphicsDevice;
        CommandList _frameCommands;
        GraphicsBackend? _newBackend;
        bool _done;

        public bool IsDepthRangeZeroToOne => _graphicsDevice?.IsDepthRangeZeroToOne ?? false;
        public bool IsClipSpaceYInverted => _graphicsDevice?.IsClipSpaceYInverted ?? false;
        public string FrameTimeText => $"{_graphicsDevice.BackendType} {_frameTimeAverager.CurrentAverageFramesPerSecond:N2} fps ({_frameTimeAverager.CurrentAverageFrameTimeMilliseconds:N3} ms)";

        public Engine(GraphicsBackend backend, bool useRenderDoc, bool startupOnly, bool showWindow, ISceneRenderer sceneRenderer, Rectangle? windowRect = null)
        {
            _newBackend = backend;
            _useRenderDoc = useRenderDoc;
            _startupOnly = startupOnly;
            _sceneRenderer = sceneRenderer ?? throw new ArgumentNullException(nameof(sceneRenderer));
            _windowHolder = showWindow ? AttachChild(new WindowHolder()) : null;

            if (windowRect.HasValue)
            {
                _defaultX = windowRect.Value.X;
                _defaultY = windowRect.Value.Y;
                _defaultWidth = windowRect.Value.Width;
                _defaultHeight = windowRect.Value.Height;
            }

            On<WindowClosedEvent>(_ =>
            {
                if (_newBackend == null)
                    _done = true;
            });
            On<QuitEvent>(_ => _done = true);
            On<WindowResizedEvent>(e => _graphicsDevice.ResizeMainWindow((uint)e.Width, (uint)e.Height));

            On<LoadRenderDocEvent>(e =>
            {
                if (_renderDoc != null || !RenderDoc.Load(out _renderDoc)) return;
                _newBackend = _graphicsDevice.BackendType;
            });

            On<RunRenderDocEvent>(e => _renderDoc?.LaunchReplayUI());
            On<TriggerRenderDocEvent>(e => _renderDoc?.TriggerCapture());
            On<SetBackendEvent>(e => _newBackend = e.Value);
            On<GarbageCollectionEvent>(e => GC.Collect());
            // On<RecreateWindowEvent>(e => { _recreateWindow = true; _newBackend = _graphicsDevice.BackendType; });
            // Raise(new EngineFlagEvent(e.Value ? FlagOperation.Set : FlagOperation.Clear, EngineFlags.VSync));
        }

        protected override void Subscribed()
        {
            Resolve<IShaderCache>().ShadersUpdated += OnShadersUpdated;
            base.Subscribed();
        }

        protected override void Unsubscribed()
        {
            Resolve<IShaderCache>().ShadersUpdated -= OnShadersUpdated;
            base.Unsubscribed();
        }

        void OnShadersUpdated(object _, EventArgs eventArgs) => _newBackend = _graphicsDevice?.BackendType;

        public void Run()
        {
            if (_windowHolder == null)
                throw new InvalidOperationException("Cannot run as main game loop without a window");

            PerfTracker.StartupEvent("Set up backend");
            Sdl2Native.SDL_Init(SDLInitFlags.GameController);

            if (ImGui.GetCurrentContext() != IntPtr.Zero)
            {
                ImGui.StyleColorsClassic();

                // Turn on ImGui docking if it's supported
                if (Enum.TryParse(typeof(ImGuiConfigFlags), "DockingEnable", out var dockingFlag) && dockingFlag != null)
                    ImGui.GetIO().ConfigFlags |= (ImGuiConfigFlags)dockingFlag;
            }

            bool first = true;
            while (!_done)
            {
                GraphicsBackend backend = _newBackend ?? _graphicsDevice.BackendType;
                using (PerfTracker.InfrequentEvent($"change backend to {backend}"))
                    ChangeBackend(backend);
                _newBackend = null;

                if (first)
                {
                    PerfTracker.StartupEvent("Startup done, rendering first frame");
                    first = false;
                }

                InnerLoop();
                DestroyAllObjects();
            }

            Resolve<IShaderCache>()?.CleanupOldFiles();
        }

        void InnerLoop()
        {
            if (_graphicsDevice == null)
                throw new InvalidOperationException("GraphicsDevice not initialised");

            var frameCounter = new FrameCounter();
            while (!_done && _newBackend == null)
            {
                var deltaSeconds = frameCounter.StartFrame();
                _frameTimeAverager.AddTime(deltaSeconds);

                PerfTracker.BeginFrame();
                using (PerfTracker.FrameEvent("1 Raising begin frame"))
                    Raise(BeginFrameEvent.Instance);

                using (PerfTracker.FrameEvent("2 Processing SDL events"))
                {
                    Sdl2Events.ProcessEvents();
                    _windowHolder.PumpEvents(deltaSeconds);
                }

                using (PerfTracker.FrameEvent("5 Performing update"))
                {
                    Raise(new EngineUpdateEvent((float) deltaSeconds));
                }

                using (PerfTracker.FrameEvent("5.1 Flushing queued events"))
                    Exchange.FlushQueuedEvents();

                using (PerfTracker.FrameEvent("5.2 Calculating UI layout"))
                    Raise(new LayoutEvent());

                using (PerfTracker.FrameEvent("6 Drawing"))
                    Draw();

                var flags = Resolve<IEngineSettings>().Flags;
                if (_graphicsDevice.SyncToVerticalBlank != ((flags & EngineFlags.VSync) != 0))
                    _graphicsDevice.SyncToVerticalBlank = (flags & EngineFlags.VSync) != 0;

                using (PerfTracker.FrameEvent("7 Swap buffers"))
                {
                    CoreTrace.Log.Info("Engine", "Swapping buffers...");
                    _graphicsDevice.SwapBuffers();
                    CoreTrace.Log.Info("Engine", "Draw complete");
                }

                if (_startupOnly)
                    _done = true;
            }
        }

        void Draw()
        {
            using (PerfTracker.FrameEvent("6.1 Prepare scenes"))
            {
                _frameCommands.Begin();
                Raise(new PostEngineUpdateEvent(_graphicsDevice, _frameCommands));
                Raise(RenderEvent.Instance); // TODO: Remove?
                Raise(new PrepareFrameResourcesEvent(_graphicsDevice, _frameCommands));
                Raise(new PrepareFrameResourceSetsEvent(_graphicsDevice, _frameCommands));
                _frameCommands.End();
                _graphicsDevice.SubmitCommands(_frameCommands);
            }

            using (PerfTracker.FrameEvent("6.2 Render scenes"))
            {
                _frameCommands.Begin();
                _sceneRenderer.Render(_graphicsDevice, _frameCommands);
                _frameCommands.End();
            }

            using (PerfTracker.FrameEvent("6.3 Submit commandlist"))
            {
                CoreTrace.Log.Info("Scene", "Submitting commands");
                _graphicsDevice.SubmitCommands(_frameCommands);
                CoreTrace.Log.Info("Scene", "Submitted commands");
                _graphicsDevice.WaitForIdle();
            }
        }

        void ChangeBackend(GraphicsBackend backend)
        {
            if (_useRenderDoc)
            {
                using (PerfTracker.InfrequentEvent("Loading renderdoc"))
                {
                    if (!RenderDoc.Load(out _renderDoc))
                        throw new InvalidOperationException("Failed to load renderdoc");
                }

                _renderDoc.APIValidation = true;
            }

            var flags = Resolve<IEngineSettings>().Flags;
            _windowHolder?.CreateWindow(_defaultX, _defaultY, _defaultWidth, _defaultHeight);
            GraphicsDeviceOptions gdOptions = new GraphicsDeviceOptions(
                _renderDoc != null,
                PixelFormat.R32_Float,
                (flags & EngineFlags.VSync) != 0,
                ResourceBindingModel.Improved,
                true,
                true,
                false)
            {
#if DEBUG
                Debug = true
#endif
            };

            // Currently this field only exists in my local build of veldrid, so set it via reflection.
            // var singleThreadedProperty = typeof(GraphicsDeviceOptions).GetField("SingleThreaded");
            // if (singleThreadedProperty != null)
            //     singleThreadedProperty.SetValueDirect(__makeref(gdOptions), true);

            if (_windowHolder != null)
            {
                _graphicsDevice = VeldridStartup.CreateGraphicsDevice(_windowHolder.Window, gdOptions, backend);
            }
            else
            {
                _graphicsDevice = backend switch
                {
                    GraphicsBackend.Direct3D11 => GraphicsDevice.CreateD3D11(gdOptions),
                    GraphicsBackend.Metal => GraphicsDevice.CreateMetal(gdOptions),
                    GraphicsBackend.Vulkan => GraphicsDevice.CreateVulkan(gdOptions),
                    _ => throw new InvalidOperationException($"Unsupported backend for headless rendering: {backend}")
                };
            }

            _graphicsDevice.WaitForIdle();

            _frameCommands = _graphicsDevice.ResourceFactory.CreateCommandList();
            _frameCommands.Name = "Frame Commands List";

            Raise(new DeviceCreatedEvent(_graphicsDevice));
            Raise(new BackendChangedEvent());
        }

        void DestroyAllObjects()
        {
            using (PerfTracker.InfrequentEvent("Destroying objects"))
            {
                Raise(new DestroyDeviceObjectsEvent());
                _graphicsDevice?.WaitForIdle();
                _frameCommands?.Dispose();
                _graphicsDevice?.Dispose();
                _frameCommands = null;
                _graphicsDevice = null;
            }
        }

        public unsafe Image<Bgra32> RenderFrame(bool captureWithRenderDoc)
        {
            if (captureWithRenderDoc)
                _renderDoc.TriggerCapture();

            Draw();

            var color = _sceneRenderer.Framebuffer.Framebuffer.ColorTargets[0].Target;
            var stagingDesc = new TextureDescription(color.Width, color.Height, 1, 1, 1, color.Format, TextureUsage.Staging, TextureType.Texture2D);
            var staging = _graphicsDevice.ResourceFactory.CreateTexture(ref stagingDesc);

            var cl = _graphicsDevice.ResourceFactory.CreateCommandList();
            cl.Name = "CL:RetrieveFramebuffer";
            cl.Begin();
            cl.CopyTexture(color, staging);
            cl.End();
            _graphicsDevice.SubmitCommands(cl);
            _graphicsDevice.WaitForIdle();
            cl.Dispose();

            var mapped = _graphicsDevice.Map(staging, MapMode.Read);
            var result = new Image<Bgra32>((int)color.Width, (int)color.Height);
            var sourceSpan = new Span<uint>(mapped.Data.ToPointer(), (int)mapped.SizeInBytes);
            var stride = (int)mapped.RowPitch / sizeof(uint);

            for (int j = 0; j < color.Height; j++)
            {
                var sourceRow = sourceSpan.Slice(stride * j, (int)color.Width);
                var destRow = MemoryMarshal.Cast<Bgra32, uint>(result.GetPixelRowSpan(j));
                sourceRow.CopyTo(destRow);
            }

            _graphicsDevice.Unmap(staging);
            staging.Dispose();
            return result;
        }

        public void Dispose()
        {
        }
    }
}

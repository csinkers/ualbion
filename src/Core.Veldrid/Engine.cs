using System;
using System.Runtime.InteropServices;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public sealed class Engine : ServiceComponent<IEngine>, IEngine, IDisposable
{
    static RenderDoc _renderDoc;

    readonly FrameTimeAverager _frameTimeAverager = new(0.5);
    readonly FenceHolder _fence;
    readonly WindowHolder _windowHolder;
    readonly PrepareFrameEvent _prepareFrameEvent = new();
    readonly PrepareFrameResourcesEvent _prepareFrameResourcesEvent = new();
    readonly PrepareFrameResourceSetsEvent _prepareFrameResourceSetsEvent = new();
    readonly bool _useRenderDoc;

    GraphicsDevice _graphicsDevice;
    CommandList _frameCommands;
    GraphicsBackend? _newBackend;
    bool _done;
    bool _active = true;

    public bool StartupOnly { get; set; }
    public bool IsDepthRangeZeroToOne => _graphicsDevice?.IsDepthRangeZeroToOne ?? false;
    public bool IsClipSpaceYInverted => _graphicsDevice?.IsClipSpaceYInverted ?? false;
    public string FrameTimeText => $"{_graphicsDevice.BackendType} {_frameTimeAverager.CurrentAverageFramesPerSecond:N2} fps ({_frameTimeAverager.CurrentAverageFrameTimeMilliseconds:N3} ms)";
    public IRenderPipeline RenderSystem { get; set; }

    public Engine(GraphicsBackend backend, bool useRenderDoc, bool showWindow)
    {
        _newBackend = backend;
        _useRenderDoc = useRenderDoc;
        _windowHolder = showWindow ? new WindowHolder() : null;
        _fence = new FenceHolder("RenderStage fence");
        AttachChild(_fence);

        if (_windowHolder != null)
            AttachChild(_windowHolder);

        On<WindowHiddenEvent>(_ => _active = false);
        On<WindowShownEvent>(_ =>
        {
            Raise(new RefreshFramebuffersEvent());
            _active = true;
        });
        On<WindowClosedEvent>(_ =>
        {
            if (_newBackend == null)
                _done = true;
        });
        On<QuitEvent>(_ => _done = true);
        On<WindowResizedEvent>(e => _graphicsDevice?.ResizeMainWindow((uint)e.Width, (uint)e.Height));

        On<LoadRenderDocEvent>(_ =>
        {
            if (_renderDoc != null || !RenderDoc.Load(out _renderDoc)) return;
            _newBackend = _graphicsDevice.BackendType;
        });

        On<RunRenderDocEvent>(_ => _renderDoc?.LaunchReplayUI());
        On<TriggerRenderDocEvent>(_ => _renderDoc?.TriggerCapture());
        On<SetBackendEvent>(e => _newBackend = e.Value);
        On<GarbageCollectionEvent>(_ => GC.Collect());
        // On<RecreateWindowEvent>(e => { _recreateWindow = true; _newBackend = _graphicsDevice.BackendType; });
        // Raise(new EngineFlagEvent(e.Value ? FlagOperation.Set : FlagOperation.Clear, EngineFlags.VSync));
    }

    protected override void Subscribed()
    {
        Resolve<IShaderLoader>().ShadersUpdated += OnShadersUpdated;
        base.Subscribed();
    }

    protected override void Unsubscribed()
    {
        Resolve<IShaderLoader>().ShadersUpdated -= OnShadersUpdated;
        base.Unsubscribed();
    }

    void OnShadersUpdated(object _, EventArgs eventArgs) => _newBackend = _graphicsDevice?.BackendType;

    public void Run()
    {
        if (_windowHolder == null)
            throw new InvalidOperationException("Cannot run as main game loop without a window");

        PerfTracker.StartupEvent("Set up backend");
        Sdl2Native.SDL_Init(SDLInitFlags.GameController);

        bool first = true;
        GraphicsBackend? oldBackend = null;

        while (!_done)
        {
            GraphicsBackend backend = _newBackend ?? _graphicsDevice.BackendType;
            using (PerfTracker.InfrequentEvent($"change backend to {backend}"))
                ChangeBackend(backend, oldBackend);

            oldBackend = _newBackend;
            _newBackend = null;

            if (first)
            {
                PerfTracker.StartupEvent("Startup done, rendering first frame");
                first = false;
            }

            InnerLoop();
            DestroyAllObjects();
        }
    }

    void InnerLoop()
    {
        if (_graphicsDevice == null)
            throw new InvalidOperationException("GraphicsDevice not initialised");

        var frameCounter = new FrameCounter();
        while (!_done && _newBackend == null)
        {
            var flags = Var(CoreVars.User.EngineFlags);
            var deltaSeconds =
                (flags & EngineFlags.FixedTimeStep) != 0
                ? 1 / 60.0f
                : frameCounter.StartFrame();

            _frameTimeAverager.AddTime(deltaSeconds);

            PerfTracker.BeginFrame();
            using (PerfTracker.FrameEvent("Raising begin frame"))
                Raise(BeginFrameEvent.Instance);

            using (PerfTracker.FrameEvent("Processing window events"))
                _windowHolder.PumpEvents(deltaSeconds);

            using (PerfTracker.FrameEvent("Performing update"))
                Raise(new EngineUpdateEvent((float)deltaSeconds));

            using (PerfTracker.FrameEvent("Flushing queued events"))
                Exchange.FlushQueuedEvents();

            if ((flags & EngineFlags.SuppressLayout) == 0)
                using (PerfTracker.FrameEvent("Calculating UI layout"))
                     Raise(new LayoutEvent());

            if (_active)
            {
                using (PerfTracker.FrameEvent("Render "))
                    Draw();

                if (_graphicsDevice.SyncToVerticalBlank != ((flags & EngineFlags.VSync) != 0))
                    _graphicsDevice.SyncToVerticalBlank = (flags & EngineFlags.VSync) != 0;

                using (PerfTracker.FrameEvent("Swap buffers"))
                {
                    CoreTrace.Log.Info("Engine", "Swapping buffers...");
                    _graphicsDevice.SwapBuffers();
                    CoreTrace.Log.Info("Engine", "Draw complete");
                }
            }
            else Thread.Sleep(16);

            if (StartupOnly)
                _done = true;

            //Console.WriteLine($"Frame {frameCounter.FrameCount} complete, press Enter to continue");
            //Console.ReadLine();
        }
    }

    void Draw()
    {
        var renderSystem = RenderSystem;
        if (renderSystem == null)
            return;

        using (PerfTracker.FrameEvent("Prepare scenes"))
        {
            _frameCommands.Begin();

            _prepareFrameResourcesEvent.Device = _graphicsDevice;
            _prepareFrameResourcesEvent.CommandList = _frameCommands;
            _prepareFrameResourceSetsEvent.Device = _graphicsDevice;
            _prepareFrameResourceSetsEvent.CommandList = _frameCommands;

            Raise(_prepareFrameEvent);
            Raise(_prepareFrameResourcesEvent);
            Raise(_prepareFrameResourceSetsEvent);

            _frameCommands.End();
        }

        using (PerfTracker.FrameEvent("Submit prepare commands"))
        {
            _fence.Fence.Reset();
            _graphicsDevice.SubmitCommands(_frameCommands, _fence.Fence);
            _graphicsDevice.WaitForFence(_fence.Fence);
        }

        renderSystem.Render(_graphicsDevice);
    }

    void ChangeBackend(GraphicsBackend backend, GraphicsBackend? oldBackend)
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

        var flags = Var(CoreVars.User.EngineFlags);
        var gdOptions = new GraphicsDeviceOptions(
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

        try
        {
            if (_windowHolder != null)
            {
                _windowHolder.CreateWindow();
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
        catch (Exception ex)
        {
            if (!oldBackend.HasValue)
                throw;

            Error($"Failed to create graphics device for {backend}: {ex}");
            Error($"Falling back to {oldBackend}");
            ChangeBackend(oldBackend.Value, null);
        }
    }

    void DestroyAllObjects()
    {
        using (PerfTracker.InfrequentEvent("Destroying objects"))
        {
            _graphicsDevice?.WaitForIdle();
            Raise(new DestroyDeviceObjectsEvent());
            _frameCommands?.Dispose();
            _graphicsDevice?.Dispose();
            _frameCommands = null;
            _graphicsDevice = null;
        }
    }

    public void RenderFrame(bool captureWithRenderDoc)
    {
        if(_newBackend != null)
            ChangeBackend(_newBackend.Value, _graphicsDevice?.BackendType);
        _newBackend = null;

        if (captureWithRenderDoc)
            _renderDoc.TriggerCapture();

        Draw();
    }

    public unsafe Image<Bgra32> ReadTexture2D(ITextureHolder textureHolder)
    {
        if (textureHolder == null) throw new ArgumentNullException(nameof(textureHolder));
        var texture = textureHolder.DeviceTexture;
        var stagingDesc = new TextureDescription(
            texture.Width, texture.Height,
            1, 1, 1,
            texture.Format,
            TextureUsage.Staging,
            TextureType.Texture2D);

        using var staging = _graphicsDevice.ResourceFactory.CreateTexture(ref stagingDesc);
        using var cl = _graphicsDevice.ResourceFactory.CreateCommandList();

        cl.Name = "CL:ReadTexture2D";
        cl.Begin();
        cl.CopyTexture(texture, staging);
        cl.End();
        _graphicsDevice.SubmitCommands(cl);
        _graphicsDevice.WaitForIdle();

        var mapped = _graphicsDevice.Map(staging, MapMode.Read);
        try
        {
            var result = new Image<Bgra32>((int)texture.Width, (int)texture.Height);
            var sourceSpan = new Span<uint>(mapped.Data.ToPointer(), (int)mapped.SizeInBytes);
            var stride = (int)mapped.RowPitch / sizeof(uint);

            for (int j = 0; j < texture.Height; j++)
            {
                var sourceRow = sourceSpan.Slice(stride * j, (int)texture.Width);
                var destRow = MemoryMarshal.Cast<Bgra32, uint>(result.GetPixelRowSpan(j));
                sourceRow.CopyTo(destRow);
            }

            return result;
        }
        finally
        {
            _graphicsDevice.Unmap(staging);
        }
    }

    public void Dispose()
    {
        DestroyAllObjects();
        _windowHolder?.Dispose();
        _fence.Dispose();
    }

    public GraphicsDeviceFeatures GraphicsFeatures => _graphicsDevice.Features;
    public PixelFormatProperties? GetPixelFormatProperties(PixelFormat format) =>
        _graphicsDevice.GetPixelFormatSupport(format, TextureType.Texture2D, TextureUsage.Sampled,
            out var properties)
            ? properties
            : null;
}

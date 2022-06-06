using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ImGuiNET;
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

namespace UAlbion.Core.Veldrid;

public sealed class Engine : ServiceComponent<IEngine>, IEngine, IDisposable
{
    static RenderDoc _renderDoc;

    readonly FrameTimeAverager _frameTimeAverager = new(0.5);
    readonly FenceHolder _fence;
    readonly WindowHolder _windowHolder;
    readonly List<IRenderPass> _renderPasses = new();
    readonly bool _useRenderDoc;
    readonly bool _startupOnly;
    readonly int _defaultWidth = 1870; // 720; // TODO: Save in user settings
    readonly int _defaultHeight = 1400; // 480; 
    readonly int _defaultX = 687; // 648;
    readonly int _defaultY = 31; // 431;

    GraphicsDevice _graphicsDevice;
    CommandList _frameCommands;
    GraphicsBackend? _newBackend;
    bool _done;

    public bool IsDepthRangeZeroToOne => _graphicsDevice?.IsDepthRangeZeroToOne ?? false;
    public bool IsClipSpaceYInverted => _graphicsDevice?.IsClipSpaceYInverted ?? false;
    public string FrameTimeText => $"{_graphicsDevice.BackendType} {_frameTimeAverager.CurrentAverageFramesPerSecond:N2} fps ({_frameTimeAverager.CurrentAverageFrameTimeMilliseconds:N3} ms)";

    public Engine(GraphicsBackend backend, bool useRenderDoc, bool startupOnly, bool showWindow, Rectangle? windowRect = null)
    {
        _newBackend = backend;
        _useRenderDoc = useRenderDoc;
        _startupOnly = startupOnly;
        _windowHolder = showWindow ? new WindowHolder() : null;
        _fence = new FenceHolder("RenderStage fence");
        AttachChild(_fence);

        if (_windowHolder != null)
            AttachChild(_windowHolder);

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
        On<WindowResizedEvent>(e =>
        {
            _graphicsDevice?.ResizeMainWindow((uint)e.Width, (uint)e.Height);
        });

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

    public IList<IRenderPass> RenderPasses => _renderPasses.AsReadOnly();
    public Engine AddRenderPass(IRenderPass pass)
    {
        _renderPasses.Add(pass);
        return this;
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
                Raise(new EngineUpdateEvent((float)deltaSeconds));

            using (PerfTracker.FrameEvent("5.1 Flushing queued events"))
                Exchange.FlushQueuedEvents();

            using (PerfTracker.FrameEvent("5.2 Calculating UI layout"))
                Raise(new LayoutEvent());

            using (PerfTracker.FrameEvent("6 Drawing"))
                Draw();

            var flags = GetVar(CoreVars.User.EngineFlags);
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

            //Console.WriteLine($"Frame {frameCounter.FrameCount} complete, press Enter to continue");
            //Console.ReadLine();
        }
    }

    void Draw()
    {
        var camera = Resolve<ICamera>(); // TODO: More sophisticated approach, support multiple cameras etc
        _fence.Fence.Reset();

        using (PerfTracker.FrameEvent("6.1 Prepare scenes"))
        {
            _frameCommands.Begin();
            Raise(new RenderEvent(camera));
            Raise(new PrepareFrameResourcesEvent(_graphicsDevice, _frameCommands));
            Raise(new PrepareFrameResourceSetsEvent(_graphicsDevice, _frameCommands));
            _frameCommands.End();
            _graphicsDevice.SubmitCommands(_frameCommands, _fence.Fence);
        }

        _graphicsDevice.WaitForFence(_fence.Fence);
        foreach (var phase in _renderPasses)
        {
            _fence.Fence.Reset();
            using (PerfTracker.FrameEvent("6.2 Render scenes"))
            {
                _frameCommands.Begin();
                phase.Render(_graphicsDevice, _frameCommands);
                _frameCommands.End();
            }

            using (PerfTracker.FrameEvent("6.3 Submit commandlist"))
            {
                CoreTrace.Log.Info("Scene", "Submitting commands");
                _graphicsDevice.SubmitCommands(_frameCommands, _fence.Fence);
                CoreTrace.Log.Info("Scene", "Submitted commands");
                _graphicsDevice.WaitForFence(_fence.Fence);
            }
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

        var flags = GetVar(CoreVars.User.EngineFlags);
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

        if (_windowHolder != null)
        {
            _windowHolder.CreateWindow(_defaultX, _defaultY, _defaultWidth, _defaultHeight);
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

    public unsafe Image<Bgra32> RenderFrame(bool captureWithRenderDoc, int phase)
    {
        if(_newBackend != null)
            ChangeBackend(_newBackend.Value);
        _newBackend = null;

        if (captureWithRenderDoc)
            _renderDoc.TriggerCapture();

        Draw();

        var color = _renderPasses[phase].Framebuffer.Framebuffer.ColorTargets[0].Target;
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
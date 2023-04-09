using System;
using System.Buffers;
using System.Numerics;
using System.Threading;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Veldrid.Reflection;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public delegate void ImGuiMenuFunc(
    IImGuiManager manager,
    IFramebufferHolder framebuffer,
    ICameraProvider camera,
    GameWindow gameWindow);

public class ImGuiManager : ServiceComponent<IImGuiManager>, IImGuiManager
{
    static readonly TimeSpan LayoutUpdateInterval = TimeSpan.FromSeconds(10);
    readonly ImGuiMenuFunc _menuFunc;
    readonly DebugGuiRenderer _renderer;
    readonly IFramebufferHolder _framebuffer;
    readonly GameWindow _gameWindow;
    readonly ICameraProvider _camera;
    int _nextWindowId;
    bool _initialised;
    DateTime _lastLayoutUpdate;

    public InputEvent LastInput { get; private set; }
    public bool ConsumedKeyboard { get; set; }
    public bool ConsumedMouse { get; set; }

    public ImGuiManager(
        DebugGuiRenderer renderer,
        IFramebufferHolder framebuffer,
        GameWindow gameWindow,
        ICameraProvider camera,
        ImGuiMenuFunc menuFunc)
    {
        _renderer    = renderer    ?? throw new ArgumentNullException(nameof(renderer));
        _framebuffer = framebuffer ?? throw new ArgumentNullException(nameof(framebuffer));
        _gameWindow  = gameWindow  ?? throw new ArgumentNullException(nameof(gameWindow));
        _camera      = camera      ?? throw new ArgumentNullException(nameof(camera));
        _menuFunc    = menuFunc    ?? throw new ArgumentNullException(nameof(menuFunc));

        On<DeviceCreatedEvent>(_ => Dirty());
        On<InputEvent>(OnInput);
    }

#pragma warning disable CA1822 // Mark members as static
    public object IoInfo
    {
        get
        {
            if (ImGui.GetCurrentContext() == IntPtr.Zero)
                return null;

            var io = ImGui.GetIO();
            return new Foo
            {
                MouseDelta = io.MouseDelta,
                DeltaTime = io.DeltaTime,
                WantCaptureKeyboard = io.WantCaptureKeyboard,
                WantCaptureMouse = io.WantCaptureMouse
            };
        }
    }
#pragma warning restore CA1822 // Mark members as static

    void OnInput(InputEvent e)
    {
        _renderer.ImGuiRenderer?.Update((float)e.DeltaSeconds, e.Snapshot);
        var io = ImGui.GetIO();
        ConsumedKeyboard = io.WantCaptureKeyboard;
        ConsumedMouse = io.WantCaptureMouse;
        LastInput = e;

        ReflectorUtil.SwapAuxiliaryState();
        _menuFunc(this, _framebuffer, _camera, _gameWindow);

        ImGui.DockSpaceOverViewport();

        // Build array of windows before iterating, as closing a window will modify the Children collection.
        int windowCount = 0;
        var array = ArrayPool<IImGuiWindow>.Shared.Rent(Children.Count);

        foreach (var child in Children)
            if (child is IImGuiWindow window)
                array[windowCount++] = window;

        for (int i = 0; i < windowCount; i++)
            array[i].Draw();

        ArrayPool<IImGuiWindow>.Shared.Return(array);

        if (DateTime.UtcNow - _lastLayoutUpdate > LayoutUpdateInterval)
        {
            _lastLayoutUpdate = DateTime.UtcNow.Date;
            var ini = ImGui.SaveIniSettingsToMemory();
        }
    }

    void Dirty() => On<PrepareFrameResourcesEvent>(CreateDeviceObjects);
    void CreateDeviceObjects(PrepareFrameResourcesEvent e)
    {
        if (ImGui.GetCurrentContext() == IntPtr.Zero)
            return;

        ImGui.StyleColorsClassic();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        Off<PrepareFrameResourcesEvent>();
    }

    protected override void Subscribed()
    {
        if (!_initialised)
        {
            var layout = Var(CoreVars.Ui.ImGuiLayout);
            // ImGui.LoadIniSettingsFromMemory();
        }

        Dirty();
    }
    // protected override void Unsubscribed() => Dispose();

    public int GetNextWindowId() => Interlocked.Increment(ref _nextWindowId);
    public void AddWindow(IImGuiWindow window) => AttachChild(window);

    public IntPtr GetOrCreateImGuiBinding(Texture texture)
    {
        var engine = Resolve<IVeldridEngine>();
        return _renderer.ImGuiRenderer.GetOrCreateImGuiBinding(engine.Device.ResourceFactory, texture);
    }

    public IntPtr GetOrCreateImGuiBinding(TextureView textureView)
    {
        var engine = Resolve<IVeldridEngine>();
        return _renderer.ImGuiRenderer.GetOrCreateImGuiBinding(engine.Device.ResourceFactory, textureView);
    }

    public void RemoveImGuiBinding(Texture texture) 
        => _renderer.ImGuiRenderer.RemoveImGuiBinding(texture);

    public void RemoveImGuiBinding(TextureView textureView) 
        => _renderer.ImGuiRenderer.RemoveImGuiBinding(textureView);
}

public class Foo
{
    public Vector2 MouseDelta { get; set; }
    public float DeltaTime { get; set; }
    public bool WantCaptureKeyboard { get; set; }
    public bool WantCaptureMouse { get; set; }
}
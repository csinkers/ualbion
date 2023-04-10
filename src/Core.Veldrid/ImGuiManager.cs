using System;
using System.Buffers;
using System.Threading;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Veldrid.Reflection;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public class ImGuiManager : ServiceComponent<IImGuiManager>, IImGuiManager
{
    static readonly TimeSpan LayoutUpdateInterval = TimeSpan.FromSeconds(10);
    readonly DebugGuiRenderer _renderer;
    int _nextWindowId;
    bool _initialised;
    DateTime _lastLayoutUpdate;

    public InputEvent LastInput { get; private set; }
    public bool ConsumedKeyboard { get; set; }
    public bool ConsumedMouse { get; set; }

    public ImGuiManager(DebugGuiRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

        On<DeviceCreatedEvent>(_ => Dirty());
        On<InputEvent>(OnInput);
    }

    void OnInput(InputEvent e)
    {
        _renderer.ImGuiRenderer?.Update((float)e.DeltaSeconds, e.Snapshot);
        var io = ImGui.GetIO();
        ConsumedKeyboard = io.WantCaptureKeyboard;
        ConsumedMouse = io.WantCaptureMouse;
        LastInput = e;

        ReflectorUtil.SwapAuxiliaryState();
        TryResolve<IImGuiMenuManager>()?.Draw(this);

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
            var lastLayout = Var(CoreVars.Ui.ImGuiLayout);
            var ini = ImGui.SaveIniSettingsToMemory();
            if (!string.Equals(lastLayout, ini, StringComparison.Ordinal))
            {
                var settings = Resolve<ISettings>();
                CoreVars.Ui.ImGuiLayout.Write(settings, ini);
            }
        }
    }

    void Dirty() => On<PrepareFrameResourcesEvent>(CreateDeviceObjects);
    void CreateDeviceObjects(PrepareFrameResourcesEvent e)
    {
        if (ImGui.GetCurrentContext() == IntPtr.Zero)
            return;

        var io = ImGui.GetIO();
        unsafe { io.NativePtr->IniFilename = null; } // Turn off ini file
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        ImGui.StyleColorsClassic();

        if (!_initialised)
        {
            var layout = Var(CoreVars.Ui.ImGuiLayout);
            ImGui.LoadIniSettingsFromMemory(layout);
            _initialised = true;
        }

        Off<PrepareFrameResourcesEvent>();
    }

    protected override void Subscribed() => Dirty();
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
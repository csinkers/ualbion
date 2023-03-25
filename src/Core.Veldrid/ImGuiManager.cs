using System;
using System.Collections.Generic;
using System.Threading;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Core.Veldrid.Reflection;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public class ImGuiManager : ServiceComponent<IImGuiManager>, IImGuiManager
{
    readonly Action<IImGuiManager> _menuFunc;
    readonly List<IImGuiWindow> _windows = new();
    int _nextWindowId;
    bool _demoOpen;

    public ImGuiManager(Action<IImGuiManager> menuFunc)
    {
        _menuFunc = menuFunc ?? throw new ArgumentNullException(nameof(menuFunc));
        On<DeviceCreatedEvent>(_ => Dirty());
    }

    void Dirty() => On<PrepareFrameResourcesEvent>(e => CreateDeviceObjects(e.Device));
    static void CreateDeviceObjects(GraphicsDevice device)
    {
        if (ImGui.GetCurrentContext() == IntPtr.Zero)
            return;

        ImGui.StyleColorsClassic();
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
    }

    protected override void Subscribed() => Dirty();
    // protected override void Unsubscribed() => Dispose();

    public int GetNextWindowId() => Interlocked.Increment(ref _nextWindowId);
    public void AddWindow(IImGuiWindow window)
    {
        if (window is IComponent component)
            AttachChild(component);

        _windows.Add(window);
    }

    public void Draw()
    {
        ReflectorUtil.SwapAuxiliaryState();
        _menuFunc(this);

        ImGui.DockSpaceOverViewport();
        ImGui.ShowDemoWindow(ref _demoOpen);
        foreach (var window in _windows)
            window.Draw();
    }
}
using System;
using System.Collections.Generic;
using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid;

public class ImGuiMenuManager : ServiceComponent<IImGuiMenuManager>, IImGuiMenuManager
{
    readonly SubMenuMenuItem _root = new("", "");
    readonly Dictionary<string, ShowWindowMenuItem> _windowTypes = [];

    public void AddMenuItem(IMenuItem item)
    {
        if (item is ShowWindowMenuItem showWindow)
            _windowTypes.Add(showWindow.Name, showWindow);

        _root.Add(item);
    }

    public IImGuiWindow CreateWindow(string name, IImGuiManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);

        if (!_windowTypes.TryGetValue(name, out var showWindow))
        {
            Warn($"Could not created window \"{name}\", no such window type registered");
            return null;
        }

        var id = manager.GetNextWindowId();
        var window = showWindow.Constructor(showWindow.BuildName(id));
        manager.AddWindow(window);
        return window;
    }

    public void Draw(IImGuiManager manager)
    {
        if (!ImGui.BeginMainMenuBar())
            return;

        _root.DrawTopLevel(manager);

        ImGui.EndMainMenuBar();
    }
}
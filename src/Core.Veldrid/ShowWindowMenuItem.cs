using System;
using ImGuiNET;

namespace UAlbion.Core.Veldrid;

public class ShowWindowMenuItem : IMenuItem
{
    public ShowWindowMenuItem(string name, string path, Func<string, IImGuiWindow> constructor)
    {
        Name = name;
        Path = path;
        Constructor = constructor;
    }

    public string Name { get; }
    public string Path { get; }
    public Func<string, IImGuiWindow> Constructor { get; }
    public override string ToString() => $"Show {Name} ({Path})";
    public string BuildName(int id) => $"{Name}##{id}";
    public void Draw(IImGuiManager manager)
    {
        if (!ImGui.MenuItem(Name))
            return;

        var windowName = BuildName(manager.GetNextWindowId());
        var window = Constructor(windowName);
        manager.AddWindow(window);
    }
}
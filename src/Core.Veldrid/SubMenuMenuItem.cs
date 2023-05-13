using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;

namespace UAlbion.Core.Veldrid;

public class SubMenuMenuItem : IMenuItem
{
    readonly List<IMenuItem> _items = new();
    public SubMenuMenuItem(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public string Name { get; }
    public string Path { get; }
    public override string ToString() => $"SubMenu {Path}/{Name}";

    public void Add(IMenuItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));

        string path = string.IsNullOrEmpty(Path) 
            ? Name 
            : $"{Path}/{Name}";

        if (!item.Path.StartsWith(path, StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected item path ({item.Path}) to start with {path}");

        if (item.Path.Length > path.Length)
        {
            var remainingPath = item.Path[path.Length..];
            var index = remainingPath.IndexOf('/', StringComparison.Ordinal);
            var nextPart = index == -1 ? remainingPath : remainingPath[..index];
            var subMenu = _items.OfType<SubMenuMenuItem>().FirstOrDefault(x => x.Name == nextPart);
            if (subMenu == null)
            {
                subMenu = new SubMenuMenuItem(nextPart, path);
                _items.Add(subMenu);
            }

            subMenu.Add(item);
        }
        else _items.Add(item);
    }

    public void DrawTopLevel(IImGuiManager manager)
    {
        foreach (var item in _items)
            item.Draw(manager);
    }

    public void Draw(IImGuiManager manager)
    {
        if (!ImGui.BeginMenu(Name))
            return;

        foreach (var item in _items)
            item.Draw(manager);

        ImGui.EndMenu();
    }
}
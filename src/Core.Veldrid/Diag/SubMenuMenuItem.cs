using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;

namespace UAlbion.Core.Veldrid.Diag;

public class SubMenuMenuItem : IMenuItem
{
    readonly List<IMenuItem> _items = [];
    public SubMenuMenuItem(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public string Name { get; }
    public string Path { get; }
    public override string ToString() => $"SubMenu {Path}";

    public void Add(IMenuItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!item.Path.StartsWith(Path, StringComparison.Ordinal))
            throw new InvalidOperationException($"Expected item path ({item.Path}) to start with {Path}");

        if (item.Path.Length > Path.Length)
        {
            var remainingPath = item.Path[Path.Length..].TrimStart('/');
            var index = remainingPath.IndexOf('/', StringComparison.Ordinal);
            var nextPart = index == -1 ? remainingPath : remainingPath[..index];
            var subMenu = _items.OfType<SubMenuMenuItem>().FirstOrDefault(x => x.Name == nextPart);
            if (subMenu == null)
            {
                var newPath = string.IsNullOrEmpty(Path) ? nextPart : $"{Path}/{nextPart}";
                subMenu = new SubMenuMenuItem(nextPart, newPath);
                _items.Add(subMenu);
                _items.Sort((x, y) => Comparer<string>.Default.Compare(x.Name, y.Name));
            }

            subMenu.Add(item);
        }
        else
        {
            _items.Add(item);
            _items.Sort((x,y) => Comparer<string>.Default.Compare(x.Name, y.Name));
        }
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

        foreach (var item in _items.OfType<SubMenuMenuItem>())
            item.Draw(manager);

        foreach (var item in _items.Where(item => item is not SubMenuMenuItem))
            item.Draw(manager);

        ImGui.EndMenu();
    }
}
using System;
using ImGuiNET;

namespace UAlbion.Core.Veldrid.Diag;

public class AdhocMenuItem : IMenuItem
{
    readonly Action<IImGuiManager> _onClick;

    public AdhocMenuItem(string name, string path, Action<IImGuiManager> onClick)
    {
        _onClick = onClick;
        Name = name;
        Path = path;
    }

    public string Name { get; }
    public string Path { get; }

    public void Draw(IImGuiManager manager)
    {
        ArgumentNullException.ThrowIfNull(manager);

        if (!ImGui.MenuItem(Name))
            return;

        _onClick(manager);
    }
}
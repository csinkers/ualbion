using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class DemoWindow : Component, IImGuiWindow
{
    public string Name { get; }
    public DemoWindow(string name) => Name = name;

    public void Draw()
    {
        bool open = true;
        ImGui.ShowDemoWindow(ref open);

        if (!open)
            Remove();
    }
}
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid.Diag;

namespace UAlbion.Game.Veldrid.Diag;

public class DemoWindow : Component, IImGuiWindow
{
    public string Name { get; }
    public DemoWindow(string name) => Name = name;

    public ImGuiWindowDrawResult Draw()
    {
        bool open = true;
        ImGui.ShowDemoWindow(ref open);
        return open ? ImGuiWindowDrawResult.None : ImGuiWindowDrawResult.Closed;
    }
}
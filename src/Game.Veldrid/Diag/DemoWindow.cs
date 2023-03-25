using ImGuiNET;
using UAlbion.Core;

namespace UAlbion.Game.Veldrid.Diag;

public class DemoWindow : IImGuiWindow
{
    bool _open;
    public DemoWindow(int id) { }
    public void Draw() => ImGui.ShowDemoWindow(ref _open);
}
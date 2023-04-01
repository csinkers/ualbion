using ImGuiNET;
using UAlbion.Core.Veldrid;
using Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class DemoWindow : IImGuiWindow
{
    bool _open;
    public DemoWindow(int id) { }
    public void Draw(GraphicsDevice device) => ImGui.ShowDemoWindow(ref _open);
}
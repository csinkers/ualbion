using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class AssetExplorerWindow : Component, IImGuiWindow
{
    readonly string _name;
    public AssetExplorerWindow(string name) => _name = name;
    public void Draw()
    {
        bool open = true;
        ImGui.Begin(_name, ref open);
        ImGui.End();

        if (!open)
            Remove();
    }
}
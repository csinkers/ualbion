using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class AssetExplorerWindow : Component, IImGuiWindow
{
    readonly string _name;
    public AssetExplorerWindow(int id) => _name = $"Asset Explorer###Assets{id}";
    public void Draw()
    {
        bool open = true;
        ImGui.Begin(_name, ref open);
        ImGui.End();

        if (!open)
            Remove();
    }
}
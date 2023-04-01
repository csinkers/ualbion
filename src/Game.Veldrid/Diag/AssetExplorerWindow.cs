using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;
using Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class AssetExplorerWindow : Component, IImGuiWindow
{
    readonly string _name;
    public AssetExplorerWindow(int id) => _name = $"Asset Explorer###Assets{id}";
    public void Draw(GraphicsDevice device)
    {
        ImGui.Begin(_name);
        ImGui.End();
    }
}
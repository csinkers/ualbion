using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;

namespace UAlbion.Game.Veldrid.Diag;

public class AssetExplorerWindow : Component, IImGuiWindow
{
    public string Name { get; }
    public AssetExplorerWindow(string name) => Name = name;
    public void Draw()
    {
        bool open = true;
        ImGui.Begin(Name, ref open);
        ImGui.End();

        if (!open)
            Remove();
    }
}
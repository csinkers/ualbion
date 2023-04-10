using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid;

public class ImGuiMenuManager : ServiceComponent<IImGuiMenuManager>, IImGuiMenuManager
{
    readonly SubMenuMenuItem _root = new("", "");
    public void AddMenuItem(IMenuItem item) => _root.Add(item);

    public void Draw(IImGuiManager manager)
    {
        if (!ImGui.BeginMainMenuBar())
            return;

        _root.DrawTopLevel(manager);

        ImGui.EndMainMenuBar();
    }
}
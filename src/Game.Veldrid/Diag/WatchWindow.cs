using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Reflection;
using UAlbion.Game.State;

namespace UAlbion.Game.Veldrid.Diag;

public class WatchWindow(string name, object globals) : Component, IImGuiWindow
{
    public string Name { get; } = name;

    public void Draw()
    {
        bool open = true;
        ImGui.Begin(Name, ref open);

        var reflectorManager = Resolve<ReflectorManager>();
        reflectorManager.RenderOptions();
        if (globals != null)
            reflectorManager.RenderNode("Globals", globals);

        var state = TryResolve<IGameState>();
        if (state != null)
            reflectorManager.RenderNode("State", state);

        reflectorManager.RenderNode("Exchange", Exchange);
        reflectorManager.RenderNode("ImGui", Resolve<IImGuiManager>());

        ImGui.End();

        if (!open)
            Remove();
    }
}
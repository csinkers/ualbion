using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Reflection;
using UAlbion.Game.State;

namespace UAlbion.Game.Veldrid.Diag;

public class WatchWindow : Component, IImGuiWindow
{
    readonly string _name;

    public WatchWindow(int id)
    {
        _name = $"Watch###Watch{id}";
    }

    public void Draw()
    {
        ImGui.Begin(_name);

        var state = TryResolve<IGameState>();
        if (state != null)
            RenderNode("State", state);

        RenderNode("Exchange", Exchange);

        ImGui.End();
    }

    static void RenderNode(string name, object target)
    {
        var meta = new ReflectorMetadata(name, null, null, null);
        var state = new ReflectorState(target, null, -1, meta);
        var reflector = ReflectorManager.Instance.GetReflectorForInstance(state.Target);
        reflector(state);
    }
}
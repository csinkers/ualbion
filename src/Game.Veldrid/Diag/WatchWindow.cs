﻿using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Reflection;
using UAlbion.Game.State;

namespace UAlbion.Game.Veldrid.Diag;

public class WatchWindow : Component, IImGuiWindow
{
    public string Name { get; }
    public WatchWindow(string name) => Name = name;

    public void Draw()
    {
        bool open = true;
        ImGui.Begin(Name, ref open);

        var state = TryResolve<IGameState>();
        if (state != null)
            RenderNode("State", state);

        RenderNode("Exchange", Exchange);
        RenderNode("ImGui", Resolve<IImGuiManager>());

        ImGui.End();

        if (!open)
            Remove();
    }

    static void RenderNode(string name, object target)
    {
        var meta = new ReflectorMetadata(name, null, null, null);
        var state = new ReflectorState(target, null, -1, meta);
        var reflector = ReflectorManager.Instance.GetReflectorForInstance(state.Target);
        reflector(state);
    }
}
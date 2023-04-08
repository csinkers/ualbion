﻿using System.Collections.Generic;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Core.Veldrid.Reflection;

namespace UAlbion.Game.Veldrid.Diag;

public class InspectorWindow : Component, IImGuiWindow
{
    readonly string _name;
    IList<Selection> _hits;

    public InspectorWindow(int id)
    {
        _name = $"Inspector###Inspect{id}";
        On<InspectorPickEvent>(e => _hits = e.Selections);
    }

    public void Draw()
    {
        bool open = true;
        ImGui.Begin(_name, ref open);
        int hitId = 0;
        if (_hits != null)
        {
            foreach (var hit in _hits)
            {
                var target = hit.Formatter == null ? hit.Target : hit.Formatter(hit.Target);
                RenderNode($"{hitId}", target);
                hitId++;
            }
        }
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
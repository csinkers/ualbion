using System.Collections.Generic;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Diag;
using UAlbion.Core.Veldrid.Reflection;

namespace UAlbion.Game.Veldrid.Diag;

public class InspectorWindow : Component, IImGuiWindow
{
    IList<Selection> _hits;

    public string Name { get; }
    public InspectorWindow(string name)
    {
        Name = name;
        On<InspectorPickEvent>(e => _hits = e.Selections);
    }

    public ImGuiWindowDrawResult Draw()
    {
        bool open = true;
        ImGui.Begin(Name, ref open);
        int hitId = 0;
        if (_hits != null)
        {
            var reflectorManager = Resolve<ReflectorManager>();
            foreach (var hit in _hits)
            {
                var target = hit.Formatter == null ? hit.Target : hit.Formatter(hit.Target);
                reflectorManager.RenderNode($"{hitId}", target);
                hitId++;
            }
        }
        ImGui.End();
        return open ? ImGuiWindowDrawResult.None : ImGuiWindowDrawResult.Closed;
    }
}
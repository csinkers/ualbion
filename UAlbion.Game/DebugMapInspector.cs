using System.Collections.Generic;
using ImGuiNET;
using UAlbion.Core;
using UAlbion.Core.Events;

namespace UAlbion.Game
{
    public class DebugMapInspector : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<DebugMapInspector, EngineUpdateEvent>((x, _) => x.RenderDialog()),
            new Handler<DebugMapInspector, SelectionResultsEvent>((x, e) => x._hits = e.Selections),
        };

        IList<Selection> _hits;

        void RenderDialog()
        {
            if (_hits == null)
                return;
            ImGui.BeginGroup();
            foreach (var hit in _hits)
                ImGui.LabelText(hit.Name, $"Hit at {hit.IntersectionPoint}: {hit.Target}");
            ImGui.EndGroup();
        }

        public DebugMapInspector() : base(Handlers)
        {
        }
    }
}
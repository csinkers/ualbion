using System.Collections.Generic;
using System.Numerics;
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
            new Handler<DebugMapInspector, ShowDebugInfoEvent>((x, e) => x._hits = e.Selections),
        };

        IList<Selection> _hits;

        void RenderDialog()
        {
            if (_hits == null)
                return;
            ImGui.Begin("Inspector");
            ImGui.BeginChild("Inspector");
            int hitId = 0;
            foreach (var hit in _hits)
            {
                if (ImGui.TreeNode($"Hit {hitId}"))
                {
                    ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.0f, 1.0f), hit.Name);
                    ImGui.Value("X", hit.IntersectionPoint.X);
                    ImGui.Value("Y", hit.IntersectionPoint.Y);
                    ImGui.Value("Z", hit.IntersectionPoint.Z);
                    ImGui.TextWrapped($"{hit.Target}");
                    ImGui.TreePop();
                }

                hitId++;
            }
            ImGui.EndChild();
            ImGui.End();

            /*

            Window: Begin & End
            Menus: BeginMenuBar, MenuItem, EndMenuBar
            Colours: ColorEdit4
            Graph: PlotLines
            Text: Text, TextColored
            ScrollBox: BeginChild, EndChild

            */
        }

        public DebugMapInspector() : base(Handlers)
        {
        }
    }
}
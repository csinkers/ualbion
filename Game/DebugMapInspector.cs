using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class DebugMapInspector : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<DebugMapInspector, EngineUpdateEvent>((x, _) => x.RenderDialog()),
            new Handler<DebugMapInspector, ShowDebugInfoEvent>((x, e) =>
            {
                x._hits = e.Selections;
                x._mousePosition = e.MousePosition;
            }),
        };

        IList<Selection> _hits;
        Vector2 _mousePosition;

        void RenderDialog()
        {
            if (_hits == null)
                return;

            var state = Exchange.Resolve<IStateManager>();
            if (state == null)
                return;

            ImGui.Begin("Inspector");
            ImGui.BeginChild("Inspector");
            ImGui.Text($"CursorPos: {_mousePosition} TileSize: {state.TileSize}");
            ImGui.Text($"Camera Position: {state.CameraPosition}");
            ImGui.Text($"TilePosition: {state.CameraTilePosition}");
            ImGui.Text($"Direction: {state.CameraDirection}");
            ImGui.Text($"Magnification: {state.CameraMagnification}");

            int hitId = 0;
            foreach (var hit in _hits)
            {
                var p = hit.IntersectionPoint;
                ImGui.SetNextTreeNodeOpen(true);
                if (ImGui.TreeNode($"Hit {hitId}"))
                {
                    ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.0f, 1.0f), $"{hit.Name} ({ p.X}, {p.Y}, {p.Z})");
                    var reflected = Reflector.Reflect(hit.Target.ToString(), hit.Target);
                    RenderNode(reflected);
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

        void RenderNode(Reflector.ReflectedObject reflected)
        {
            if (reflected.SubObjects != null)
            {
                if (ImGui.TreeNode($"{reflected.Name}: {reflected.Value} ({reflected.Object.GetType().Name})"))
                {
                    foreach (var child in reflected.SubObjects)
                        RenderNode(child);
                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.TextWrapped($"{reflected.Name}: {reflected.Value} ({reflected.Object?.GetType().Name})");
            }
        }

        public DebugMapInspector() : base(Handlers) { }
    }
}
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game
{
    public class DebugMapInspector : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<DebugMapInspector, EngineUpdateEvent>((x, _) => x.RenderDialog()),
            H<DebugMapInspector, ShowDebugInfoEvent>((x, e) =>
            {
                x._hits = e.Selections;
                x._mousePosition = e.MousePosition;
            }),
            H<DebugMapInspector, SetTextureOffsetEvent>((x, e) =>
            {
                EightBitTexture.OffsetX = e.X;
                EightBitTexture.OffsetY = e.Y;
            }),
            H<DebugMapInspector, SetTextureScaleEvent>((x, e) =>
            {
                EightBitTexture.ScaleAdjustX = e.X;
                EightBitTexture.ScaleAdjustY = e.Y;
            }));

        IList<Selection> _hits;
        Vector2 _mousePosition;

        void RenderDialog()
        {
            if (_hits == null)
                return;

            var state = Exchange.Resolve<IStateManager>();
            var window = Exchange.Resolve<IWindowManager>();
            if (state == null)
                return;

            ImGui.Begin("Inspector");
            ImGui.BeginChild("Inspector");

            var normPos = window.PixelToNorm(_mousePosition);
            var uiPos = window.NormToUi(normPos);
            uiPos.X = (int) uiPos.X; uiPos.Y = (int) uiPos.Y;
            ImGui.Text($"Cursor Pix: {_mousePosition} UI: {uiPos} Norm: {normPos} Scale: {window.GuiScale} PixSize: {window.Size}");
            ImGui.Text($"Camera World: {state.CameraPosition} Tile: {state.CameraTilePosition} Dir: {state.CameraDirection} Mag: {state.CameraMagnification}");
            ImGui.Text($"TileSize: {state.TileSize}");

            int hitId = 0;
            foreach (var hit in _hits)
            {
                var p = hit.IntersectionPoint;
                ImGui.SetNextItemOpen(true);
                if (ImGui.TreeNode($"Hit {hitId}"))
                {
                    Reflector.ReflectedObject reflected;
                    if (hit.Target is INamed named)
                    {
                        ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.0f, 1.0f), $"{named.Name} ({p.X}, {p.Y}, {p.Z})");
                        reflected = Reflector.Reflect(named.Name, hit.Target);
                    }
                    else
                    {
                        ImGui.TextColored(new Vector4(1.0f, 0.8f, 0.0f, 1.0f), $"{hit.Target} ({p.X}, {p.Y}, {p.Z})");
                        reflected = Reflector.Reflect(null, hit.Target);
                    }

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
            var description = 
                reflected.Name == null
                ? $"{reflected.Value} ({reflected.Object.GetType().Name})"
                : $"{reflected.Name}: {reflected.Value} ({reflected.Object.GetType().Name})";

            if (reflected.SubObjects != null)
            {
                if (ImGui.TreeNode(description))
                {
                    foreach (var child in reflected.SubObjects)
                        RenderNode(child);
                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.TextWrapped(description);
            }
        }

        public DebugMapInspector() : base(Handlers) { }
    }
}

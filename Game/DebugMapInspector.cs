using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Game.Events;
using UAlbion.Game.Settings;
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

            var state = Resolve<IStateManager>();
            var window = Resolve<IWindowManager>();
            if (state == null)
                return;

            var scene = Resolve<ISceneManager>().ActiveScene;
            Vector3 cameraPosition = scene.Camera.Position;
            Vector3 cameraTilePosition = cameraPosition;

            var map = Resolve<IMapManager>().Current;
            if (map != null)
                cameraTilePosition /= map.TileSize;

            Vector3 cameraDirection = scene.Camera.LookDirection;
            float cameraMagnification = scene.Camera.Magnification;

            ImGui.Begin("Inspector");
            ImGui.BeginChild("Inspector");

            void BoolOption(string name, Func<bool> getter, Action<bool> setter)
            {
                bool value = getter();
                bool initialValue = value;
                ImGui.Checkbox(name, ref value);
                if (value != initialValue)
                    setter(value);
            }

            var settings = Resolve<ISettings>();
            ImGui.BeginGroup();
            ImGui.Columns(3);
            BoolOption("DrawPositions",            () => settings.Debug.DrawPositions,            x => Raise(new SetDrawPositionsEvent(x)));
            BoolOption("HighlightTile",            () => settings.Debug.HighlightTile,            x => Raise(new SetHighlightTileEvent(x)));
            ImGui.NextColumn();
            BoolOption("HighlightSelection",       () => settings.Debug.HighlightSelection,       x => Raise(new SetHighlightSelectionEvent(x)));
            BoolOption("HighlightEventChainZones", () => settings.Debug.HighlightEventChainZones, x => Raise(new SetHighlightEventChainZonesEvent(x)));
            ImGui.NextColumn();
            BoolOption("ShowPaths",                () => settings.Debug.ShowPaths,                x => Raise(new SetShowPathsEvent(x)));
            ImGui.Columns(1);
            ImGui.EndGroup();

            var normPos = window.PixelToNorm(_mousePosition);
            var uiPos = window.NormToUi(normPos);
            uiPos.X = (int) uiPos.X; uiPos.Y = (int) uiPos.Y;
            ImGui.Text($"Cursor Pix: {_mousePosition} UI: {uiPos} Norm: {normPos} Scale: {window.GuiScale} PixSize: {window.Size}");
            ImGui.Text($"Camera World: {cameraPosition} Tile: {cameraTilePosition} Dir: {cameraDirection} Mag: {cameraMagnification}");
            ImGui.Text($"TileSize: {map?.TileSize}");

            int hitId = 0;
            foreach (var hit in _hits)
            {
                if (ImGui.TreeNode($"{hitId} {hit.Target}"))
                {
                    var reflected = Reflector.Reflect(null, hit.Target);
                    if (reflected.SubObjects != null)
                        foreach (var child in reflected.SubObjects)
                            RenderNode(child);
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
            var typeName = reflected.Object?.GetType().Name ?? "null";
            var description = 
                reflected.Name == null
                ? $"{reflected.Value} ({typeName})"
                : $"{reflected.Name}: {reflected.Value} ({typeName})";

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

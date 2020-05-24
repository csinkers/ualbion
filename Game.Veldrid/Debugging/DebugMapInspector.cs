using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Game.Debugging;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using UAlbion.Game.Settings;
using UAlbion.Game.State;
using UAlbion.Game.Veldrid.Audio;

namespace UAlbion.Game.Veldrid.Debugging
{
    [Event("hide_debug_window", "Hide the debug window")]
    public class HideDebugWindowEvent : Event { }

    public class DebugMapInspector : Component
    {
        readonly IDictionary<Type, Func<DebugInspectorAction, Reflector.ReflectedObject, object>> _behaviours =
            new Dictionary<Type, Func<DebugInspectorAction, Reflector.ReflectedObject, object>>();

        readonly IContainer _services;
        readonly IList<object> _fixedObjects = new List<object>();
        IList<Selection> _hits;
        Vector2 _mousePosition;
        Reflector.ReflectedObject _lastHoveredItem;

        public DebugMapInspector(IContainer services)
        {
            _services = services;
            On<EngineUpdateEvent>(e => RenderDialog());
            On<HideDebugWindowEvent>(e => _hits = null);
            On<ShowDebugInfoEvent>(e =>
            {
                _hits = e.Selections;
                _mousePosition = e.MousePosition;
            });
        }

        public DebugMapInspector AddBehaviour(IDebugBehaviour behaviour)
        {
            foreach(var type in behaviour.HandledTypes)
                _behaviours[type] = behaviour.Handle;
            return this;
        }

        void RenderDialog()
        {
            bool anyHovered = false;
            if (_hits == null)
                return;

            var state = Resolve<IGameState>();
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
            if (ImGui.Button("Close"))
            {
                _hits = null;
                ImGui.EndChild();
                ImGui.End();
                return;
            }

            void BoolOption(string name, Func<bool> getter, Action<bool> setter)
            {
                bool value = getter();
                bool initialValue = value;
                ImGui.Checkbox(name, ref value);
                if (value != initialValue)
                    setter(value);
            }

            if (ImGui.TreeNode("Fixed"))
            {
                for (int i = 0; i < _fixedObjects.Count; i++)
                {
                    var thing = _fixedObjects[i];
                    Reflector.ReflectedObject reflected = Reflector.Reflect($"Fixed{i}", thing, null);
                    anyHovered |= RenderNode(reflected, true);
                }
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Stats"))
            {
                if (ImGui.Button("Clear"))
                    PerfTracker.Clear();

                if (ImGui.TreeNode("Perf"))
                {
                    ImGui.BeginGroup();
                    ImGui.Text(Resolve<IEngine>().FrameTimeText);

                    var (descriptions, stats) = PerfTracker.GetFrameStats();
                    ImGui.Columns(2);
                    ImGui.SetColumnWidth(0, 300);
                    foreach (var description in descriptions)
                        ImGui.Text(description);

                    ImGui.NextColumn();
                    foreach (var stat in stats)
                        ImGui.Text(stat);

                    ImGui.Columns(1);
                    ImGui.EndGroup();
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Audio"))
                {
                    var audio = Resolve<IAudioManager>();
                    if (audio == null)
                        ImGui.Text("Audio Disabled");
                    else
                        foreach (var sound in audio.ActiveSounds)
                            ImGui.Text(sound);

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("DeviceObjects"))
                {
                    ImGui.Text(Resolve<IDeviceObjectManager>()?.Stats());
                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Input"))
                {
                    var im = Resolve<IInputManager>();
                    ImGui.Text($"Input Mode: {im.InputMode}");
                    ImGui.Text($"Mouse Mode: {im.MouseMode}");
                    ImGui.Text($"Input Mode Stack: {string.Join(", ", im.InputModeStack)}");
                    ImGui.Text($"Mouse Mode Stack: {string.Join(", ", im.MouseModeStack)}");

                    if (ImGui.TreeNode("Bindings"))
                    {
                        var ib = Resolve<IInputBinder>();
                        foreach (var mode in ib.Bindings)
                        {
                            ImGui.Text(mode.Item1.ToString());
                            foreach(var binding in mode.Item2)
                                ImGui.Text($"    {binding.Item1}: {binding.Item2}");
                        }

                        ImGui.TreePop();
                    }

                    ImGui.TreePop();
                }

                if (ImGui.TreeNode("Textures"))
                {
                    ImGui.Text(Resolve<ITextureManager>()?.Stats());
                    ImGui.TreePop();
                }

                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Settings"))
            {
                var settings = Resolve<ISettings>();
                ImGui.BeginGroup();

#if DEBUG
                if (ImGui.TreeNode("Debug"))
                {
                    void DebugFlagOption(DebugFlags flag)
                    {
                        BoolOption(flag.ToString(), () => settings.Debug.DebugFlags.HasFlag(flag),
                            x => Raise(new DebugFlagEvent(x ? FlagOperation.Set : FlagOperation.Clear, flag)));
                    }

                    DebugFlagOption(DebugFlags.DrawPositions);
                    DebugFlagOption(DebugFlags.HighlightTile);
                    DebugFlagOption(DebugFlags.HighlightEventChainZones);
                    DebugFlagOption(DebugFlags.HighlightCollision);
                    DebugFlagOption(DebugFlags.ShowPaths);
                    DebugFlagOption(DebugFlags.NoMapTileBoundingBoxes);
                    DebugFlagOption(DebugFlags.ShowCursorHotspot);
                    DebugFlagOption(DebugFlags.TraceAttachment);
                    ImGui.TreePop();
                }
#endif

                if (ImGui.TreeNode("Engine"))
                {
                    void EngineFlagOption(EngineFlags flag)
                    {
                        BoolOption(flag.ToString(), () => settings.Engine.Flags.HasFlag(flag),
                            x => Raise(new EngineFlagEvent(x ? FlagOperation.Set : FlagOperation.Clear, flag)));
                    }

                    EngineFlagOption(EngineFlags.ShowBoundingBoxes);
                    EngineFlagOption(EngineFlags.ShowCameraPosition);
                    EngineFlagOption(EngineFlags.FlipDepthRange);
                    EngineFlagOption(EngineFlags.FlipYSpace);
                    EngineFlagOption(EngineFlags.VSync);
                    EngineFlagOption(EngineFlags.HighlightSelection);
                    ImGui.TreePop();
                }

                ImGui.EndGroup();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Positions"))
            {
                var normPos = window.PixelToNorm(_mousePosition);
                var uiPos = window.NormToUi(normPos);
                uiPos.X = (int)uiPos.X;
                uiPos.Y = (int)uiPos.Y;

                Vector3? playerTilePos = Resolve<IParty>()?.WalkOrder.FirstOrDefault()?.GetPosition();

                ImGui.Text($"Cursor Pix: {_mousePosition} UI: {uiPos} Scale: {window.GuiScale} PixSize: {window.Size} Norm: {normPos}");
                ImGui.Text($"Camera World: {cameraPosition} Tile: {cameraTilePosition} Dir: {cameraDirection} Mag: {cameraMagnification}");
                ImGui.Text($"TileSize: {map?.TileSize} PlayerTilePos: {playerTilePos}");
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Services"))
            {
                var reflected = Reflector.Reflect(null, _services, null);
                if (reflected.SubObjects != null)
                    foreach (var child in reflected.SubObjects)
                        anyHovered |= RenderNode(child, false);
                ImGui.TreePop();
            }

            if (ImGui.TreeNode("Exchange"))
            {
                var reflected = Reflector.Reflect(null, Exchange, null);
                if (reflected.SubObjects != null)
                    foreach (var child in reflected.SubObjects)
                        anyHovered |= RenderNode(child, false);
                ImGui.TreePop();
            }

            int hitId = 0;
            foreach (var hit in _hits)
            {
                if (ImGui.TreeNode($"{hitId} {hit.Target}"))
                {
                    var reflected = Reflector.Reflect(null, hit.Target, null);
                    if (reflected.SubObjects != null)
                        foreach (var child in reflected.SubObjects)
                            anyHovered |= RenderNode(child, false);
                    ImGui.TreePop();
                }

                hitId++;
            }

            ImGui.EndChild();
            ImGui.End();

            if (!anyHovered && _lastHoveredItem?.Object != null &&
                _behaviours.TryGetValue(_lastHoveredItem.Object.GetType(), out var callback))
                callback(DebugInspectorAction.Blur, _lastHoveredItem);

            /*

            Window: Begin & End
            Menus: BeginMenuBar, MenuItem, EndMenuBar
            Colours: ColorEdit4
            Graph: PlotLines
            Text: Text, TextColored
            ScrollBox: BeginChild, EndChild

            */
        }

        bool CheckHover(Reflector.ReflectedObject reflected)
        {
            if (!ImGui.IsItemHovered())
                return false;

            if (_lastHoveredItem != reflected)
            {
                if (_lastHoveredItem?.Object != null &&
                    _behaviours.TryGetValue(_lastHoveredItem.Object.GetType(), out var blurredCallback))
                    blurredCallback(DebugInspectorAction.Blur, _lastHoveredItem);

                if (reflected.Object != null &&
                    _behaviours.TryGetValue(reflected.Object.GetType(), out var hoverCallback))
                    hoverCallback(DebugInspectorAction.Hover, reflected);

                _lastHoveredItem = reflected;
            }

            return true;
        }

        bool RenderNode(Reflector.ReflectedObject reflected, bool fixedObject)
        {
            var type = reflected.Object?.GetType();
            var typeName = type?.Name ?? "null";
            var description =
                reflected.Name == null
                    ? $"{reflected.Value} ({typeName})"
                    : $"{reflected.Name}: {reflected.Value} ({typeName})";


            if (type != null &&
                _behaviours.TryGetValue(type, out var callback) &&
                callback(DebugInspectorAction.Format, reflected) is string formatted)
            {
                description += " " + formatted;
            }

            description = FormatUtil.WordWrap(description, 120);
            bool anyHovered = false;
            if (reflected.SubObjects != null)
            {
                if (ImGui.TreeNodeEx(description, ImGuiTreeNodeFlags.AllowItemOverlap))
                {
                    if (!fixedObject && ImGui.Button("Track"))
                        _fixedObjects.Add(reflected.Object);

                    if (fixedObject && ImGui.Button("Stop tracking"))
                        _fixedObjects.Remove(reflected.Object);

                    anyHovered |= CheckHover(reflected);
                    foreach (var child in reflected.SubObjects)
                        anyHovered |= RenderNode(child, false);
                    ImGui.TreePop();
                }
                anyHovered |= CheckHover(reflected);
            }
            else
            {
                ImGui.TextWrapped(description);
                anyHovered |= CheckHover(reflected);
            }

            return anyHovered;
        }
    }
}

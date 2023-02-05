using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Game.Diag;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.Input;
using UAlbion.Game.Settings;
using UAlbion.Game.State;
using UAlbion.Game.Veldrid.Audio;

namespace UAlbion.Game.Veldrid.Debugging;

public class DiagInspector : Component
{
    // readonly IList<object> _fixedObjects = new List<object>();
    readonly Dictionary<Type, DiagInspectorBehaviour> _behaviours = new();
    IList<Selection> _hits;
    Vector2 _mousePosition;
    ReflectorState _lastHoveredItem;

    public DiagInspector()
    {
        On<ShowDebugInfoEvent>(e =>
        {
            _hits = e.Selections;
            _mousePosition = e.MousePosition;
        });
    }

    public void AddBehaviour(Type type, DiagInspectorBehaviour behaviour) => _behaviours[type] = behaviour;

    public void Render()
    {
        var state = TryResolve<IGameState>();
        if (state == null)
            return;

        bool anyHovered = RenderNode("State", state);
        // DrawFixedObjects(ref anyHovered);
        DrawStats();
        DrawSettings();
        DrawPositions();
        DrawExchange(ref anyHovered);
        DrawHits(ref anyHovered);

        if (!anyHovered && _lastHoveredItem.Target != null &&
            _behaviours.TryGetValue(_lastHoveredItem.GetType(), out var callback))
        {
            callback(DebugInspectorAction.Blur, _lastHoveredItem);
        }

    }

    static void BoolOption(string name, Func<bool> getter, Action<bool> setter)
    {
        bool value = getter();
        bool initialValue = value;
        ImGui.Checkbox(name, ref value);
        if (value != initialValue)
            setter(value);
    }

    void DrawStats()
    {
        if (!ImGui.TreeNode("Stats"))
            return;

        if (ImGui.Button("Clear"))
            PerfTracker.Clear();

        if (ImGui.TreeNode("Perf"))
        {
            ImGui.BeginGroup();
            ImGui.Text(Resolve<IEngine>().FrameTimeText);

            var (descriptions, stats) = PerfTracker.GetFrameStats();
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 320);
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
            var audio = TryResolve<IAudioManager>();
            if (audio == null)
                ImGui.Text("Audio Disabled");
            else
                foreach (var sound in audio.ActiveSounds)
                    ImGui.Text(sound);

            ImGui.TreePop();
        }

        // if (ImGui.TreeNode("DeviceObjects"))
        // {
        //     ImGui.Text(Resolve<IDeviceObjectManager>()?.Stats());
        //     ImGui.TreePop();
        // }

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
                    foreach (var binding in mode.Item2)
                        ImGui.Text($"    {binding.Item1}: {binding.Item2}");
                }

                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        // if (ImGui.TreeNode("Textures"))
        // {
        //     ImGui.Text(Resolve<ITextureSource>()?.Stats());
        //     ImGui.TreePop();
        // }

        ImGui.TreePop();
    }
/*
    void DrawFixedObjects(ref bool anyHovered)
    {
        if (!ImGui.TreeNode("Fixed"))
            return;

        for (int i = 0; i < _fixedObjects.Count; i++)
            anyHovered |= RenderNode($"Fixed{i}", _fixedObjects[i]);

        ImGui.TreePop();
    }
*/

    void DrawSettings()
    {
        if (!ImGui.TreeNode("Settings"))
            return;

        ImGui.BeginGroup();

#if DEBUG
        if (ImGui.TreeNode("Debug"))
        {
            static void DebugFlagOption(DiagInspector me, DebugFlags flags, DebugFlags flag)
            {
                BoolOption(flag.ToString(), () => (flags & flag) != 0,
                    x => me.Raise(new DebugFlagEvent(x ? FlagOperation.Set : FlagOperation.Clear, flag)));
            }

            var curFlags = Var(UserVars.Debug.DebugFlags);
            DebugFlagOption(this, curFlags, DebugFlags.DrawPositions);
            DebugFlagOption(this, curFlags, DebugFlags.HighlightTile);
            // DebugFlagOption(this, curFlags, DebugFlags.HighlightChain);
            DebugFlagOption(this, curFlags, DebugFlags.ShowCursorHotspot);
            DebugFlagOption(this, curFlags, DebugFlags.TraceAttachment);

            DebugFlagOption(this, curFlags, DebugFlags.CollisionLayer);
            DebugFlagOption(this, curFlags, DebugFlags.SitLayer);
            DebugFlagOption(this, curFlags, DebugFlags.ZoneLayer);
            DebugFlagOption(this, curFlags, DebugFlags.NpcColliderLayer);
            DebugFlagOption(this, curFlags, DebugFlags.NpcPathLayer);
            ImGui.TreePop();
        }
#endif

        if (ImGui.TreeNode("Engine"))
        {
            static void EngineFlagOption(DiagInspector me, EngineFlags flags, EngineFlags flag)
            {
                BoolOption(flag.ToString(), () => (flags & flag) != 0,
                    x => me.Raise(new EngineFlagEvent(x ? FlagOperation.Set : FlagOperation.Clear, flag)));
            }

            var curFlags = Var(CoreVars.User.EngineFlags);
            EngineFlagOption(this, curFlags, EngineFlags.ShowBoundingBoxes);
            EngineFlagOption(this, curFlags, EngineFlags.ShowCameraPosition);
            EngineFlagOption(this, curFlags, EngineFlags.FlipDepthRange);
            EngineFlagOption(this, curFlags, EngineFlags.FlipYSpace);
            EngineFlagOption(this, curFlags, EngineFlags.VSync);
            EngineFlagOption(this, curFlags, EngineFlags.HighlightSelection);
            EngineFlagOption(this, curFlags, EngineFlags.UseCylindricalBillboards);
            EngineFlagOption(this, curFlags, EngineFlags.RenderDepth);
            EngineFlagOption(this, curFlags, EngineFlags.FixedTimeStep);
            ImGui.TreePop();
        }

        ImGui.EndGroup();
        ImGui.TreePop();
    }

    void DrawPositions()
    {
        if (!ImGui.TreeNode("Positions"))
            return;

        var window = Resolve<IWindowManager>();
        var camera = Resolve<ICamera>();
        Vector3 cameraPosition = camera.Position;
        Vector3 cameraTilePosition = cameraPosition;

        var map = Resolve<IMapManager>().Current;
        if (map != null)
            cameraTilePosition /= map.TileSize;

        Vector3 cameraDirection = camera.LookDirection;
        float cameraMagnification = camera.Magnification;

        var normPos = window.PixelToNorm(_mousePosition);
        var uiPos = window.NormToUi(normPos);
        uiPos.X = (int)uiPos.X;
        uiPos.Y = (int)uiPos.Y;

        var walkOrder = Resolve<IParty>()?.WalkOrder;
        Vector3? playerTilePos = walkOrder?[0].GetPosition();

        ImGui.Text($"Cursor Pix: {_mousePosition} UI: {uiPos} Scale: {window.GuiScale} PixSize: {window.Size} Norm: {normPos}");
        ImGui.Text($"Camera World: {cameraPosition} Tile: {cameraTilePosition} Dir: {cameraDirection} Mag: {cameraMagnification}");
        ImGui.Text($"TileSize: {map?.TileSize} PlayerTilePos: {playerTilePos}");
        ImGui.TreePop();
    }

    void DrawExchange(ref bool anyHovered)
    {
        anyHovered |= RenderNode("Exchange", Exchange);
    }

    void DrawHits(ref bool anyHovered)
    {
        int hitId = 0;
        foreach (var hit in _hits)
        {
            var target = hit.Formatter == null ? hit.Target : hit.Formatter(hit.Target);
            anyHovered |= RenderNode($"{hitId}", target);
            hitId++;
        }
    }

    bool CheckHover(in ReflectorState state)
    {
        if (!ImGui.IsItemHovered())
            return false;

        if (_lastHoveredItem.Target != state.Target)
        {
            if (_lastHoveredItem.Target != null &&
                _behaviours.TryGetValue(_lastHoveredItem.Target.GetType(), out var blurredCallback))
                blurredCallback(DebugInspectorAction.Blur, _lastHoveredItem);

            if (state.Target != null &&
                _behaviours.TryGetValue(state.Target.GetType(), out var hoverCallback))
                hoverCallback(DebugInspectorAction.Hover, state);

            _lastHoveredItem = state;
        }

        return true;
    }

    bool RenderNode(string name, object target)
    {
        var reflector = Reflector.Instance.GetReflectorForInstance(target);
        var state = new ReflectorState(name, target, reflector, null, -1);
        return RenderNode(state);
    }

    bool RenderNode(in ReflectorState state)
    {
        var type = state.Target?.GetType();
        var typeName = state.Reflector.TypeName;
        var value = state.Reflector.GetValue(state.Target);
        var description =
            state.Name == null
                ? $"{value} ({typeName})"
                : $"{state.Name}: {value} ({typeName})";


        if (type != null &&
            _behaviours.TryGetValue(type, out var callback) &&
            callback(DebugInspectorAction.Format, state) is string formatted)
        {
            description += " " + formatted;
        }

        description = FormatUtil.WordWrap(description, 120);
        bool anyHovered = false;
        if (state.Reflector.HasSubObjects && state.Target != null)
        {
            bool customStyle = false;
            if (state.Target is Component component)
            {
                if (!component.IsSubscribed)
                {
                    ImGui.PushStyleColor(0, new Vector4(0.6f, 0.6f, 0.6f, 1));
                    customStyle = true;
                }
                /*
                if (showCheckbox)
                {
                    bool active = component.IsActive;
                    ImGui.Checkbox(component.ComponentId.ToString(CultureInfo.InvariantCulture), ref active);
                    ImGui.SameLine();
                    if (active != component.IsActive)
                        component.IsActive = active;
                }
                */
            }

            bool treeOpen = ImGui.TreeNodeEx(description, ImGuiTreeNodeFlags.AllowItemOverlap);
            if (customStyle)
                ImGui.PopStyleColor();

            if (treeOpen)
            {
                // if (!fixedObject && ImGui.Button("Track"))
                //     _fixedObjects.Add(state.Target);

                // if (fixedObject && ImGui.Button("Stop tracking"))
                //     _fixedObjects.Remove(state.Target);

                anyHovered |= CheckHover(state);
                RenderUiPos(state.Target);
                foreach (var child in state.Reflector.SubObjects(state.Target))
                    anyHovered |= RenderNode(child);
                ImGui.TreePop();
            }
            anyHovered |= CheckHover(state);
        }
        else
        {
            ImGui.Indent();
            ImGui.TextWrapped(description);
            ImGui.Unindent();
            anyHovered |= CheckHover(state);
        }

        return anyHovered;
    }

    void RenderUiPos(object target)
    {
        if (target is not IUiElement element)
            return;

        var snapshot = Resolve<ILayoutManager>().LastSnapshot;
        if (!snapshot.TryGetValue(element, out var node))
            return;

        ImGui.Text($"UI {node.Extents} {node.Order}");
    }
}
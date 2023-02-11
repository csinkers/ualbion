using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Visual;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using UAlbion.Game.Settings;
using UAlbion.Game.State;
using UAlbion.Game.Veldrid.Audio;
using UAlbion.Game.Veldrid.Diag.Reflection;

namespace UAlbion.Game.Veldrid.Diag;

public class DiagInspector : Component
{
    IList<Selection> _hits;
    Vector2 _mousePosition;

    public DiagInspector()
    {
        On<ShowDebugInfoEvent>(e =>
        {
            _hits = e.Selections;
            _mousePosition = e.MousePosition;
        });
    }

    public void Render()
    {
        ReflectorUtil.SwapAuxiliaryState();
        var state = TryResolve<IGameState>();
        if (state == null)
            return;
        RenderNode("State", state);
        DrawStats();
        DrawSettings();
        DrawPositions();
        RenderNode("Exchange", Exchange);
        DrawHits();
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
        //     ImGui.Text(TryResolve<IDeviceObjectManager>()?.Stats());
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
        //     ImGui.Text(TryResolve<ITextureSource>()?.Stats());
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

    void DrawHits()
    {
        int hitId = 0;
        foreach (var hit in _hits)
        {
            var target = hit.Formatter == null ? hit.Target : hit.Formatter(hit.Target);
            RenderNode($"{hitId}", target);
            hitId++;
        }
    }

    static void RenderNode(string name, object target)
    {
        var meta = new ReflectorMetadata(name, null, null, null);
        var state = new ReflectorState(target, null, -1, meta);
        var reflector = ReflectorManager.Instance.GetReflectorForInstance(state.Target);
        reflector(state);
    }
}

using System;
using ImGuiNET;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid;
using UAlbion.Game.Events;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Veldrid.Diag;

public class SettingsWindow : Component, IImGuiWindow
{
    readonly string _name;

    public SettingsWindow(int id)
    {
        _name = $"Settings###Settings{id}";
    }

    public void Draw()
    {
        bool open = true;
        ImGui.Begin(_name, ref open);

#if DEBUG
        if (ImGui.TreeNode("Debug"))
        {
            static void DebugFlagOption(SettingsWindow me, DebugFlags flags, DebugFlags flag)
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
            static void EngineFlagOption(SettingsWindow me, EngineFlags flags, EngineFlags flag)
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

        ImGui.End();

        if (!open)
            Remove();
    }

    static void BoolOption(string name, Func<bool> getter, Action<bool> setter)
    {
        bool value = getter();
        bool initialValue = value;
        ImGui.Checkbox(name, ref value);
        if (value != initialValue)
            setter(value);
    }
}
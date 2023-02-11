using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Veldrid.Diag.Reflection;

class Vec3Reflector : IReflector
{
    public static Vec3Reflector Instance { get; } = new();
    Vec3Reflector() { }

    public void Reflect(in ReflectorState state)
    {
        if (state.Meta?.Options?.Style != DiagEditStyle.ColorPicker)
        {
            RenderLabel(state);
            return;
        }

        var v = (Vector3)state.Target;
        var name = ReflectorUtil.NameText(state);

        ImGui.Indent();
        ImGui.TextUnformatted(name);
        ImGui.SameLine();
        if (ImGui.ColorEdit3("##" + name, ref v))
            state.Meta.Setter(state, v);
        ImGui.Unindent();
    }

    static void RenderLabel(in ReflectorState state)
    {
        var v = (Vector3)state.Target;
        var value = $"({v.X}, {v.Y}, {v.Z})";
        var description = ReflectorUtil.Describe(state, "Vector3", value);
        ImGui.Indent();
        ImGui.TextWrapped(description);
        ImGui.Unindent();
    }
}
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Reflection;

sealed class Vec4Reflector : IReflector
{
    public static Vec4Reflector Instance { get; } = new();
    Vec4Reflector() { }

    public void Reflect(in ReflectorState state)
    {
        if (state.Meta?.Options?.Style != DiagEditStyle.ColorPicker)
        {
            RenderLabel(state);
            return;
        }

        var v = (Vector4)state.Target;
        var name = ReflectorUtil.NameText(state);

        ImGui.Indent();
        ImGui.TextUnformatted(name);
        ImGui.SameLine();
        if (ImGui.ColorEdit4("##" + name, ref v))
            state.Meta.Setter(state, v);
        ImGui.Unindent();
    }

    static void RenderLabel(in ReflectorState state)
    {
        var v = (Vector4)state.Target;
        var value = $"({v.X}, {v.Y}, {v.Z}, {v.W})";
        var description = ReflectorUtil.Describe(state, "Vector4", value);
        ImGui.Indent();
        ImGui.TextWrapped(description);
        ImGui.Unindent();
    }
}
using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Reflection;

sealed class Vec3Reflector : IReflector
{
    const float DefaultSizeLimits = 300.0f;

    public static Vec3Reflector Instance { get; } = new();
    Vec3Reflector() { }

    public void Reflect(in ReflectorState state)
    {
        switch (state.Meta?.Options?.Style)
        {
            case DiagEditStyle.Position: RenderPosition(state); break;
            case DiagEditStyle.Size: RenderSize(state); break;
            case DiagEditStyle.ColorPicker: RenderColorPicker(state); break;
            default: RenderLabel(state); break;
        }
    }

    static void RenderPosition(in ReflectorState state)
    {
        var v = (Vector3)state.Target;
        var name = ReflectorUtil.NameText(state);

        if (ImGui.DragFloat3(name, ref v))
            state.Meta.Setter(state, v);
    }

    static void RenderSize(in ReflectorState state)
    {
        var v = (Vector3)state.Target;
        var name = ReflectorUtil.NameText(state);

        if (ImGui.SliderFloat3(name, ref v, 0, DefaultSizeLimits))
            state.Meta.Setter(state, v);
    }

    static void RenderColorPicker(in ReflectorState state)
    {
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
        var description = ReflectorUtil.DescribePlain(state, "Vector3", value);
        ImGui.Indent();
        ImGui.TextWrapped(description);
        ImGui.Unindent();
    }
}
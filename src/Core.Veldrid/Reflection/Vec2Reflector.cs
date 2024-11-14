using System.Numerics;
using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Reflection;

sealed class Vec2Reflector : IReflector
{
    const float DefaultSizeLimits = 300.0f;

    public static Vec2Reflector Instance { get; } = new();
    Vec2Reflector() { }

    public void Reflect(in ReflectorState state)
    {
        switch (state.Meta?.Options?.Style)
        {
            case DiagEditStyle.Position: RenderPosition(state); break;
            case DiagEditStyle.Size: RenderSize(state); break;
            default: RenderLabel(state); break;
        }
    }

    static void RenderPosition(in ReflectorState state)
    {
        var v = (Vector2)state.Target;
        var name = ReflectorUtil.NameText(state);

        if (ImGui.DragFloat2(name, ref v))
            state.Meta.Setter(state, v);
    }

    static void RenderSize(in ReflectorState state)
    {
        var v = (Vector2)state.Target;
        var name = ReflectorUtil.NameText(state);

        if (ImGui.SliderFloat2(name, ref v, 0, DefaultSizeLimits))
            state.Meta.Setter(state, v);
    }

    static void RenderLabel(in ReflectorState state)
    {
        var v = (Vector2)state.Target;
        var value = $"({v.X}, {v.Y})";
        var description = ReflectorUtil.DescribePlain(state, "Vector2", value);
        ImGui.Indent();
        ImGui.TextWrapped(description);
        ImGui.Unindent();
    }
}
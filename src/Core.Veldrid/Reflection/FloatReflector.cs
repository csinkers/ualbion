using System;
using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Reflection;

public class FloatReflector(string typeName, Func<object, float> toFloat) : IReflector
{
    const float DefaultMin = -10.0f;
    const float DefaultMax = 10.0f;
    public void Reflect(in ReflectorState state)
    {
        ImGui.Indent();
        var style = state.Meta?.Options?.Style ?? DiagEditStyle.Label;
        switch (style)
        {
            case DiagEditStyle.NumericInput: RenderInput(state); break;
            case DiagEditStyle.NumericSlider: RenderSlider(state); break;
            default: RenderLabel(state); break;
        }
        ImGui.Unindent();
    }

    void RenderSlider(in ReflectorState state)
    {
        if (state.Meta?.Options == null)
        {
            RenderLabel(state);
            return;
        }

        float value = toFloat(state.Target);
        var label = state.Meta.Name ?? state.Index.ToString();
        var options = state.Meta.Options;

        float min = DefaultMin;
        float max = DefaultMax;

        if (options.Min is float minFloat) min = minFloat;
        else if (options.MinProperty != null)
        {
            options.GetMinProperty ??= ReflectorUtil.BuildPropertyGetter(options.MinProperty, state.Parent.GetType());
            min = options.GetMinProperty(state.Parent) as float? ?? DefaultMin;
        }

        if (options.Max is float maxFloat) max = maxFloat;
        else if (options.MaxProperty != null)
        {
            options.GetMaxProperty ??= ReflectorUtil.BuildPropertyGetter(options.MaxProperty, state.Parent.GetType());
            max = options.GetMaxProperty(state.Parent) as float? ?? DefaultMax;
        }

        ImGui.TextUnformatted(label);
        ImGui.SameLine();
        if (ImGui.SliderFloat("##" + label, ref value, min, max))
            state.Meta.Setter(state, value);
    }

    void RenderInput(in ReflectorState state)
    {
        float value = toFloat(state.Target);
        var label = state.Meta.Name ?? state.Index.ToString();
        ImGui.TextUnformatted(label);
        ImGui.SameLine();
        if (ImGui.InputFloat("##" + label, ref value))
            state.Meta.Setter(state, value);
    }

    void RenderLabel(in ReflectorState state)
    {
        var description = ReflectorUtil.DescribePlain(state, typeName, state.Target);
        ImGui.TextUnformatted(description);
    }
}
using System;
using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Reflection;

public class IntReflector(string typeName, Func<object, int> toInt) : IReflector
{
    const int DefaultMin = -1024;
    const int DefaultMax = 1024;
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

        int value = toInt(state.Target);
        var label = state.Meta.Name ?? state.Index.ToString();
        var options = state.Meta.Options;

        int min = DefaultMin;
        int max = DefaultMax;

        if (options.Min is int minInt) min = minInt;
        else if (options.MinProperty != null)
        {
            options.GetMinProperty ??= ReflectorUtil.BuildPropertyGetter(options.MinProperty, state.Parent.GetType());
            min = options.GetMinProperty(state.Parent) as int? ?? 0;
        } 

        if (options.Max is int maxInt) max = maxInt;
        else if (options.MaxProperty != null)
        {
            options.GetMaxProperty ??= ReflectorUtil.BuildPropertyGetter(options.MaxProperty, state.Parent.GetType());
            max = options.GetMaxProperty(state.Parent) as int? ?? 0;
        } 

        ImGui.TextUnformatted(label);
        ImGui.SameLine();
        if (ImGui.SliderInt("##" + label, ref value, min, max))
            state.Meta.Setter(state, value);
    }

    void RenderInput(in ReflectorState state)
    {
        int value = toInt(state.Target);
        var label = state.Meta.Name ?? state.Index.ToString();
        ImGui.TextUnformatted(label);
        ImGui.SameLine();
        if (ImGui.InputInt("##" + label, ref value))
            state.Meta.Setter(state, value);
    }

    void RenderLabel(in ReflectorState state)
    {
        var description = ReflectorUtil.DescribePlain(state, typeName, state.Target);
        ImGui.TextUnformatted(description);
    }
}
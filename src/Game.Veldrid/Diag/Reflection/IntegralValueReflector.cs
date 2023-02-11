using System;
using System.Globalization;
using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Veldrid.Diag.Reflection;

public class IntegralValueReflector : IReflector
{
    readonly string _typeName;
    readonly Func<object, int> _toInt;

    public IntegralValueReflector(string typeName, Func<object, int> toInt)
    {
        _typeName = typeName;
        _toInt = toInt;
    }

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

        int value = _toInt(state.Target);
        var label = state.Meta.Name ?? state.Index.ToString(CultureInfo.InvariantCulture);
        var options = state.Meta.Options;
        ImGui.TextUnformatted(label);
        ImGui.SameLine();
        if (ImGui.SliderInt("##" + label, ref value, options.Min, options.Max))
            state.Meta.Setter(state, value);
    }

    void RenderInput(in ReflectorState state)
    {
        int value = _toInt(state.Target);
        var label = state.Meta.Name ?? state.Index.ToString(CultureInfo.InvariantCulture);
        ImGui.TextUnformatted(label);
        ImGui.SameLine();
        if (ImGui.InputInt("##" + label, ref value))
            state.Meta.Setter(state, value);
    }

    void RenderLabel(in ReflectorState state)
    {
        var description = ReflectorUtil.Describe(state, _typeName, state.Target);
        ImGui.TextUnformatted(description);
    }
}
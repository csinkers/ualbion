using System;
using ImGuiNET;

namespace UAlbion.Game.Veldrid.Diag;

public class ValueReflector
{
    readonly string _typeName;
    readonly Func<object, object> _toValue;

    ValueReflector(string typeName, Func<object, object> toValue)
    {
        _typeName = typeName;
        _toValue = toValue ?? (x => x);
    }

    public static Reflector Build(string typeName) => new ValueReflector(typeName, null).Render;
    public static Reflector Build(string typeName, Func<object, object> toValue) => new ValueReflector(typeName, toValue).Render;

    void Render(in ReflectorState state)
    {
        var value = _toValue(state.Target);
        var description = ReflectorUtil.Describe(state, _typeName, value);
        ImGui.Indent();
        ImGui.TextWrapped(description);
        ImGui.Unindent();
    }
}
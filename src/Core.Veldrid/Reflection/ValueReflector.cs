﻿using System;
using ImGuiNET;

namespace UAlbion.Core.Veldrid.Reflection;

public class ValueReflector : IReflector
{
    readonly string _typeName;
    readonly Func<object, object> _toValue;

    public ValueReflector(string typeName, Func<object, object> toValue = null)
    {
        _typeName = typeName;
        _toValue = toValue ?? (x => x);
    }

    public void Reflect(in ReflectorState state)
    {
        var value = _toValue(state.Target);
        var description = ReflectorUtil.DescribePlain(state, _typeName, value);
        ImGui.Indent();
        ImGui.TextWrapped(description);
        ImGui.Unindent();
    }
}
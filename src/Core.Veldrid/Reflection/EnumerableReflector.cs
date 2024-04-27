﻿using System;
using System.Collections;
using ImGuiNET;

namespace UAlbion.Core.Veldrid.Reflection;

public class EnumerableReflector : IReflector
{
    readonly ReflectorMetadata _meta;
    readonly ReflectorManager _manager;
    readonly string _typeName;

    public EnumerableReflector(ReflectorManager manager, Type type)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _typeName = ReflectorUtil.BuildTypeName(type);
        _meta = new ReflectorMetadata(null, Getter, Setter, null);
    }

    static object Getter(in ReflectorState state)
    {
        switch (state.Parent)
        {
            case IList coll: return coll[state.Index];
            case IEnumerable enumerable:
            {
                int index = 0;
                foreach (var item in enumerable)
                    if (index++ == state.Index)
                        return item;
                break;
            }
        }

        return null;
    }

    static void Setter(in ReflectorState state, object value)
    {
        if (state.Parent is not IList list)
            return;

        list[state.Index] = value;
    }

    public void Reflect(in ReflectorState state)
    {
        var value = state.Target is ICollection coll ? $"{coll.Count}" : "<...>";
        var description = ReflectorUtil.Describe(state, _typeName, value);
        bool treeOpen = ImGui.TreeNodeEx(description, ImGuiTreeNodeFlags.AllowOverlap);
        if (!treeOpen) return;

        var meta = _meta;
        if (state.Meta?.Options != null)
        {
            meta = ReflectorUtil.GetAuxiliaryState(
                state,
                "EnumMeta",
                s => new ReflectorMetadata(null, Getter, Setter, s.Meta.Options));
        }

        int index = 0;
        foreach (var child in (IEnumerable)state.Target)
        {
            var childState = new ReflectorState(child, state.Target, index, meta);
            var childReflector = _manager.GetReflectorForInstance(child);
            childReflector(childState);
            index++;
        }

        ImGui.TreePop();
    }
}

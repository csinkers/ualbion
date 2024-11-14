using System;
using System.Collections;
using ImGuiNET;

namespace UAlbion.Core.Veldrid.Reflection;

public class EnumerableReflector(ReflectorManager manager, Type type) : IReflector
{
    readonly ReflectorMetadata _meta = new(null, null, type, Getter, Setter, null);
    readonly string _typeName = ReflectorUtil.BuildTypeName(type);

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
        var description = ReflectorUtil.DescribeAsNodeId(state, _typeName, value);
        bool treeOpen = ImGui.TreeNodeEx(description, ImGuiTreeNodeFlags.AllowOverlap);
        if (!treeOpen) return;

        var meta = _meta;
        if (state.Meta?.Options != null)
        {
            meta = ReflectorUtil.GetAuxiliaryState(
                state,
                "EnumMeta",
                s => new ReflectorMetadata(null, null, null, Getter, Setter, s.Meta.Options));
        }

        int index = 0;
        foreach (var child in (IEnumerable)state.Target)
        {
            var childState = new ReflectorState(child, state.Target, index, meta);
            var childReflector = manager.GetReflectorForInstance(child);
            childReflector(childState);
            index++;
        }

        ImGui.TreePop();
    }
}

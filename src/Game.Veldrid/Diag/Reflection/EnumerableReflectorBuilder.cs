using System;
using System.Collections;
using System.Globalization;
using ImGuiNET;

namespace UAlbion.Game.Veldrid.Diag.Reflection;

public static class EnumerableReflectorBuilder
{
    public static Reflector Build(ReflectorManager manager, Type type)
    {
        var typeName = ReflectorUtil.BuildTypeName(type);
        Func<object, string> getValueFunc = typeof(ICollection).IsAssignableFrom(type) ? CollectionGetValue : DummyGetValue;

        return (in ReflectorState state) =>
        {
            var value = getValueFunc(state.Target);
            var description = ReflectorUtil.Describe(state, typeName, value);
            bool treeOpen = ImGui.TreeNodeEx(description, ImGuiTreeNodeFlags.AllowItemOverlap);
            if (!treeOpen)
                return;

            int index = 0;
            foreach (var child in (IEnumerable)state.Target)
            {
                var childState = new ReflectorState(child, state.Target, index, null);
                var childReflector = manager.GetReflectorForInstance(child);
                childReflector(childState);
                index++;
            }

            ImGui.TreePop();
        };
    }

    static string DummyGetValue(object x) => x == null ? "null" : "<...>";
    static string CollectionGetValue(object x)
    {
        var e = (ICollection)x;
        return e.Count.ToString(CultureInfo.InvariantCulture);
    }
}
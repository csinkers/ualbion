using System;
using System.Collections;
using System.Globalization;
using ImGuiNET;
using UAlbion.Formats;

namespace UAlbion.Game.Veldrid.Diag;

public static class EnumerableReflectorBuilder
{
    public static Reflector Build(ReflectorManager manager, Type type)
    {
        var typeName = ReflectorManager.BuildTypeName(type);
        Func<object, string> getValueFunc = typeof(ICollection).IsAssignableFrom(type) ? CollectionGetValue : DummyGetValue;

        return (in ReflectorState state) =>
        {
            var value = getValueFunc(state.Target);
            var description =
                state.Meta?.Name == null
                    ? $"{value} ({typeName})"
                    : $"{state.Meta.Name}: {value} ({typeName})";

            description = FormatUtil.WordWrap(description, 120);
            bool treeOpen = ImGui.TreeNodeEx(description, ImGuiTreeNodeFlags.AllowItemOverlap);
            if (treeOpen)
            {
                int index = 0;
                foreach (var child in (IEnumerable)state.Target)
                {
                    // var name = index.ToString(CultureInfo.InvariantCulture);
                    var childState = new ReflectorState(child, state.Target, index, null);
                    var childReflector = manager.GetReflectorForInstance(child);
                    childReflector(childState);
                    index++;
                }

                ImGui.TreePop();
            }
        };
    }

    static string DummyGetValue(object x) => x == null ? "null" : "<...>";
    static string CollectionGetValue(object x)
    {
        var e = (ICollection)x;
        return e.Count.ToString(CultureInfo.InvariantCulture);
    }
}
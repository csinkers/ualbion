using System;
using System.Collections;
using System.Globalization;
using ImGuiNET;

namespace UAlbion.Game.Veldrid.Diag.Reflection;

public class EnumerableReflector : IReflector
{
    readonly ReflectorManager _manager;
    readonly Func<object, string> _getValueFunc;
    readonly string _typeName;

    public EnumerableReflector(ReflectorManager manager, Type type)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _typeName = ReflectorUtil.BuildTypeName(type);
        _getValueFunc = typeof(ICollection).IsAssignableFrom(type) 
            ? CollectionGetValue 
            : DummyGetValue;
    }

    public void Reflect(in ReflectorState state)
    {
        var value = _getValueFunc(state.Target);
        var description = ReflectorUtil.Describe(state, _typeName, value);
        bool treeOpen = ImGui.TreeNodeEx(description, ImGuiTreeNodeFlags.AllowItemOverlap);
        if (!treeOpen) return;

        int index = 0;
        foreach (var child in (IEnumerable)state.Target)
        {
            var childState = new ReflectorState(child, state.Target, index, null);
            var childReflector = _manager.GetReflectorForInstance(child);
            childReflector(childState);
            index++;
        }

        ImGui.TreePop();
    }

    static string DummyGetValue(object x) => x == null ? "null" : "<...>";
    static string CollectionGetValue(object x)
    {
        var e = (ICollection)x;
        return e.Count.ToString(CultureInfo.InvariantCulture);
    }
}
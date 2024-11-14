using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Reflection;

public class ObjectReflector : IReflector
{
    // For ignoring fields on built-in types where we can't just add a [DiagIgnore] attribute.
    static readonly Dictionary<string, string[]> MembersToIgnore = new()
    {
        // Type starting with:
        // * = matches anything
        // ~ = contains
        // ! = full type name
        // > = starts with
        // anything else = exact short name match

        { "*", ["~__BackingField", "_syncRoot"] }, // ignore backing fields, lock objects etc

        // These often throw exceptions and cripple performance
        { "MemberInfo",  ["DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition"] },
        { "Type",        ["DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition"] },
        { "TypeInfo",    ["DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition"] },
        { "RuntimeType", ["DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition"] },
    };

    readonly ReflectorManager _manager;
    readonly ReflectorMetadataStore _store;
    readonly ReflectorMetadata[] _subObjects;
    readonly string _typeName;

    public ObjectReflector(ReflectorManager manager, ReflectorMetadataStore store, Type type)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        ArgumentNullException.ThrowIfNull(type);

        _typeName = ReflectorUtil.BuildTypeName(type);
        var subObjects = new Dictionary<string, ReflectorMetadata>();
        PopulateMembers(type, subObjects);

        _subObjects = subObjects
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToArray();
    }

    static readonly List<string> EditButtonLabels = [];
    public void Reflect(in ReflectorState state)
    {
        var description = ReflectorUtil.DescribeAsNodeId(state, _typeName, state.Target);
        bool treeOpen = ImGui.TreeNodeEx(description, ImGuiTreeNodeFlags.AllowOverlap);
        if (!treeOpen)
            return;

        for (var index = 0; index < _subObjects.Length; index++)
        {
            var subObject = _subObjects[index];
            var child = subObject.Getter(state);
            var childState = new ReflectorState(child, state.Target, -1, subObject);
            var childReflector = _manager.GetReflectorForInstance(child);

            DrawEditButton(index, subObject);
            childReflector(childState);
        }

        ImGui.TreePop();
    }

    public void ReflectComponent(in ReflectorState state)
    {
        var component = (Component)state.Target;
        var description = ReflectorUtil.DescribeAsNodeId(state, _typeName, component);
        Vector4 color = GetComponentColor(component);
        ImGui.PushStyleColor(0, color);
        bool treeOpen = ImGui.TreeNodeEx(description, ImGuiTreeNodeFlags.AllowOverlap);
        ImGui.PopStyleColor();

        if (!treeOpen)
            return;

        /*
        if (component is IUiElement element)
        {
            var snapshot = Resolve<ILayoutManager>().LastSnapshot;
            if (!snapshot.TryGetValue(element, out var node))
                return;

            ImGui.Text($"UI {node.Extents} {node.Order}");
        }
        */

        for (var index = 0; index < _subObjects.Length; index++)
        {
            var subObject = _subObjects[index];
            var child = subObject.Getter(state);
            var childState = new ReflectorState(child, state.Target, -1, subObject);
            var childReflector = _manager.GetReflectorForInstance(child);

            DrawEditButton(index, subObject);
            childReflector(childState);
        }

        ImGui.TreePop();
    }

    void DrawEditButton(int index, ReflectorMetadata subObject)
    {
        if (!_manager.IsEditMode)
            return;

        while (EditButtonLabels.Count <= index)
            EditButtonLabels.Add($"##{EditButtonLabels.Count}");

        var label = EditButtonLabels[index];
        bool selected = _manager.EditTarget == subObject;

        if (selected)
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.One);

        if (ImGui.Button(label, new Vector2(12, 12)))
            _manager.EditTarget = subObject;

        if (selected)
            ImGui.PopStyleColor();

        ImGui.SameLine();
    }

    static Vector4 GetComponentColor(Component component) =>
        component.IsActive
            ? component.IsSubscribed
                ? new Vector4(0.4f, 0.9f, 0.4f, 1)
                : new Vector4(0.6f, 0.6f, 0.6f, 1)
            : new Vector4(1.0f, 0.6f, 0.6f, 1);

    void PopulateMembers(Type type, Dictionary<string, ReflectorMetadata> subObjects)
    {
        const BindingFlags publicFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        const BindingFlags privateFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        if (type.BaseType != null)
            PopulateMembers(type.BaseType, subObjects);

        List<string> ignoreList = [];
        foreach (var kvp in MembersToIgnore)
        {
            bool isMatch = kvp.Key[0] switch
            {
                '*' => true,
                '~' => type.Name.Contains(kvp.Key[1..], StringComparison.Ordinal),
                '!' => type.FullName?.Equals(kvp.Key[1..], StringComparison.Ordinal) ?? false,
                '>' => type.Name.StartsWith(kvp.Key[1..], StringComparison.Ordinal),
                _ => type.Name.Equals(kvp.Key, StringComparison.Ordinal)
            };

            if (!isMatch)
                continue;

            if (kvp.Value == null)
                return; // Suppress the whole type?

            ignoreList.AddRange(kvp.Value);
        }

        var publicProperties = type.GetProperties(publicFlags);
        var privateProperties = type.GetProperties(privateFlags);
        var publicFields = type.GetFields(publicFlags);
        var privateFields = type.GetFields(privateFlags);

        foreach (var prop in publicProperties)
        {
            var meta = _store.GetPropertyMetadata(type, prop, ignoreList);
            if (meta != null)
                subObjects["A|" + prop.Name] = meta;
        }

        foreach (var field in publicFields)
        {
            var meta = _store.GetFieldMetadata(type, field, ignoreList);
            if (meta != null)
                subObjects["A|" + field.Name] = meta;
        }

        foreach (var prop in privateProperties)
        {
            var meta = _store.GetPropertyMetadata(type, prop, ignoreList);
            if (meta != null)
                subObjects["B|" + prop.Name] = meta;
        }

        foreach (var field in privateFields)
        {
            var meta = _store.GetFieldMetadata(type, field, ignoreList);
            if (meta != null)
                subObjects["B|" + field.Name] = meta;
        }
    }
}
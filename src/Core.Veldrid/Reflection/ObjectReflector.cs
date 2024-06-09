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
    readonly ReflectorManager _manager;

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
        { "MemberInfo", ["DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition"] },
        { "Type", ["DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition"] },
        { "TypeInfo", ["DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition"] },
        { "RuntimeType", ["DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition"] },
    };

    readonly string _typeName;
    readonly ReflectorMetadata[] _subObjects;

    public ObjectReflector(ReflectorManager manager, Type type)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        ArgumentNullException.ThrowIfNull(type);

        _typeName = ReflectorUtil.BuildTypeName(type);
        var subObjects = new Dictionary<string, ReflectorMetadata>();
        PopulateMembers(type, subObjects);

        _subObjects = subObjects
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToArray();
    }

    public void Reflect(in ReflectorState state)
    {
        var description = ReflectorUtil.Describe(state, _typeName, state.Target);
        bool treeOpen = ImGui.TreeNodeEx(description, ImGuiTreeNodeFlags.AllowOverlap);
        if (!treeOpen)
            return;

        foreach (var subObject in _subObjects)
        {
            var child = subObject.Getter(state);
            var childState = new ReflectorState(child, state.Target, -1, subObject);
            var childReflector = _manager.GetReflectorForInstance(child);
            childReflector(childState);
        }

        ImGui.TreePop();
    }

    public void ReflectComponent(in ReflectorState state)
    {
        var component = (Component)state.Target;
        var description = ReflectorUtil.Describe(state, _typeName, component);
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

        foreach (var subObject in _subObjects)
        {
            var child = subObject.Getter(state);
            var childState = new ReflectorState(child, state.Target, -1, subObject);
            var childReflector = _manager.GetReflectorForInstance(child);
            childReflector(childState);
        }

        ImGui.TreePop();
    }

    static Vector4 GetComponentColor(Component component) =>
        component.IsActive
            ? component.IsSubscribed
                ? new Vector4(0.4f, 0.9f, 0.4f, 1)
                : new Vector4(0.6f, 0.6f, 0.6f, 1)
            : new Vector4(1.0f, 0.6f, 0.6f, 1);

    static void PopulateMembers(Type type, Dictionary<string, ReflectorMetadata> subObjects)
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
            var meta = BuildPropertyMetadata(prop, ignoreList);
            if (meta != null)
                subObjects["A|" + prop.Name] = meta;
        }

        foreach (var field in publicFields)
        {
            var meta = BuildFieldMetadata(field, ignoreList);
            if (meta != null)
                subObjects["A|" + field.Name] = meta;
        }

        foreach (var prop in privateProperties)
        {
            var meta = BuildPropertyMetadata(prop, ignoreList);
            if (meta != null)
                subObjects["B|" + prop.Name] = meta;
        }

        foreach (var field in privateFields)
        {
            var meta = BuildFieldMetadata(field, ignoreList);
            if (meta != null)
                subObjects["B|" + field.Name] = meta;
        }
    }

    static ReflectorMetadata BuildPropertyMetadata(PropertyInfo prop, List<string> ignoreList)
    {
        if (prop.GetIndexParameters().Length > 0)
            return null; 

        if (IsMemberIgnored(ignoreList, prop.Name, prop.GetCustomAttributes(), out var options))
            return null;

        return new ReflectorMetadata(
            prop.Name,
            BuildPropertyGetter(prop),
            BuildPropertySetter(prop),
            options);
    }

    static ReflectorMetadata BuildFieldMetadata(FieldInfo field, List<string> ignoreList)
    {
        if (IsMemberIgnored(ignoreList, field.Name, field.GetCustomAttributes(), out var options))
            return null;

        return new ReflectorMetadata(
            field.Name,
            BuildFieldGetter(field),
            BuildFieldSetter(field),
            options);
    }

    static bool IsMemberIgnored(
        List<string> ignoreList,
        string name,
        IEnumerable<Attribute> customAttributes,
        out DiagEditAttribute options)
    {
        options = null;
        bool isIgnored = false;
        foreach (var attrib in customAttributes)
        {
            if (attrib is DiagEditAttribute editAttribute)
                options = editAttribute;

            if (attrib is DiagIgnoreAttribute)
                isIgnored = true;
        }

        if (isIgnored)
            return true;

        foreach (var entry in ignoreList)
        {
            switch (entry[0])
            {
                case '*': return true;
                case '~': if (name.Contains(entry[1..], StringComparison.Ordinal)) return true; break;
                case '>': if (name.StartsWith(entry[1..], StringComparison.Ordinal)) return true; break;
                default: if (name.Equals(entry, StringComparison.Ordinal)) return true; break;
            }
        }

        return false;
    }

    static void NullSetter(in ReflectorState _, object value) { }
    static ReflectorGetter NameGetter(PropertyInfo prop) => (in ReflectorState _) => prop.PropertyType.Name;
    static ReflectorGetter NameGetter(FieldInfo field) => (in ReflectorState _) => field.FieldType.Name;

    static ReflectorGetter BuildFieldGetter(FieldInfo field)
    {
        if (field.FieldType.Name.StartsWith("Span", StringComparison.Ordinal))
            return NameGetter(field);
        if (field.FieldType.Name.StartsWith("ReadOnlySpan", StringComparison.Ordinal))
            return NameGetter(field);
        return (in ReflectorState state) => GetFieldSafe(field, state.Target);
    }

    static ReflectorGetter BuildPropertyGetter(PropertyInfo prop)
    {
        if (prop.PropertyType.Name.StartsWith("Span", StringComparison.Ordinal)) return NameGetter(prop);
        if (prop.PropertyType.Name.StartsWith("ReadOnlySpan", StringComparison.Ordinal)) return NameGetter(prop);
        if (!prop.CanRead) return (in ReflectorState _) => prop.PropertyType.Name;
        return (in ReflectorState state) => GetPropertySafe(prop, state.Target);
    }

    static ReflectorSetter BuildFieldSetter(FieldInfo field)
    {
        if (field.FieldType.Name.StartsWith("Span", StringComparison.Ordinal)) return NullSetter;
        if (field.FieldType.Name.StartsWith("ReadOnlySpan", StringComparison.Ordinal)) return NullSetter;
        return (in ReflectorState state, object value) => SetFieldSafe(field, state.Parent, value);
    }

    static ReflectorSetter BuildPropertySetter(PropertyInfo prop)
    {
        if (prop.PropertyType.Name.StartsWith("Span", StringComparison.Ordinal)) return NullSetter;
        if (prop.PropertyType.Name.StartsWith("ReadOnlySpan", StringComparison.Ordinal)) return NullSetter;
        if (!prop.CanWrite) return NullSetter;
        return (in ReflectorState state, object value) => SetPropertySafe(prop, state.Parent, value);
    }

    static object GetPropertySafe(PropertyInfo x, object o)
    {
        try { return x.GetValue(o); }
        catch (TargetException e) { return e; }
        catch (TargetParameterCountException e) { return e; }
        catch (NotSupportedException e) { return e; }
        catch (MethodAccessException e) { return e; }
        catch (TargetInvocationException e) { return e; }
    }

    static object GetFieldSafe(FieldInfo x, object o)
    {
        try { return x.GetValue(o); }
        catch (TargetException e) { return e; }
        catch (NotSupportedException e) { return e; }
        catch (FieldAccessException e) { return e; }
        catch (ArgumentException e) { return e; }
    }

    static void SetPropertySafe(PropertyInfo x, object o, object value)
    {
        try { x.SetValue(o, value); }
        catch (TargetException) { }
        catch (TargetParameterCountException) { }
        catch (NotSupportedException) { }
        catch (MethodAccessException) { }
        catch (TargetInvocationException) { }
    }

    static void SetFieldSafe(FieldInfo x, object o, object value)
    {
        try { x.SetValue(o, value); }
        catch (TargetException) { }
        catch (NotSupportedException) { }
        catch (FieldAccessException) { }
        catch (ArgumentException) { }
    }
}
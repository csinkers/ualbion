using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Diag;

public class ObjectTypeReflectorBuilder : IReflectorBuilder
{
    public static ObjectTypeReflectorBuilder Instance { get; } = new();
    ObjectTypeReflectorBuilder() { }

    record SubObjectGetter(string Name, Func<object, object> Getter);

    static readonly Dictionary<string, string[]> MembersToIgnore = new()
    {
        // Type starting with:
        //     * = matches anything
        //     ~ = "contains"
        //     ! = "full type name"
        //     > = "starts with"
        //     none = exact short name match
        { "*", new [] { "~__BackingField", "_syncRoot" } },
        { "MemberInfo", new[] { "DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition" } },
        { "Type", new[] { "DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition" } },
        { "TypeInfo", new[] { "DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition" } },
        { "RuntimeType", new[] { "DeclaringMethod", "GenericParameterAttributes", "GenericParameterPosition" } },
    };

    public Reflector Build(ReflectorManager manager, string name, Type type)
    {
        // Generic object handling
        if (manager == null) throw new ArgumentNullException(nameof(manager));
        if (type == null) throw new ArgumentNullException(nameof(type));

        var subObjects = new Dictionary<string, SubObjectGetter>();
        PopulateMembers(type, subObjects);

        SubObjectGetter[] subObjectArray = subObjects
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToArray();

        IEnumerable<ReflectorState> VisitChildren(object target)
        {
            if (target == null) yield break;
            foreach (var subObject in subObjectArray)
            {
                var child = subObject.Getter(target);
                var childReflector = manager.GetReflectorForInstance(child);
                yield return new ReflectorState(subObject.Name, child, childReflector, target, -1);
            }
        }

        return new Reflector(name, null, null, VisitChildren);
    }

    static void PopulateMembers(Type type, Dictionary<string, SubObjectGetter> subObjects)
    {
        const BindingFlags publicFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        const BindingFlags privateFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        if (type.BaseType != null)
            PopulateMembers(type.BaseType, subObjects);

        List<string> ignoreList = new();
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
            if (prop.GetIndexParameters().Length > 0) continue; 
            if (IsMemberIgnored(ignoreList, prop.Name, prop.GetCustomAttributes())) continue; 
            var getter = new SubObjectGetter(prop.Name, BuildPropertyGetter(prop));
            subObjects["A|" + prop.Name] = getter;
        }

        foreach (var field in publicFields)
        {
            if (IsMemberIgnored(ignoreList, field.Name, field.GetCustomAttributes())) continue;
            var getter = new SubObjectGetter(field.Name, BuildFieldGetter(field));
            subObjects["A|" + field.Name] = getter;
        }

        foreach (var prop in privateProperties)
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            if (IsMemberIgnored(ignoreList, prop.Name, prop.GetCustomAttributes())) continue;
            var getter = new SubObjectGetter(prop.Name, BuildPropertyGetter(prop));
            subObjects["B|" + prop.Name] = getter;
        }

        foreach (var field in privateFields)
        {
            if (IsMemberIgnored(ignoreList, field.Name, field.GetCustomAttributes())) continue;
            var getter = new SubObjectGetter(field.Name, BuildFieldGetter(field));
            subObjects["B|" + field.Name] = getter;
        }
    }

    static bool IsMemberIgnored(List<string> ignoreList, string name, IEnumerable<Attribute> customAttributes)
    {
        foreach (var attrib in customAttributes)
            if (attrib is DiagIgnoreAttribute)
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

    static Func<object, object> BuildFieldGetter(FieldInfo field)
    {
        if (field.FieldType.Name.StartsWith("Span", StringComparison.Ordinal)) return _ => field.FieldType.Name;
        if (field.FieldType.Name.StartsWith("ReadOnlySpan", StringComparison.Ordinal)) return _ => field.FieldType.Name;
        return x => GetFieldSafe(field, x);
    }

    static Func<object, object> BuildPropertyGetter(PropertyInfo prop)
    {
        if (prop.PropertyType.Name.StartsWith("Span", StringComparison.Ordinal)) return _ => prop.PropertyType.Name;
        if (prop.PropertyType.Name.StartsWith("ReadOnlySpan", StringComparison.Ordinal)) return _ => prop.PropertyType.Name;
        return x => GetPropertySafe(prop, x);
    }

    static object GetPropertySafe(PropertyInfo x, object o)
    {
        try
        {
            return !x.CanRead ? "<< No Getter! >>" : x.GetValue(o);
        }
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
}
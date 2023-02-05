using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UAlbion.Game.Diag;

public class TypeReflector
{
    public delegate IEnumerable<ReflectorState> VisitChildrenDelegate(object target);
    public delegate string GetValueDelegate(object target);
    record SubObjectGetter(string Name, Func<object, object> Getter);

    readonly Reflector _manager;
    readonly Type _type;
    SubObjectGetter[] _subObjects;

    public GetValueDelegate GetValue { get; private set; } = DefaultGetValue;
    public VisitChildrenDelegate SubObjects { get; private set; } = NoChildren;
    public bool HasSubObjects => SubObjects != NoChildren;
    public string TypeName { get; }
    public override string ToString() => $"{_type.Name} reflector";

    public TypeReflector(Reflector manager, string name, Type type, GetValueDelegate getValue)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _type = type;
        TypeName = name;
        GetValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
    }

    public TypeReflector(Reflector manager, Type type)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _type = type ?? throw new ArgumentNullException(nameof(type));
        TypeName = BuildTypeName(type);
    }

    public void Reflect()
    {
        if (typeof(Enum).IsAssignableFrom(_type))
            return;

        if (typeof(ICollection).IsAssignableFrom(_type))
        {
            GetValue = x =>
            {
                if (x == null) return "null";
                var e = (ICollection)x;
                return e.Count.ToString(CultureInfo.InvariantCulture);
            };

            SubObjects = VisitEnumerable;
            return;
        }

        if (typeof(IEnumerable).IsAssignableFrom(_type))
        {
            GetValue = x => x == null ?"null" : "<...>";
            SubObjects = VisitEnumerable;
            return;
        }

        // Generic object handling
        var subObjects = new Dictionary<string, SubObjectGetter>();
        PopulateMembers(_type, subObjects);

        _subObjects = subObjects
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToArray();
        SubObjects = VisitObject;
    }

    static void PopulateMembers(Type type, Dictionary<string, SubObjectGetter> subObjects)
    {
        const BindingFlags publicFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        const BindingFlags privateFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        if (type.BaseType != null)
            PopulateMembers(type.BaseType, subObjects);

        var publicProperties = type.GetProperties(publicFlags);
        var privateProperties = type.GetProperties(privateFlags);
        var publicFields = type.GetFields(publicFlags);
        var privateFields = type.GetFields(privateFlags);

        foreach (var prop in publicProperties)
        {
            if (prop.GetIndexParameters().Length > 0)
                continue;

            var getter = new SubObjectGetter(prop.Name, BuildPropertyGetter(prop));
            subObjects["A|" + prop.Name] = getter;
        }

        foreach (var field in publicFields)
        {
            var getter = new SubObjectGetter(field.Name, BuildFieldGetter(field));
            subObjects["A|" + field.Name] = getter;
        }

        foreach (var prop in privateProperties)
        {
            if (prop.GetIndexParameters().Length > 0)
                continue;

            var getter = new SubObjectGetter(prop.Name, BuildPropertyGetter(prop));
            subObjects["B|" + prop.Name] = getter;
        }

        foreach (var field in privateFields)
        {
            if (field.Name.Contains("__BackingField", StringComparison.InvariantCulture))
                continue;

            var getter = new SubObjectGetter(field.Name, BuildFieldGetter(field));
            subObjects["B|" + field.Name] = getter;
        }
    }

    static Func<object, object> BuildFieldGetter(FieldInfo field)
    {
        if (field.FieldType.Name.Contains("Span", StringComparison.Ordinal))
            return x => "Span";
        return x => GetFieldSafe(field, x);
    }

    static Func<object, object> BuildPropertyGetter(PropertyInfo prop)
    {
        if (prop.PropertyType.Name.Contains("Span", StringComparison.Ordinal))
            return x => "Span";
        return x => GetPropertySafe(prop, x);
    }

    static string DefaultGetValue(object target) => target?.ToString() ?? "null";
    static IEnumerable<ReflectorState> NoChildren(object target) => Enumerable.Empty<ReflectorState>();
    IEnumerable<ReflectorState> VisitObject(object target)
    {
        if (target == null) yield break;
        foreach (var subObject in _subObjects)
        {
            var child = subObject.Getter(target);
            var reflector = _manager.GetReflectorForInstance(child);
            yield return new ReflectorState(subObject.Name, child, reflector, target, -1);
        }
    }

    IEnumerable<ReflectorState> VisitEnumerable(object target)
    {
        if (target == null) yield break;
        int index = 0;
        foreach (var child in (IEnumerable)target)
        {
            var childReflector = _manager.GetReflectorForInstance(child);
            yield return new ReflectorState(index.ToString(CultureInfo.InvariantCulture), child, childReflector, target, index);
            index++;
        }
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

    static string BuildTypeName(Type type)
    {
        if (type == null)
            return "null";

        var generic = type.GetGenericArguments();
        if (generic.Length == 0)
            return type.Name;

        int index = type.Name.IndexOf('`', StringComparison.Ordinal);
        if (index == -1)
            return type.Name;

        var sb = new StringBuilder();
        sb.Append(type.Name[..index]);
        sb.Append('<');
        bool first = true;
        foreach (var arg in generic)
        {
            if (!first)
                sb.Append(", ");

            sb.Append(BuildTypeName(arg));
            first = false;
        }

        sb.Append('>');
        return sb.ToString();
    }
}
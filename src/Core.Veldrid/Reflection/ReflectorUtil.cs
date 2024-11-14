using System;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace UAlbion.Core.Veldrid.Reflection;

public static class ReflectorUtil
{
    public static string NameText(in ReflectorState state) =>
        state.Meta.Name != null
            ? $"{state.Meta.Name}: "
            : state.Index == -1
                ? ""
                : $"{state.Index}: ";

    public static string DescribeAsNodeId(in ReflectorState state, string typeName, object target)
    {
        target ??= state.Target;
        var description =
            state.Meta?.Name != null
                ? $"{state.Meta.Name}: {target} ({typeName})###{state.Meta.Name}"
                : state.Index == -1
                    ? $"{target} ({typeName})"
                    : $"{state.Index}: {target} ({typeName})###{state.Index}";

        return CoreUtil.WordWrap(description, 120);
    }

    public static string DescribePlain(in ReflectorState state, string typeName, object target)
    {
        target ??= state.Target;
        var description =
            state.Meta?.Name != null
                ? $"{state.Meta.Name}: {target} ({typeName})"
                : state.Index == -1
                    ? $"{target} ({typeName})"
                    : $"{state.Index}: {target} ({typeName})";

        return CoreUtil.WordWrap(description, 120);
    }

    public static string BuildTypeName(Type type)
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

    static readonly object SyncRoot = new();
    static readonly AuxiliaryReflectorStateCache AuxState = new();
    public static void SwapAuxiliaryState()
    {
        lock (SyncRoot)
            AuxState.Swap();
    }

    public static T GetAuxiliaryState<T>(in ReflectorState state, string type, Func<ReflectorState, T> auxStateBuilder)
    {
        ArgumentNullException.ThrowIfNull(auxStateBuilder);

        lock (SyncRoot)
        {
            var result = AuxState.Get(state, type);
            if (result == null)
            {
                result = auxStateBuilder(state);
                AuxState.Set(state, type, result);
            }

            return (T)result;
        }
    }

    public static Func<object, object> BuildPropertyGetter(string propertyName, Type type)
    {
        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        var parameter = Expression.Parameter(typeof(object), "target");
        var stronglyTypedParameter = Expression.Convert(parameter, type);
        var property = Expression.Property(stronglyTypedParameter, propertyName);
        var castResult = Expression.Convert(property, typeof(object));

        var lambda = Expression.Lambda<Func<object, object>>(castResult, parameter);
        return lambda.Compile();
    }

    static void NullSetter(in ReflectorState _, object value) { }
    static ReflectorGetter NameGetter(PropertyInfo prop) => (in ReflectorState _) => prop.PropertyType.Name;
    static ReflectorGetter NameGetter(FieldInfo field) => (in ReflectorState _) => field.FieldType.Name;

    public static ReflectorGetter BuildSafeFieldGetter(FieldInfo field)
    {
        if (field.FieldType.Name.StartsWith("Span", StringComparison.Ordinal))
            return NameGetter(field);
        if (field.FieldType.Name.StartsWith("ReadOnlySpan", StringComparison.Ordinal))
            return NameGetter(field);

        return (in ReflectorState state) => GetFieldSafe(field, state.Target);
    }

    public static ReflectorGetter BuildSafePropertyGetter(PropertyInfo prop)
    {
        if (prop.PropertyType.Name.StartsWith("Span", StringComparison.Ordinal)) return NameGetter(prop);
        if (prop.PropertyType.Name.StartsWith("ReadOnlySpan", StringComparison.Ordinal)) return NameGetter(prop);
        if (!prop.CanRead) return (in ReflectorState _) => prop.PropertyType.Name;
        return (in ReflectorState state) => GetPropertySafe(prop, state.Target);
    }

    public static ReflectorSetter BuildSafeFieldSetter(FieldInfo field)
    {
        if (field.FieldType.Name.StartsWith("Span", StringComparison.Ordinal)) return NullSetter;
        if (field.FieldType.Name.StartsWith("ReadOnlySpan", StringComparison.Ordinal)) return NullSetter;
        return (in ReflectorState state, object value) => SetFieldSafe(field, state.Parent, value);
    }

    public static ReflectorSetter BuildSafePropertySetter(PropertyInfo prop)
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
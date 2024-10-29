using System;
using System.Text;
using System.Linq.Expressions;

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
}
using System;
using System.Text;

namespace UAlbion.Core.Veldrid.Reflection;

public static class ReflectorUtil
{
    public static string NameText(in ReflectorState state) =>
        state.Meta.Name != null
            ? $"{state.Meta.Name}: "
            : state.Index == -1
                ? ""
                : $"{state.Index}: ";

    public static string Describe(in ReflectorState state, string typeName, object target)
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
        if (auxStateBuilder == null) throw new ArgumentNullException(nameof(auxStateBuilder));

        lock (AuxState)
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
}
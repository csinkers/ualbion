using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;

namespace UAlbion.Core;

public class VarRegistry : ServiceComponent<IVarRegistry>, IVarRegistry
{
    // ReSharper disable once NotAccessedPositionalProperty.Local
    sealed record VarInfo(IVar Var, Type ValueType, Type OwningType);
    readonly Dictionary<string, VarInfo> _vars = [];

    public IEnumerable<IVar> Vars => _vars.Select(x => x.Value.Var).OrderBy(x => x.Key);
    public bool IsVarRegistered(string key) => _vars.ContainsKey(key);
    public void Clear() => _vars.Clear();

    public void Register(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var libraryProperty = type.GetProperty("Library", BindingFlags.Public | BindingFlags.Static);
        if (libraryProperty == null)
            throw new InvalidOperationException($"Expected var holder type \"{type}\" to contain a public static Library property of type VarLibrary");

        var library = (VarLibrary)libraryProperty.GetValue(null);
        if (library == null)
            throw new InvalidOperationException($"Expected non-null value from Library property on var holder type \"{type}\"");

        foreach(var v in library.Vars)
            RegisterVar(v, type);
    }

    void RegisterVar(IVar instance, Type owningType)
    {
        Type valueType = null;

        var varType = instance.GetType();
        foreach (var iface in varType.GetInterfaces())
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IVar<>))
                valueType = iface.GetGenericArguments()[0];

        if (_vars.TryGetValue(instance.Key, out var existing))
            throw new InvalidOperationException($"{owningType} tried to register a var with key \"{instance.Key}\", but {existing.OwningType} already registered a var with that path");

        _vars[instance.Key] = new VarInfo(instance, valueType, owningType);
    }
}
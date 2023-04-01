using System;
using System.Collections.Generic;
using System.Reflection;
using UAlbion.Api.Eventing;
using UAlbion.Api.Settings;

namespace UAlbion.Core;

public class VarRegistry : ServiceComponent<IVarRegistry>, IVarRegistry
{
    record VarInfo(IVar Var, Type ValueType, Type OwningType);
    readonly Dictionary<string, VarInfo> _vars = new();

    public bool IsVarRegistered(string key) => _vars.ContainsKey(key);
    public void Clear() => _vars.Clear();

    public void Register(Type type)
    {
        var subClasess = type.GetNestedTypes(BindingFlags.Public);
        foreach(var  sub in subClasess)
            Register(sub);

        var properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
        foreach (var property in properties)
        {
            if (property.PropertyType.IsAssignableTo(typeof(IVar)))
            {
                var instance = (IVar)property.GetValue(null);
                RegisterVar(instance, property.PropertyType, type);
            }
        }

        var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (field.FieldType.IsAssignableTo(typeof(IVar)))
            {
                var instance = (IVar)field.GetValue(null);
                RegisterVar(instance, field.FieldType, type);
            }
        }
    }

    void RegisterVar(IVar instance, Type varType, Type owningType)
    {
        Type valueType = null;

        foreach (var iface in varType.GetInterfaces())
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IVar<>))
                valueType = iface.GetGenericArguments()[0];

        if (_vars.TryGetValue(instance.Key, out var existing))
            throw new InvalidOperationException($"{owningType} tried to register a var with key \"{instance.Key}\", but {existing.OwningType} already registered a var with that path");

        _vars[instance.Key] = new VarInfo(instance, valueType, owningType);
    }
}
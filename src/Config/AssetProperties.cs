using System;
using System.Collections.Generic;
using System.Reflection;

namespace UAlbion.Config;

public class AssetProperties
{
    readonly Dictionary<Type, Dictionary<string, IAssetProperty>> _properties = []; // loader/container specific properties
    readonly Dictionary<string, IAssetProperty> _globalProperties = [];
    readonly AssetProperties _parent;

    public AssetProperties(AssetProperties parent) 
        => _parent = parent;

    public IAssetProperty GetGlobalProperty(string name)
    {
        _globalProperties.TryGetValue(name, out var globalProperty);
        return globalProperty;
    }

    public IAssetProperty GetProperty(string name, Type context)
    {
        if (context == null)
            return null;

        _properties.TryGetValue(context, out var dict);
        if (dict == null)
            return null;

        dict.TryGetValue(name, out var assetProperty);
        return assetProperty;
    }

    public void LoadAssetPropertiesFromType(bool loadGlobal, Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        var properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
        foreach (var property in properties)
        {
            if (!property.PropertyType.IsAssignableTo(typeof(IAssetProperty))) continue;
            var instance = (IAssetProperty)property.GetValue(null);
            if (instance == null) continue;

            if (loadGlobal)
                AddGlobalProperty(instance);
            else
                AddProperty(type, instance);
        }

        var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (!field.FieldType.IsAssignableTo(typeof(IAssetProperty))) continue;
            var instance = (IAssetProperty)field.GetValue(null);
            if (instance == null) continue;

            if (loadGlobal)
                AddGlobalProperty(instance);
            else
                AddProperty(type, instance);
        }
    }

    void AddProperty(Type type, IAssetProperty assetProperty)
    {
        _properties.TryGetValue(type, out var dict);
        if (dict == null)
        {
            dict = [];
            _properties[type] = dict;
        }

        if (dict.ContainsKey(assetProperty.Name))
            throw new InvalidOperationException($"Tried to registry property \"{assetProperty.Name}\" for type {type}, but another property is already registered with that name");

        if (_parent?.GetProperty(assetProperty.Name, type) != null)
            throw new InvalidOperationException($"Tried to registry property \"{assetProperty.Name}\" for type {type}, but another property is already registered with that name in an inherited mod's type config");

        dict[assetProperty.Name] = assetProperty;
    }

    void AddGlobalProperty(IAssetProperty assetProperty)
    {
        if (_globalProperties.ContainsKey(assetProperty.Name))
            throw new InvalidOperationException($"Tried to registry property \"{assetProperty.Name}\" globally, but another property is already registered with that name");

        if (_parent?._globalProperties.ContainsKey(assetProperty.Name) == true)
            throw new InvalidOperationException($"Tried to registry property \"{assetProperty.Name}\" globally, but another property is already registered with that name in an inherited mod's type config");

        _globalProperties[assetProperty.Name] = assetProperty;
    }
}
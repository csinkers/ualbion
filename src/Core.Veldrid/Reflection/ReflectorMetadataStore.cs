using System;
using System.Collections.Generic;
using System.Reflection;
using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Veldrid.Reflection;

public class ReflectorMetadataStore(IFileSystem disk, IJsonUtil jsonUtil, string settingsPath)
{
    readonly Dictionary<string, DiagEditAttribute> _defaults = []; // Keep track of which options were set via attributes in the code so we don't save redundant info.
    readonly Dictionary<string, DiagEditAttribute> _overrides = []; // This only contains overrides that were loaded from the settings file
    readonly Dictionary<(Type, string), ReflectorMetadata> _metadata = []; // Keep track of all the metadata used by ObjectReflector, because their Options could go from null to an override

    static string ToStringKey(Type type, string memberName) => $"{type.FullName}:{memberName}";
    static (Type, string)? FromStringKey(string key)
    {
        var parts = key.Split(':');
        if (parts.Length != 2)
            return null;

        Type type = null;
        try { type = Type.GetType(parts[0]); }
        catch (TypeLoadException) { }

        if (type == null)
            return null;

        return (type, parts[1]);
    }

    public void LoadOverrides()
    {
        if (!disk.FileExists(settingsPath))
            return;

        var json = disk.ReadAllText(settingsPath);
        var overrides = jsonUtil.Deserialize<Dictionary<string, DiagEditAttribute>>(json);

        _overrides.Clear();
        foreach (var (stringKey, options) in overrides)
            _overrides[stringKey] = options;
    }

    public void SaveOverrides()
    {
        var overridesToSave = new Dictionary<string, DiagEditAttribute>();

        // Persist any valid overrides that weren't ever actually reflected on during runtime
        foreach (var (stringKey, options) in _overrides)
        {
            var tuple = FromStringKey(stringKey);
            if (tuple == null)
                continue; // Discard any overrides for types that aren't loaded

            try
            {
                _ = tuple.Value.Item1.GetMember(tuple.Value.Item2);
            }
            catch (MissingMemberException)
            {
                continue; // Discard any overrides for members that have been renamed / removed
            }

            overridesToSave[stringKey] = options;
        }

        // Persist any overrides that were actually used
        foreach (var ((type, name), meta) in _metadata)
        {
            var stringKey = ToStringKey(type, name);
            if (meta.Options == null)
            {
                overridesToSave.Remove(stringKey); // If we reset the options back to defaults then don't restore the last value that was loaded.
                continue;
            }

            if (_defaults.TryGetValue(stringKey, out var defaultOptions) && meta.Options.IsEquivalentTo(defaultOptions))
                continue;

            overridesToSave[stringKey] = meta.Options;
        }

        var json = jsonUtil.Serialize(overridesToSave);
        disk.WriteAllText(settingsPath, json);
    }

    public ReflectorMetadata GetPropertyMetadata(Type type, PropertyInfo prop, List<string> ignoreList)
    {
        if (_metadata.TryGetValue((type, prop.Name), out var existingMetadata))
            return existingMetadata;

        if (prop.GetIndexParameters().Length > 0)
            return null;

        ReadAttributes(ignoreList, prop.Name, prop.GetCustomAttributes(), out var isIgnored, out var defaultOptions);
        if (isIgnored)
            return null;

        var stringKey = ToStringKey(type, prop.Name);
        _defaults[stringKey] = defaultOptions;

        if (!_overrides.TryGetValue(stringKey, out var options))
            options = defaultOptions?.Clone();

        var result = new ReflectorMetadata(
            prop.Name,
            type,
            prop.PropertyType,
            ReflectorUtil.BuildSafePropertyGetter(prop),
            ReflectorUtil.BuildSafePropertySetter(prop),
            options);

        _metadata[(type, prop.Name)] = result;
        return result;
    }

    public ReflectorMetadata GetFieldMetadata(Type type, FieldInfo field, List<string> ignoreList)
    {
        if (_metadata.TryGetValue((type, field.Name), out var existingMetadata))
            return existingMetadata;

        ReadAttributes(ignoreList, field.Name, field.GetCustomAttributes(), out bool isIgnored, out var defaultOptions);
        if (isIgnored)
            return null;

        var stringKey = ToStringKey(type, field.Name);
        _defaults[stringKey] = defaultOptions;

        if (!_overrides.TryGetValue(stringKey, out var options))
            options = defaultOptions?.Clone();

        var result = new ReflectorMetadata(
            field.Name,
            type,
            field.FieldType,
            ReflectorUtil.BuildSafeFieldGetter(field),
            ReflectorUtil.BuildSafeFieldSetter(field),
            options);

        _metadata[(type, field.Name)] = result;
        return result;
    }

    static void ReadAttributes(
        List<string> ignoreList,
        string name,
        IEnumerable<Attribute> customAttributes,
        out bool isIgnored,
        out DiagEditAttribute options)
    {
        options = null;
        isIgnored = false;

        foreach (var attrib in customAttributes)
        {
            if (attrib is DiagEditAttribute editAttribute)
                options = editAttribute;

            if (attrib is DiagIgnoreAttribute)
                isIgnored = true;
        }

        if (isIgnored)
            return;

        foreach (var entry in ignoreList)
        {
            switch (entry[0])
            {
                case '*': isIgnored = true; break;
                case '~': if (name.Contains(entry[1..], StringComparison.Ordinal)) isIgnored = true; break;
                case '>': if (name.StartsWith(entry[1..], StringComparison.Ordinal)) isIgnored = true; break;
                default: if (name.Equals(entry, StringComparison.Ordinal)) isIgnored = true; break;
            }
        }
    }
}
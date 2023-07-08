using System;
using System.Collections.Generic;

namespace UAlbion.Config;

public class TypeConfig // Defines the vocabulary to be used in assets.json files / AssetConfig
{
    readonly AssetMapping _mapping;

    public TypeConfig(string modName, AssetMapping mapping)
    {
        ModName = modName;
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
    }

    /// <summary>
    /// The name of the mod that this type config came from
    /// </summary>
    public string ModName { get; }

    /// <summary>
    /// A set of aliases for enums to be used as asset ids
    /// </summary>
    public IReadOnlyDictionary<string, AssetTypeInfo> IdTypes { get; init; }

    /// <summary>
    /// A set of supported natural languages that the player can choose to play the game in.
    /// </summary>
    public IReadOnlyDictionary<string, LanguageConfig> Languages { get; init; }

    /// <summary>
    /// A set of aliases for .NET types (deriving from IAssetLoader) to be used for loading assets
    /// </summary>
    public IReadOnlyDictionary<string, Type> Loaders { get; init; }

    /// <summary>
    /// A set of aliases for .NET types (deriving from IAssetContainer) to be used for extracting individual assets from container formats.
    /// </summary>
    public IReadOnlyDictionary<string, Type> Containers { get; init; }

    /// <summary>
    /// A set of aliases for .NET types (deriving from IAssetPostProcessor) to be used for additional post-load asset processing.
    /// </summary>
    public IReadOnlyDictionary<string, Type> PostProcessors { get; init; }

    /// <summary>
    /// The collection of types containing static IAssetProperty members to be registered
    /// </summary>
    public AssetProperties PropertyTypes { get; init; }

    /// <summary>
    /// The collection of types containing vars
    /// </summary>
    public List<string> VarTypes { get; init; } = new();

    public IAssetProperty GetAssetProperty(string name, AssetNode node)
    {
        var loader = node?.Loader;
        if (loader != null)
        {
            var loaderProperty = PropertyTypes.GetProperty(name, loader);
            if (loaderProperty != null)
                return loaderProperty;
        }

        var container = node?.Container;
        if (container != null)
        {
            var containerProperty = PropertyTypes.GetProperty(name, container);
            if (containerProperty != null)
                return containerProperty;
        }

        return PropertyTypes.GetGlobalProperty(name);
    }

    public AssetId ResolveId(string id)
    {
        if (string.IsNullOrEmpty(id))
            return AssetId.None;

        var (type, val) = SplitId(id, '.');
        if (val == null)
            throw new FormatException("Asset IDs should consist of an alias type and value, separated by a '.' character");
        var enumType = ResolveIdType(type);
        return _mapping.EnumToId(enumType, val);
    }

    static (string, string) SplitId(string id, char separator)
    {
        int index = id.IndexOf(separator, StringComparison.Ordinal);
        if (index == -1)
            return (id, null);

        var type = id[..index];
        var val = id[(index + 1)..];
        return (type, val);
    }

    Type ResolveIdType(string type)
    {
        if (!IdTypes.TryGetValue(type, out var assetType))
            throw new FormatException($"Could not resolve asset id alias \"{type}\"");

        var enumType = Type.GetType(assetType.EnumType);
        if (enumType == null)
            throw new FormatException($"Could not resolve type \"{assetType.EnumType}\" (alias \"{assetType.Alias}\")");

        return enumType;
    }

    public AssetRange ParseIdRange(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentNullException(nameof(s));

        int hyphenIndex = s.IndexOf('-', StringComparison.Ordinal);
        if (hyphenIndex == -1)
        {
            var id = ResolveId(s);
            return new AssetRange(id, id);
        }

        var first = s[..hyphenIndex];
        var dotIndex = first.IndexOf('.', StringComparison.Ordinal);
        if (dotIndex == -1)
            throw new FormatException("Expected '.' in first section of range specifier \"{s}\"");

        var firstId = ResolveId(first);

        var typePart = first[..dotIndex];
        var secondPart = s[(hyphenIndex + 1)..];
        AssetId secondId;
        if (secondPart == "*")
        {
            var (type, _) = _mapping.IdToEnum(firstId);
            secondId = _mapping.MaxIdForType(type);
        }
        else
        {
            var second = $"{typePart}.{secondPart}";
            secondId = ResolveId(second);
        }

        return new AssetRange(firstId, secondId);
    }
}
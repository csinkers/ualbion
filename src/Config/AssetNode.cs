using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using UAlbion.Config.Properties;

namespace UAlbion.Config;

public class AssetNode
{
    readonly AssetId _startOfRangeId;
    readonly AssetNode _parent;
    bool _frozen;

    public AssetNode(AssetId startOfRangeId) => _startOfRangeId = startOfRangeId;
    public AssetNode(AssetNode parent)
    {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _startOfRangeId = parent._startOfRangeId;
    }

    /// <summary>
    /// The set of additional properties that relate to assets in the file
    /// </summary>
    [JsonInclude]
    [JsonExtensionData] 
    public Dictionary<string, object> Properties { get; private set; }

    public int GetIndex(AssetId id)
    {
        if (id.Type != _startOfRangeId.Type)
            throw new InvalidOperationException($"AssetId ({id}) of incorrect type passed to AssetRangeInfo.GetIndex (range {this})");

        return id.Id - _startOfRangeId.Id;
    }

    /// <summary>
    /// Retrieve a property's value
    /// </summary>
    /// <typeparam name="T">The type to interpret the property value as</typeparam>
    /// <param name="assetProperty">The property to retrieve</param>
    /// <returns>The parsed value, or defaultValue if no value existed or could be parsed.</returns>
    public T GetProperty<T>(IAssetProperty<T> assetProperty)
    {
        if (assetProperty == null) throw new ArgumentNullException(nameof(assetProperty));
        return GetProperty(assetProperty, assetProperty.DefaultValue);
    }

    /// <summary>
    /// Retrieve a property's value
    /// </summary>
    /// <typeparam name="T">The type to interpret the property value as</typeparam>
    /// <param name="assetProperty">The property to retrieve</param>
    /// <param name="defaultValue">The default value to use when the property is not set</param>
    /// <returns>The parsed value, or defaultValue if no value existed or could be parsed.</returns>
    public T GetProperty<T>(IAssetProperty<T> assetProperty, T defaultValue)
    {
        if (assetProperty == null) throw new ArgumentNullException(nameof(assetProperty));
        var name = assetProperty.Name;

        object token = null;
        Properties?.TryGetValue(name, out token);

        if (token == null)
        {
            return _parent != null 
                ? _parent.GetProperty(assetProperty) 
                : defaultValue;
        }

        return (T)token;
    }

    /// <summary>
    /// Set's a property by name
    /// </summary>
    /// <param name="assetProperty">The property</param>
    /// <param name="value">The value to set the property to</param>
    /// <param name="typeConfig">The type config</param>
    public void SetProperty(IAssetProperty assetProperty, JsonElement value, TypeConfig typeConfig)
    {
        if (assetProperty == null) throw new ArgumentNullException(nameof(assetProperty));
        if (_frozen) throw new InvalidOperationException("Tried to modify asset node after it was frozen. Asset nodes are immutable after mods have been loaded.");

        try
        {
            Properties ??= new Dictionary<string, object>();
            Properties[assetProperty.Name] = assetProperty.FromJson(value, typeConfig);
        }
        catch (Exception ex)
        {
            var message = $@"Error setting property {assetProperty.Name} to {value} ({assetProperty.GetType().Name}):
  {ex.Message}";
            throw new AssetConfigLoadException(message, ex);
        }
    }

    public void SetProperty<T>(IAssetProperty<T> assetProperty, T value)
    {
        if (assetProperty == null) throw new ArgumentNullException(nameof(assetProperty));
        if (_frozen) throw new InvalidOperationException("Tried to modify asset node after it was frozen. Asset nodes are immutable after mods have been loaded.");
        Properties ??= new Dictionary<string, object>();
        Properties[assetProperty.Name] = value;
    }

    public void SetProperties(IReadOnlyDictionary<string, JsonElement> dict, TypeConfig typeConfig, string contextName, object context)
    {
        if (dict == null) throw new ArgumentNullException(nameof(dict));
        if (typeConfig == null) throw new ArgumentNullException(nameof(typeConfig));

        // The loader and container can change the set of valid properties, so we have to make sure those are set first.
        foreach (var kvp in dict)
        {
            if (kvp.Key != AssetProps.Loader.Name && kvp.Key != AssetProps.Container.Name)
                continue;

            var assetProperty = typeConfig.GetAssetProperty(kvp.Key, this);
            if (assetProperty == null)
                throw new AssetConfigLoadException($"Unexpected property \"{kvp.Key}\" encountered when loading {contextName} \"{context}\" - did not match any registered global property or any exposed by the asset's loader/container");

            SetProperty(assetProperty, kvp.Value, typeConfig);
        }

        foreach (var kvp in dict)
        {
            if (kvp.Key == AssetProps.Loader.Name || kvp.Key == AssetProps.Container.Name)
                continue;

            var assetProperty = typeConfig.GetAssetProperty(kvp.Key, this);
            if (assetProperty == null)
                throw new AssetConfigLoadException($"Unexpected property \"{kvp.Key}\" encountered when loading {contextName} \"{context}\" - did not match any registered global property or any exposed by the asset's loader/container");

            SetProperty(assetProperty, kvp.Value, typeConfig);
        }

        _frozen = true;
    }

    // Asset loading control props
    public string Filename => GetProperty(AssetProps.Filename);
    public string Sha256Hash => GetProperty(AssetProps.Sha256Hash);
    public Type Container => GetProperty(AssetProps.Container); // The alias of the IAssetContainer to use for loading the file
    public Type Loader => GetProperty(AssetProps.Loader); // The alias of the IAssetLoader to use for loading assets in this file
    public Type PostProcessor => GetProperty(AssetProps.Post); // The alias of the IAssetPostProcessor to use for loading assets in this file
    public bool IsReadOnly => GetProperty(AssetProps.IsReadOnly); // To prevent zeroing out files when repacking formats that don't have writing code yet, e.g. ILBM images</summary>

    // Common loader / container specific properties
    public int Width => GetProperty(AssetProps.Width); // The default width, in pixels, of frames in images inside the file. For sprites only.
    public int Height => GetProperty(AssetProps.Height); // The default height, in pixels, of frames in images inside the file. For sprites only.
    public AssetId PaletteId => GetProperty(AssetProps.Palette); // for providing context when exporting 8-bit images to true-colour PNGs
}
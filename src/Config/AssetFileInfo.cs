using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using UAlbion.Api;

namespace UAlbion.Config;

/// <summary>
/// Represents all information about a given file on disk that is required by the asset loading / saving system.
/// </summary>
public class AssetFileInfo
{
    /// <summary>The filename, relative to the mod directory</summary>
    [JsonIgnore] public string Filename { get; set; } // Just mirrors the dictionary key

    /// <summary>The first 32-bits of the file's SHA256 hash (if specified)</summary>
    [JsonIgnore] public string Sha256Hash { get; set; }

    /// <summary>The alias of the IAssetContainer to use for loading the file</summary>
    public string Container { get; set; }

    /// <summary>The alias of the IAssetLoad to use for loading assets in this file</summary>
    public string Loader { get; set; }
    /// <summary>The alias of the IAssetPostProcessor to run on assets in this file after loading</summary>
    public string Post { get; set; }

    /// <summary>The maximum container-index to load from this file.</summary>
    [JsonInclude] public int? Max { get; private set; }

    /// <summary>The mapping from container indices to assets</summary>
    [JsonInclude] public Dictionary<int, AssetInfo> Map { get; private set; } = new();

    /// <summary>Path to an optional JSON file containing the mapping (for cases where the mapping is complicated and would clutter the main assets.json file)</summary>
    public string MapFile { get; set; }

    /// <summary>
    /// The set of additional properties that relate to assets in the file
    /// </summary>
    [JsonInclude]
    [JsonExtensionData] 
    public Dictionary<string, object> Properties { get; private set; }

    /// <summary>
    /// The default width, in pixels, of frames in images inside the file
    /// </summary>
    public int? Width // For sprites only
    {
        get => Get(AssetProperty.Width, (int?)null);
        set => Set(AssetProperty.Width, value);
    }

    /// <summary>
    /// The default height, in pixels, of frames in images inside the file
    /// </summary>
    public int? Height // For sprites only
    {
        get => Get(AssetProperty.Height, (int?)null);
        set => Set(AssetProperty.Height, value);
    }

    internal AssetConfig Config { get; set; }

    public override string ToString() => $"AssetFile: {Filename}{(string.IsNullOrEmpty(Sha256Hash) ? "" : $"#{Sha256Hash}")} ({Map.Count})";

    /// <summary>
    /// Retrieve a property's value by name
    /// </summary>
    /// <typeparam name="T">The type to interpret the property value as</typeparam>
    /// <param name="property">The property name</param>
    /// <param name="defaultValue">The value to use if the property does not exist, or cannot be parsed as the requested type</param>
    /// <returns>The parsed value, or defaultValue if no value existed or could be parsed.</returns>
    public T Get<T>(string property, T defaultValue)
    {
        if (Properties == null || !Properties.TryGetValue(property, out var token))
            return defaultValue;

        if (token is JsonElement elem)
        {
            if (typeof(T) == typeof(string)) return (T)(object)elem.GetString();
            if (typeof(T) == typeof(int)) return (T)(object)elem.GetInt32();
            if (typeof(T) == typeof(long)) return (T)(object)elem.GetInt64();
            if (typeof(T) == typeof(bool)) return (T)(object)elem.GetBoolean();
        }
        //if (token is double asDouble)
        //{
        //    if (typeof(T) == typeof(int))
        //        return (T)(object)Convert.ToInt32(asDouble);

        //    if (typeof(T) == typeof(int?))
        //        return (T)(object)Convert.ToInt32(asDouble);
        //}

        if (typeof(T).IsAssignableFrom(typeof(AssetId)))
        {
            var id = (string)token;
            return CastHelper<AssetId, T>.Cast(ResolveId(id));
        }

        if (typeof(T).IsEnum)
            return (T)Enum.Parse(typeof(T), (string)token);

        return (T)token;
    }

    /// <summary>
    /// Set's a property by name
    /// </summary>
    /// <typeparam name="T">The type of value to set</typeparam>
    /// <param name="property">The property name</param>
    /// <param name="value">The value to set the property to</param>
    public void Set<T>(string property, T value)
    {
        if (value == null)
        {
            if (Properties == null)
                return;

            Properties.Remove(property);
            if (Properties.Count == 0)
                Properties = null;
        }
        else
        {
            Properties ??= new Dictionary<string, object>();
            Properties[property] = value;
        }
    }

    void MergeMapping(Dictionary<int, AssetInfo> mapping)
    {
        foreach (var kvp in mapping)
        {
            if (Map.TryGetValue(kvp.Key, out var info))
            {
                kvp.Value.File = this;
                info.Id ??= kvp.Value.Id;
                if (kvp.Value.Properties != null)
                    foreach (var prop in kvp.Value.Properties)
                        info.Set(prop.Key, prop.Value);
            }
            else
            {
                info = kvp.Value;
                Map[kvp.Key] = info;
            }

            info.Index = kvp.Key;
            info.File = this;
        }
    }

    /// <summary>
    /// Uses the given callbacks to determine which assets are actually present in the file in the current file system.
    /// </summary>
    /// <param name="jsonUtil">The JSON parsing utility</param>
    /// <param name="getSubItemCountForFile">A method which takes an AssetFileInfo and returns the list of container id ranges (offsets + lengths) that the file contains.</param>
    /// <param name="readAllBytesFunc">A method which takes a relative filename and returns the file contents as a byte array</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    public void PopulateAssetIds(
        IJsonUtil jsonUtil,
        Func<AssetFileInfo, IList<(int, int)>> getSubItemCountForFile,
        Func<string, byte[]> readAllBytesFunc)
    {
        if (jsonUtil == null) throw new ArgumentNullException(nameof(jsonUtil));
        if (getSubItemCountForFile == null) throw new ArgumentNullException(nameof(getSubItemCountForFile));
        if (readAllBytesFunc == null) throw new ArgumentNullException(nameof(readAllBytesFunc));

        if (!string.IsNullOrEmpty(MapFile))
        {
            var mappingJson = readAllBytesFunc(MapFile);
            var mapping = jsonUtil.Deserialize<Dictionary<int, AssetInfo>>(mappingJson);
            MergeMapping(mapping);
            MapFile = null;
        }

        AssetInfo last = null;
        var ranges = getSubItemCountForFile(this);
        if (ranges == null)
            return;

        foreach (var asset in Map.Values)
        {
            last ??= asset; // Let last start off as the first mapped info, in case the range doesn't overlap with the mapped ids.
            if (asset.Id == null || !asset.AssetId.IsNone) continue;
            asset.AssetId = ResolveId(asset.Id);
        }

        foreach (var range in ranges)
        {
            for (int i = range.Item1; i < range.Item1 + range.Item2; i++)
            {
                if (last != null && i < last.Index) // Don't add any assets below the mapped range
                    break;

                if (!Map.TryGetValue(i, out var asset))
                {
                    if (last == null)
                        continue;

                    asset = new AssetInfo();
                    Map[i] = asset;
                }

                if(last == null && asset.Id == null)
                    throw new FormatException("The first sub-item in a file's asset mapping must have its Id property set");

                asset.File = this;
                asset.Index = i;
                if (asset.Id == null && last != null)
                    asset.AssetId = new AssetId(last.AssetId.Type, i - last.Index + last.AssetId.Id);
                last = asset;
            }
        }
    }

    internal AssetId ResolveId(string id) => Config.ResolveId(id);
}
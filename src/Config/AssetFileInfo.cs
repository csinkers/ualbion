using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using UAlbion.Api;

namespace UAlbion.Config
{
    public class AssetFileInfo
    {
        [JsonIgnore] public string Filename { get; set; } // Just mirrors the dictionary key
        [JsonIgnore] public string Sha256Hash { get; set; } // Currently only used for MAIN.EXE

        public string Container { get; set; }
        public string Loader { get; set; }
        public string Post { get; set; }
        [JsonInclude] public int? Max { get; private set; }
        [JsonInclude] public Dictionary<int, AssetInfo> Map { get; private set; } = new();
        public string MapFile { get; set; }

        [JsonInclude]
        [JsonExtensionData] 
        public Dictionary<string, object> Properties { get; private set; }

        public int? Width // For sprites only
        {
            get => Get(AssetProperty.Width, (int?)null);
            set => Set(AssetProperty.Width, value);
        }

        public int? Height // For sprites only
        {
            get => Get(AssetProperty.Height, (int?)null);
            set => Set(AssetProperty.Height, value);
        }

        public override string ToString() => $"AssetFile: {Filename} ({Map.Count})";

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
            return (T)token;
        }

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

        public void PopulateAssetIds(
            IJsonUtil jsonUtil,
            Func<string, AssetId> resolveId,
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

            foreach(var asset in Map.Values)
            {
                last ??= asset; // Let last start off as the first mapped info, in case the range doesn't overlap with the mapped ids.
                if (asset.Id == null || !asset.AssetId.IsNone) continue;
                asset.AssetId = resolveId(asset.Id);
            }

            foreach(var range in ranges)
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
    }
}

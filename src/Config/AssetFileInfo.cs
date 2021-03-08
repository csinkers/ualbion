using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Config
{
    public class MappingDictionary : Dictionary<int, AssetInfo> { }
    public class AssetFileInfo
    {
        [JsonIgnore] public string Filename { get; set; } // Just mirrors the dictionary key
        [JsonIgnore] public string Sha256Hash { get; set; } // Currently only used for MAIN.EXE

        public string Container { get; set; }
        public string Loader { get; set; }
        public string Post { get; set; }
        public MappingDictionary Map { get; } = new MappingDictionary();
        public string MapFile { get; set; }
        [JsonExtensionData] public IDictionary<string, JToken> Properties { get; set; }

        [JsonIgnore] public int? Width // For sprites only
        {
            get => Get(AssetProperty.Width, (int?)null);
            set => Set(AssetProperty.Width, value);
        }

        [JsonIgnore]
        public int? Height // For sprites only
        {
            get => Get(AssetProperty.Height, (int?)null);
            set => Set(AssetProperty.Height, value);
        }

        public T Get<T>(string property, T defaultValue)
        {
            if (Properties == null || !Properties.TryGetValue(property, out var token))
                return defaultValue;

            return (T)token.Value<T>();
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
                Properties ??= new Dictionary<string, JToken>();
                Properties[property] = new JValue(value);
            }
        }

        void MergeMapping(MappingDictionary mapping)
        {
            foreach (var kvp in mapping)
            {
                if (Map.TryGetValue(kvp.Key, out var info))
                {
                    kvp.Value.File = this;
                    info.Id ??= kvp.Value.Id;
                    if(kvp.Value.Properties != null)
                    {
                        info.Properties ??= new Dictionary<string, JToken>();
                        foreach(var prop in kvp.Value.Properties)
                            info.Properties[prop.Key] = prop.Value;
                    }
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
            Func<string, AssetId> resolveId,
            Func<AssetFileInfo, IList<(int, int)>> getSubItemCountForFile,
            Func<string, string> readAllTextFunc)
        {
            if (getSubItemCountForFile == null) throw new ArgumentNullException(nameof(getSubItemCountForFile));
            if (readAllTextFunc == null) throw new ArgumentNullException(nameof(readAllTextFunc));

            if (!string.IsNullOrEmpty(MapFile))
            {
                var mappingJson = readAllTextFunc(MapFile);
                var mapping = JsonConvert.DeserializeObject<MappingDictionary>(mappingJson);
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

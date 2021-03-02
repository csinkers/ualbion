﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Config
{
    public class AssetFileInfo
    {
        [JsonIgnore] public string Filename { get; set; } // Just mirrors the dictionary key
        [JsonIgnore] public string Sha256Hash { get; set; } // Currently only used for MAIN.EXE

        public string Container { get; set; }
        public string Loader { get; set; }
        public bool ShouldSerializeLoader() => Loader != null;
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool? Transposed { get; set; }
        public IDictionary<int, AssetInfo> Map { get; } = new Dictionary<int, AssetInfo>();
        [JsonExtensionData] public IDictionary<string, JToken> Properties { get; set; }

        public T Get<T>(string property, T defaultValue)
        {
            if (Properties == null || !Properties.TryGetValue(property, out var token))
                return defaultValue;

            return (T)token.Value<T>();
        }

        // TODO: Text encoding
        public void PopulateAssetIds(
            Func<string, AssetId> resolveId,
            Func<AssetFileInfo, IList<(int, int)>> getSubItemCountForFile)
        {
            if (getSubItemCountForFile == null) throw new ArgumentNullException(nameof(getSubItemCountForFile));

            AssetInfo last = null;
            var ranges = getSubItemCountForFile(this);
            if (ranges == null)
                return;

            foreach(var asset in Map.Values)
            {
                if (asset.Id == null || !asset.AssetId.IsNone) continue;
                asset.AssetId = resolveId(asset.Id);
            }

            foreach(var range in ranges)
            {
                for (int i = range.Item1; i < range.Item1 + range.Item2; i++)
                {
                    if (!Map.TryGetValue(i, out var asset))
                    {
                        if (last == null)
                            continue;

                        asset = new AssetInfo
                        {
                            Width = last.Width,
                            Height = last.Height
                        };
                        Map[i] = asset;
                    }

                    if(last == null && asset.Id == null)
                        throw new FormatException("The first sub-item in a file's asset mapping must have its Id property set");

                    asset.File = this;
                    asset.SubAssetId = i;
                    if (asset.Id == null && last != null)
                        asset.AssetId = new AssetId(last.AssetId.Type, i - last.SubAssetId + last.AssetId.Id);
                    last = asset;
                }
            }
        }
    }
}

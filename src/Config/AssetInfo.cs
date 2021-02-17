using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#pragma warning disable CA2227 // Collection properties should be read only
namespace UAlbion.Config
{
    public class AssetInfo
    {
        public string Id { get; set; } // Id of this asset in the mapped enum type.
        [JsonExtensionData] public IDictionary<string, JToken> Properties { get; set; }

        [JsonIgnore] public AssetId AssetId { get; set; }
        [JsonIgnore] public int SubAssetId { get; set; } // Sub-asset offset in the container file (or 0 if not inside a container)
        [JsonIgnore] public AssetFileInfo File { get; set; }

        [JsonIgnore] public bool Transposed => File?.Transposed ?? false; // For sprites only
        [JsonIgnore] public int Width // For sprites only
        {
            get => Get(nameof(Width), File?.Width ?? 0);
            set => Set(nameof(Width), value == 0 ? (object)null : value);
        }

        [JsonIgnore] public int Height // For sprites only
        {
            get => Get(nameof(Height), File?.Height ?? 0);
            set => Set(nameof(Height), value == 0 ? (object)null : value);
        }

        [JsonIgnore] public string Name
        {
            get => Get<string>(nameof(Name), null);
            set => Set(nameof(Name), value);
        }

        public override string ToString() => $"I:{AssetId} ({File.Filename}.{SubAssetId})";

        public T Get<T>(string propertyName, T defaultValue)
        {
            if (Properties == null || !Properties.TryGetValue(propertyName, out var token))
                return defaultValue;

            return token.Value<T>();
        }

        public void Set<T>(string propertyName, T value)
        {
            if (value == null)
            {
                if (Properties == null)
                    return;

                Properties.Remove(propertyName);
                if (Properties.Count == 0)
                    Properties = null;
            }
            else
            {
                Properties ??= new Dictionary<string, JToken>();
                Properties[propertyName] = new JValue(value);
            }
        }

        public T[] GetArray<T>(string propertyName)
        {
            if (Properties == null || !Properties.TryGetValue(propertyName, out var token))
                return null;

            if (token is JArray array)
                return array.Select(x => x.Value<T>()).ToArray();

            return null;
        }

        public JToken GetRaw(string propertyName) => Properties != null && Properties.TryGetValue(propertyName, out var token) ? token : null;
    }
}
#pragma warning restore CA2227 // Collection properties should be read only

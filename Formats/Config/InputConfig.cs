using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public class InputConfig
    {
        public IDictionary<InputMode, IDictionary<string, string>> Bindings { get; private set; }
        readonly string _basePath;

        public InputConfig(string basePath)
        {
            _basePath = basePath;
        }

        public static InputConfig Load(string basePath)
        {
            var inputConfig = new InputConfig(basePath);
            var configPath = Path.Combine(basePath, "data", "input.json");
            if (File.Exists(configPath))
            {
                var configText = File.ReadAllText(configPath);
                inputConfig.Bindings = JsonConvert.DeserializeObject<IDictionary<InputMode, IDictionary<string, string>>>(configText);
            }

            return inputConfig;
        }

        public void Save()
        {
            var configPath = Path.Combine(_basePath, "data", "input.json");
            var serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(this, serializerSettings);
            File.WriteAllText(configPath, json);
        }
    }
}

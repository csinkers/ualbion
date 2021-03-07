using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UAlbion.Api;
using UAlbion.Config;

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

        public static InputConfig Load(string basePath, IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            var inputConfig = new InputConfig(basePath);
            var configPath = Path.Combine(basePath, "data", "input.json");
            if (disk.FileExists(configPath))
            {
                var configText = disk.ReadAllText(configPath);
                inputConfig.Bindings = JsonConvert.DeserializeObject<IDictionary<InputMode, IDictionary<string, string>>>(configText);
            }

            return inputConfig;
        }

        public void Save(IFileSystem disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));
            var configPath = Path.Combine(_basePath, "data", "input.json");
            var json = JsonConvert.SerializeObject(this, ConfigUtil.JsonSerializerSettings);
            disk.WriteAllText(configPath, json);
        }
    }
}

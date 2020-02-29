using System.IO;
using Newtonsoft.Json;

namespace UAlbion.Formats.Config
{
    public class GeneralConfig : IGeneralConfig
    {
        public const string Filename = "config.json";
        [JsonIgnore] public string BasePath { get; set; }
        [JsonIgnore] public string BaseDataPath => Path.Combine(BasePath, "data");
        public string XldPath { get; set; }
        public string ExePath { get; set; }
        public string SavePath { get; set; }
        public string ExportedXldPath { get; set; }

        public static GeneralConfig Load(string basePath)
        {
            var configPath = Path.Combine(basePath, "data", Filename);
            GeneralConfig config;
            if (File.Exists(configPath))
            {
                var configText = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<GeneralConfig>(configText);
            }
            else
            {
                config = new GeneralConfig
                {
                    XldPath = @"albion/CD/XLDLIBS",
                    ExportedXldPath = @"exported"
                };
            }

            config.BasePath = basePath;
            return config;
        }
    }
}